﻿using Amazon.CDK.AWS.StepFunctions;
using Amazon.CDK.AWS.StepFunctions.Tasks;
using System.Collections.Generic;

namespace ServerlessDocProcessing.Constructs
{
    internal class DocProcessingStepFunction : Construct
    {
        public StateMachine StateMachine { get; init; }

        
        public DocProcessingStepFunction(Construct scope, string id, DocProcessingStepFunctionProps props) 
            : base(scope, id)
        {
            // Step Functions Tasks
            LambdaInvoke initializeState = new(this, "initializeState", new LambdaInvokeProps
            {
                LambdaFunction = props.InitializeFunction,
                Comment = "Initializes the Document Processing Workflow",
                OutputPath = "$.Payload",
                Payload = TaskInput.FromJsonPathAt("$"),
            });


            LambdaInvoke textractState = new(this, "textractState", new LambdaInvokeProps
            {
                IntegrationPattern = IntegrationPattern.WAIT_FOR_TASK_TOKEN,
                TaskTimeout = Timeout.Duration(Duration.Seconds(StepFunctionDefaults.TEXTRACT_STEP_TIME_OUT)),
                LambdaFunction = props.SubmitToTextractFunction,
                Comment = "Function to send document to textract asynchronously",
                Payload = TaskInput.FromObject(new Dictionary<string, object> {
                { "id", JsonPath.StringAt("$.id") },
                { "taskToken", JsonPath.TaskToken}
                })
            });
 
            LambdaInvoke processTextractResultsState = new(this, "processTextractResults", new LambdaInvokeProps
            {
                LambdaFunction = props.ProcessTextractResultFunction,
                Comment = "Function to process textract results asynchronously",
                OutputPath = "$.Payload",
            });

            SqsSendMessage sendFailureState = new(this, "sendFailureState", new SqsSendMessageProps
            {
                Queue = props.SendFailureQueue,
                Comment = "Send Failure Message",
                MessageBody = TaskInput.FromJsonPathAt("$"),
            });

            SqsSendMessage sendSuccessState = new(this, "sendSuccessState", new SqsSendMessageProps
            {
                Queue = props.SendSuccessQueue,
                Comment = "Send Success Message",
                MessageBody = TaskInput.FromJsonPathAt("$")
            });

            // Compose the workflow sequence
            initializeState.Next(textractState);
            initializeState.AddCatch(sendFailureState, new CatchProps
            {
                Errors = new[] { "States.ALL" },
                ResultPath = "$.error"
            });
            textractState.Next(processTextractResultsState);
            textractState.AddCatch(sendFailureState, new CatchProps
            {
                Errors = new[] { "States.ALL" },
                ResultPath = "$.error"
            });

            processTextractResultsState.Next(sendSuccessState);
            processTextractResultsState.AddCatch(sendFailureState, new CatchProps
            {
                Errors = new[] { "States.ALL" },
                ResultPath = "$.error"
            });

            StateMachine = new StateMachine (this, $"{id}StateMachine", new StateMachineProps
            { 
                DefinitionBody = DefinitionBody.FromChainable(initializeState),
            });
         }
    }
}