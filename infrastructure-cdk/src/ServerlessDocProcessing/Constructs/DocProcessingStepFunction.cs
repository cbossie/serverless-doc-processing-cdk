using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.StepFunctions;
using Amazon.CDK.AWS.StepFunctions.Tasks;
using System.Collections.Generic;

namespace ServerlessDocProcessing.Constructs
{
    internal class DocProcessingStepFunction : Construct
    {
        public StateMachine StateMachine { get; init; }

        public IRole Role => StateMachine?.Role;

        public DocProcessingStepFunction(Construct scope, string id, DocProcessingStepFunctionProps props)
            : base(scope, id)
        {
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
                    { "execution", JsonPath.ExecutionName },
                    { "error", JsonPath.StringAt("$.error.Error") },
                    { "cause", JsonPath.StringAt("$.error.Cause") },
                })
            });

            LambdaInvoke sendFailure = new(this, "sendFailure", new LambdaInvokeProps
            {
                LambdaFunction = props.SuccessFunction,
                Comment = "Sends a failure message to the SQS Queue",
                OutputPath = "$.Payload",
                Payload = TaskInput.FromObject(new Dictionary<string, object> {
                    { "id", JsonPath.StringAt("$.id") },
                })
            });

            // Standard Textract Analysis
            LambdaInvoke textractState = new(this, "textractState", new LambdaInvokeProps
            {
                IntegrationPattern = IntegrationPattern.WAIT_FOR_TASK_TOKEN,
                TaskTimeout = Timeout.Duration(Duration.Seconds(StepFunctionDefaults.TEXTRACT_STEP_TIME_OUT)),
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
                TaskTimeout = Timeout.Duration(Duration.Seconds(StepFunctionDefaults.TEXTRACT_STEP_TIME_OUT)),
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
            processTextractExpenseResultsState.AddCatch(sendFailureQueue, new CatchProps
            {
                Errors = new[] { "States.ALL" },
                ResultPath = "$.error"
            });

            sendFailure.Next(sendFailureQueue);


            // Update the step function body and use the passed in props
            props.DefinitionBody = DefinitionBody.FromChainable(initializeState);
            StateMachine = new StateMachine(this, $"{id}StateMachine", props);
        }
    }
}
