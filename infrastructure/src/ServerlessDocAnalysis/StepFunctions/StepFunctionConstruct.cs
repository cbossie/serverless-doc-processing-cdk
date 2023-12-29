﻿using Amazon.CDK.AWS.Events.Targets;
using Amazon.CDK.AWS.Logs;
using Amazon.CDK.AWS.StepFunctions;
using Amazon.CDK.AWS.StepFunctions.Tasks;
using System.Collections.Generic;

namespace ServerlessDocProcessing.StepFunctions;

public class StepFunctionConstruct : Construct
{
    public StateMachine StateMachine { get; init; }
    private string EnvironmentName { get; }
    public string ResourceNamePrefix { get; }


    public StepFunctionConstruct(Construct scope, string id, StepFunctionProps props)
        : base(scope, id)
    {
        EnvironmentName = props.EnvironmentName;
        ResourceNamePrefix = props.ResourceNamePrefix;

        // Step Functions Tasks
        LambdaInvoke initializeState = new(this, "initializeState", new LambdaInvokeProps
        {
            LambdaFunction = props.InitializeFunction,
            Comment = "Initializes the Document Processing Workflow",
            OutputPath = "$.Payload",
            Payload = TaskInput.FromObject(new Dictionary<string, object>
            {
                { "ExecutionId", JsonPath.ExecutionName },
                { "Event", JsonPath.EntirePayload }
            })
        });

        LambdaInvoke sendSuccess = new(this, "sendSuccess", new LambdaInvokeProps
        {
            LambdaFunction = props.SuccessFunction,
            Comment = "Sends a success message to the SQS Queue",
            OutputPath = "$.Payload",
            Payload = TaskInput.FromObject(new Dictionary<string, object> {
                { "id", JsonPath.StringAt("$.id") },
            })

        });

        LambdaInvoke sendFailure = new(this, "sendFailure", new LambdaInvokeProps
        {
            LambdaFunction = props.FailureFunction,
            Comment = "Sends a failure message to the SQS Queue",
            OutputPath = "$.Payload",
            Payload = TaskInput.FromObject(new Dictionary<string, object> {
                { "execution", JsonPath.ExecutionName },
                { "error", JsonPath.StringAt("$.error.Error") },
                { "cause", JsonPath.StringAt("$.error.Cause") },
            })
        });

        // Standard Textract Analysis
        LambdaInvoke textractState = new(this, "textractState", new LambdaInvokeProps
        {
            IntegrationPattern = IntegrationPattern.WAIT_FOR_TASK_TOKEN,
            TaskTimeout = Timeout.Duration(Duration.Seconds(600)),

            LambdaFunction = props.SubmitToTextractFunction,
            Comment = "Function to send document to textract asynchronously for query analysis",
            Payload = TaskInput.FromObject(new Dictionary<string, object> {
            { "id", JsonPath.StringAt("$.id") },
            { "taskToken", JsonPath.TaskToken}
            })
        });

        LambdaInvoke processTextractResultsState = new(this, "processTextractQueryResults", new LambdaInvokeProps
        {
            LambdaFunction = props.ProcessTextractQueryFunction,
            Comment = "Function to process textract query results asynchronously",
            OutputPath = "$.Payload",
        });

        // Expense Textract Analysis
        LambdaInvoke textractExpenseState = new(this, "textractExpenseState", new LambdaInvokeProps
        {
            IntegrationPattern = IntegrationPattern.WAIT_FOR_TASK_TOKEN,
            TaskTimeout = Timeout.Duration(Duration.Seconds(600)),
            LambdaFunction = props.SubmitToTextractExpenseFunction,
            Comment = "Function to send document to textract asynchronously for expense analysis",
            Payload = TaskInput.FromObject(new Dictionary<string, object> {
            { "id", JsonPath.StringAt("$.id") },
            { "taskToken", JsonPath.TaskToken}
            })
        });

        LambdaInvoke processTextractExpenseResultsState = new(this, "processTextractExpenseResults", new LambdaInvokeProps
        {
            LambdaFunction = props.ProcessTextractExpenseFunction,
            Comment = "Function to process textract Expense results asynchronously",
            OutputPath = "$.Payload",
        });

        SqsSendMessage sendFailureQueue = new(this, "sendFailureQueue", new SqsSendMessageProps
        {
            Queue = props.SendFailureQueue,
            Comment = "Send Failure Message",
            MessageBody = TaskInput.FromJsonPathAt("$"),
        });

        SqsSendMessage sendSuccessQueue = new(this, "sendSuccessQueue", new SqsSendMessageProps
        {
            Queue = props.SendSuccessQueue,
            Comment = "Send Success Message",
            MessageBody = TaskInput.FromJsonPathAt("$")
        });

        // Compose the workflow sequence
        initializeState.Next(textractState);
        initializeState.AddCatch(sendFailure, new CatchProps
        {
            Errors = new[] { "States.ALL" },
            ResultPath = "$.error"
        });
        textractState.Next(processTextractResultsState);
        textractState.AddCatch(sendFailure, new CatchProps
        {
            Errors = new[] { "States.ALL" },
            ResultPath = "$.error"
        });

        processTextractResultsState.Next(textractExpenseState);
        processTextractResultsState.AddCatch(sendFailure, new CatchProps
        {
            Errors = new[] { "States.ALL" },
            ResultPath = "$.error"
        });

        textractExpenseState.Next(processTextractExpenseResultsState);
        textractExpenseState.AddCatch(sendFailure, new CatchProps
        {
            Errors = new[] { "States.ALL" },
            ResultPath = "$.error"
        });

        processTextractExpenseResultsState.Next(sendSuccess);
        processTextractExpenseResultsState.AddCatch(sendFailure, new CatchProps
        {
            Errors = new[] { "States.ALL" },
            ResultPath = "$.error"
        });

        sendSuccess.Next(sendSuccessQueue);

        sendFailure.Next(sendFailureQueue);


        // Log group for the step function
        LogGroup stepFunctionLogGroup = new(this, "stepFunctionLogGroup", new Amazon.CDK.AWS.Logs.LogGroupProps
        {
            LogGroupName = GetLogGroupName("docProcessingWorkflow"),
            RemovalPolicy = RemovalPolicy.DESTROY
        });

        // Update the step function body and use the passed in props
        StateMachine = new StateMachine(this, $"{id}StateMachine", new StateMachineProps
        {
            StateMachineName = GetStateMachineName("stateMachine"),
            DefinitionBody = DefinitionBody.FromChainable(initializeState),
            Comment = "State Machine used to process uploaded PDFs and retrieve Query/Expense data.",
            TracingEnabled = true,
            StateMachineType = StateMachineType.STANDARD,
            Logs = new LogOptions
            {
                Destination = stepFunctionLogGroup,
                IncludeExecutionData = true,
                Level = LogLevel.ALL
            }
        });

        // Allow the state machine to write to CloudWatch Logs
        stepFunctionLogGroup.GrantWrite(StateMachine);

        // Start the Function state machine from EventBridge
        props.EventBridgeRule.AddTarget(new SfnStateMachine(StateMachine, new SfnStateMachineProps
        {
            DeadLetterQueue = props.DeadLetterQueue,
            RetryAttempts = 3,
            Role = props.EventBridgeRole
        }));
        StateMachine.GrantStartExecution(props.EventBridgeRole);
    }

    private string GetStateMachineName(string baseName) => $"{ResourceNamePrefix}-{baseName}-{EnvironmentName}";
    private string GetLogGroupName(string baseName) => $"{ResourceNamePrefix}-{baseName}-{EnvironmentName}";

}

