using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.S3;
using Amazon.CDK.AWS.SNS;
using Amazon.CDK.AWS.SQS;
using Amazon.CDK.AWS.StepFunctions;
using Amazon.CDK.AWS.Events;
using System.Collections.Generic;
using Amazon.CDK.AWS.ElasticLoadBalancingV2;
using Amazon.CDK.AWS.Events.Targets;
using Amazon.CDK.AWS.IAM;
using StepFunctionTasks = Amazon.CDK.AWS.StepFunctions.Tasks;
using Amazon.CDK.AWS.StepFunctions.Tasks;
using Amazon.CDK.AWS.Logs;
using Amazon.CDK.AWS.EC2;

namespace ServerlessDocProcessing;

public class ServerlessDocProcessingStack : Stack
{
    CustomFunctionFactory FunctionFactory { get; }

    public string EnvironmentName { get; set; }

    internal ServerlessDocProcessingStack(Construct scope, string id, ServerlessDocProcessingStackProps props = null) : base(scope, id, props)
    {
        EnvironmentName = props.EnvironmentName;

        FunctionFactory = new(this, EnvironmentName);

        // Functions
        var initializeFunction = FunctionFactory.CreateCustomFunction("InitializeProcessing");
        

        // Tables
        Table configTable = new(this, "configTable", new TableProps
        {
            TableName = GetTableName("config"),
            PartitionKey = new Amazon.CDK.AWS.DynamoDB.Attribute() { Name = "query", Type = AttributeType.STRING },
            BillingMode = BillingMode.PAY_PER_REQUEST,
            RemovalPolicy = RemovalPolicy.DESTROY
        });

        Table dataTable = new(this, "dataTable", new TableProps
        {
            TableName = GetTableName("data"),
            PartitionKey = new Amazon.CDK.AWS.DynamoDB.Attribute() { Name = "id", Type = AttributeType.STRING },
            BillingMode = BillingMode.PAY_PER_REQUEST,
            RemovalPolicy = RemovalPolicy.DESTROY
        });

        // Buckets
        Bucket inputBucket = new(this, "inputBucket", new BucketProps
        {
            Encryption = BucketEncryption.S3_MANAGED,
            BucketName = GetBucketName("input-bucket"),
            EventBridgeEnabled = true,
            RemovalPolicy = RemovalPolicy.DESTROY,
        });

        Bucket extractedDataBucket = new(this, "extractedData", new BucketProps
        {
            Encryption = BucketEncryption.S3_MANAGED,
            BucketName = GetBucketName("extracted-data-bucket"),
            RemovalPolicy = RemovalPolicy.DESTROY
        });

        // Messaging (SQS / SNS / Event Bridge)
        Queue successQueue = new(this, "successQueue", new QueueProps
        {
            QueueName = GetQueueName("successQueue")
        });

        Queue failureQueue = new(this, "failureQueue", new QueueProps
        {
            QueueName = GetQueueName("failureQueue"),

        });

        Topic textractSuccessTopic = new(this, "textractSuccessTopic", new TopicProps
        {
            Fifo = false,
            TopicName = GetTopicname("TextractSuccess"),
            DisplayName = "Textract Success Topic"
        });

        Topic textractFailureTopic = new(this, "textractFailureTopic", new TopicProps
        {
            Fifo = false,
            TopicName = GetTopicname("TextractFailure"),
            DisplayName = "Textract Failure Topic"
        });






        // Logging
        LogGroup stepFunctionLogGroup = new(this, "stepFunctionLogGroup", new Amazon.CDK.AWS.Logs.LogGroupProps
        {
            LogGroupName = GetLogGroupName("docProcessingWorkflow"),
            RemovalPolicy= RemovalPolicy.DESTROY
        });


        // StepFunction Tasks
        Pass nullState = new(this, "nullState", new PassProps
        {
        });

        StepFunctionTasks.LambdaInvoke initializeState = new(this, "initializeState", new LambdaInvokeProps
        {
            LambdaFunction = initializeFunction,
            Comment = "Initializes the Document Processing Workflow",
            InputPath = "$.detail"
        });
        



        StateMachine docProcessingStepFunction = new(this, "docProcessing", new StateMachineProps
        {
            Logs = new LogOptions
            { 
                Destination = stepFunctionLogGroup,
                IncludeExecutionData = true,
                Level = LogLevel.ALL
            },
            Definition = initializeState
        });


        // EventBridge
        Rule rule = new(this, "inputBucketRule", new RuleProps
        {
            Enabled = true,
            RuleName = "InputBucketRule",
            EventPattern = new EventPattern
            {

                Source = new[] { "aws.s3" },
                DetailType = new[] { "Object Created" },
                Detail = new Dictionary<string, object> {
                     {
                         "bucket", new Dictionary<string, object> { {"name", new [] {inputBucket.BucketName } }
                     } },
            }}
        });

        Queue inputDlq = new(this, "inputDlq", new QueueProps 
        {
            Encryption = QueueEncryption.SQS_MANAGED,
            EnforceSSL = true,
        });

        Role eventRole = new Role(this, "inputEventRole", new RoleProps
        {
            AssumedBy = new ServicePrincipal("events.amazonaws.com")
        });


        rule.AddTarget(new SfnStateMachine(docProcessingStepFunction, new SfnStateMachineProps
        {
            DeadLetterQueue = inputDlq,
            RetryAttempts = 3,
            Role = eventRole
        }));

        docProcessingStepFunction.GrantStartExecution(eventRole);
        stepFunctionLogGroup.GrantWrite(docProcessingStepFunction);



        // Outputs
        new CfnOutput(this, "inputBucket", new CfnOutputProps
        {
            Description = "Input Bucket",
            Value = inputBucket.BucketName
        });


    }

    // Functions to create unique names
    private string GetBucketName(string baseName) => $"{baseName}-{EnvironmentName}-{Account}";

    private string GetQueueName(string baseName) => $"{baseName}-{EnvironmentName}-{Account}";

    private string GetTopicname(string baseName) => $"{baseName}-{EnvironmentName}-{Account}";

    private string GetTableName(string baseName) => $"{EnvironmentName}-{baseName}";

    private string GetLogGroupName(string baseName) => $"lg{StackName}-{EnvironmentName}-{baseName}-{Account}";
}
