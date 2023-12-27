using Amazon.CDK.AWS.CloudWatch;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.Events;
using Amazon.CDK.AWS.Events.Targets;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Logs;
using Amazon.CDK.AWS.S3;
using Amazon.CDK.AWS.SES.Actions;
using Amazon.CDK.AWS.SNS;
using Amazon.CDK.AWS.SNS.Subscriptions;
using Amazon.CDK.AWS.SQS;
using Amazon.CDK.AWS.StepFunctions;
using ServerlessDocProcessing.Infrastructure;
using ServerlessDocProcessing.Lambda;
using ServerlessDocProcessing.StepFunctions;
using System.Collections.Generic;

namespace ServerlessDocProcessing;

public class DocAnalysisStack : Stack
{
    public string EnvironmentName { get; set; }

    internal DocAnalysisStack(Construct scope, string id, DocAnalysisStackProps props = null)
        : base(scope, id, props)
    {
        /// ===================================
        /// Base infrastructure
        /// ===================================
        InfrastructureConstruct infrastructure = new(this, "Infrastructure", new InfrastructureProps
        {
            EnvironmentName = props.EnvironmentName,
            ResourceNamePrefix = props.ResourceNamePrefix
        });

        /// ===================================
        /// Lambda Functions
        /// ===================================
        FunctionCollectionConstruct functions = new(this, "Functions", new FunctionCollectionProps
        {
            EnvironmentName = props.EnvironmentName,
            ResourceNamePrefix = props.ResourceNamePrefix,
            FunctionCodeBaseDirectory = props.FunctionCodeBaseDirectory,            
            TextractBucket = infrastructure.TextractBucket,
            TextractTopic = infrastructure.TextractTopic,
            TextractRole = infrastructure.TextractRole,
            InputBucket = infrastructure.InputBucket,
            ProcessDataTable = infrastructure.DataTable,
            QueryConfigTable = infrastructure.ConfigTable
        });

        /// ===================================
        /// Step Function
        /// ===================================
        _ = new StepFunctionConstruct(this, "StepFunction", new StepFunctionProps
        {
            EnvironmentName = props.EnvironmentName,
            ResourceNamePrefix = props.ResourceNamePrefix,
            InitializeFunction = functions.InitializeFunction,
            SuccessFunction = functions.SuccessFunction,
            FailureFunction = functions.FailureFunction,
            SubmitToTextractFunction = functions.SubmitToTextractFunction,
            SubmitToTextractExpenseFunction = functions.SubmitToTextractExpenseFunction,
            ProcessTextractQueryFunction = functions.ProcessTextractQueryResultFunction,
            ProcessTextractExpenseFunction = functions.ProcessTextractExpenseResultFunction,
            SendFailureQueue = infrastructure.FailureQueue,
            SendSuccessQueue = infrastructure.SuccessQueue,
            EventBridgeRole = infrastructure.EventRole,
            DeadLetterQueue = infrastructure.InputDLQ,
            EventBridgeRule = infrastructure.InputBucketRule
        });

        // Grant permissions to functions to use Object Persistence Model in DynamoDB
        //InfrastructureStack.ConfigTable.GrantDocumentObjectModelPermissions(FunctionStack.InitializeFunction);
        //InfrastructureStack.DataTable.GrantDocumentObjectModelPermissions(FunctionStack.InitializeFunction);
        //InfrastructureStack.DataTable.GrantDocumentObjectModelPermissions(FunctionStack.SubmitToTextractFunction);
        //InfrastructureStack.DataTable.GrantDocumentObjectModelPermissions(FunctionStack.SubmitToTextractExpenseFunction);
        //InfrastructureStack.DataTable.GrantDocumentObjectModelPermissions(FunctionStack.RestartStepFunction);
        //InfrastructureStack.DataTable.GrantDocumentObjectModelPermissions(FunctionStack.ProcessTextractQueryResultFunction);
        //InfrastructureStack.DataTable.GrantDocumentObjectModelPermissions(FunctionStack.ProcessTextractExpenseResultFunction);
        //InfrastructureStack.DataTable.GrantDocumentObjectModelPermissions(FunctionStack.SuccessFunction);
        //InfrastructureStack.DataTable.GrantDocumentObjectModelPermissions(FunctionStack.FailureFunction);

        /// ===================================
        /// Outputs
        /// ===================================
        _ = new CfnOutput(this, "inputBucketOutput", new CfnOutputProps
        {
            Description = "Input Bucket",
            Value = infrastructure.InputBucket.BucketName
        });

        _ = new CfnOutput(this, "inputBucketPolicyOutput", new CfnOutputProps
        {
            Description = "Example policy to add that will give you access to the input bucket",
            Value = $@"
            {{
			    ""Sid"": ""InputBucketStatement"",
			    ""Effect"": ""Allow"",
			    ""Action"": ""s3:PutObject"",
			    ""Resource"": ""{infrastructure.InputBucket.BucketArn}""
            }}            
            "
        });

        _ = new CfnOutput(this, "successQueueOutput", new CfnOutputProps
        {
            Description = "Queue where success messages are sent",
            Value = infrastructure.SuccessQueue.QueueUrl
        });

        _ = new CfnOutput(this, "successQueuePolicyOutput", new CfnOutputProps
        {
            Description = "Example policy to add that will give you access to the success queue",
            Value = $@"
            {{
            	""Sid"": ""SuccessQueueStatement"",
            	""Effect"": ""Allow"",
            	""Action"": [
            		""sqs:DeleteMessage"",
                    ""sqs:ReceiveMessage""
            	],
            	""Resource"": ""{infrastructure.SuccessQueue.QueueArn}""
            }}            
            "
        });

        _ = new CfnOutput(this, "failureQueueOutput", new CfnOutputProps
        {
            Description = "Queue where failure messages are sent",
            Value = infrastructure.SuccessQueue.QueueUrl
        });

        _ = new CfnOutput(this, "failQueuePolicyOutput", new CfnOutputProps
        {
            Description = "Example policy to add that will give you access to the failure queue",
            Value = $@"
            {{
            	""Sid"": ""FailQueueStatement"",
            	""Effect"": ""Allow"",
            	""Action"": [
            		""sqs:DeleteMessage"",
                    ""sqs:ReceiveMessage""
            	],
            	""Resource"": ""{infrastructure.FailureQueue.QueueArn}""
            }}            
            "
        });

    }


    


    
}
