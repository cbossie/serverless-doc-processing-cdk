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
using System.Diagnostics.Contracts;

namespace ServerlessDocProcessing;

public class ServerlessDocProcessingStack : Stack
{
    CustomFunctionFactory FunctionFactory { get; }

    public string EnvironmentName { get; set; }

    internal ServerlessDocProcessingStack(Construct scope, string id, ServerlessDocProcessingStackProps props = null) : base(scope, id, props)
    {
        EnvironmentName = props.EnvironmentName;

        // Function Factory
        FunctionFactory = new(this, EnvironmentName);
        FunctionFactory.Timeout = 30;
        FunctionFactory.Memory = 512;
        FunctionFactory.AddEnvironmentVariable("POWERTOOLS_SERVICE_NAME", $"docprocessing-{EnvironmentName}");
        FunctionFactory.AddEnvironmentVariable("POWERTOOLS_LOG_LEVEL", $"Debug");
        FunctionFactory.AddEnvironmentVariable("POWERTOOLS_LOGGER_CASE", $"SnakeCase");
        FunctionFactory.AddEnvironmentVariable("POWERTOOLS_LOGGER_LOG_EVENT", $"true");
        FunctionFactory.AddEnvironmentVariable("POWERTOOLS_LOGGER_SAMPLE_RATE", $"0");
        FunctionFactory.AddEnvironmentVariable("POWERTOOLS_TRACE_DISABLED", $"false");
        FunctionFactory.AddEnvironmentVariable("POWERTOOLS_TRACER_CAPTURE_RESPONSE", $"true");
        FunctionFactory.AddEnvironmentVariable("POWERTOOLS_TRACER_CAPTURE_ERROR", $"true");


        // Tables
        Table configTable = new(this, "queryData", new TableProps
        {
            TableName = GetTableName("QueryData"),
            PartitionKey = new Amazon.CDK.AWS.DynamoDB.Attribute() { Name = "query", Type = AttributeType.STRING },
            BillingMode = BillingMode.PAY_PER_REQUEST,
            RemovalPolicy = RemovalPolicy.DESTROY
        });

        Table dataTable = new(this, "dataTable", new TableProps
        {
            TableName = GetTableName("ProcessData"),
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
            RemovalPolicy = RemovalPolicy.DESTROY
        });


        // Functions
        var initializeFunction = FunctionFactory.CreateCustomFunction("InitializeProcessing")
            .AddEnvironment("POWERTOOLS_METRICS_NAMESPACE", $"InitializeProcessing");

        var textractFunction = FunctionFactory.CreateCustomFunction("SubmitToTextract")
            .AddEnvironment("SUCCESS_TOPIC", textractSuccessTopic.TopicArn)
            .AddEnvironment("FAIL_TOPIC", textractFailureTopic.TopicArn)
            .AddEnvironment("POWERTOOLS_METRICS_NAMESPACE", $"SubmitToTextract");



        // Step Functions Tasks
        StepFunctionTasks.LambdaInvoke initializeState = new(this, "initializeState", new LambdaInvokeProps
        {
            LambdaFunction = initializeFunction,
            Comment = "Initializes the Document Processing Workflow",
            OutputPath = "$.Payload",
            Payload = TaskInput.FromJsonPathAt("$"),
        });

        StepFunctionTasks.LambdaInvoke textractState = new(this, "textractState", new LambdaInvokeProps
        {
            LambdaFunction = textractFunction,
            Comment = "Function to send document to textract asynchronously",
            OutputPath = "$.Payload",
            Payload = TaskInput.FromJsonPathAt("$"),
        });

        StepFunctionTasks.SqsSendMessage sendFailureState = new(this, "sendFailureState", new SqsSendMessageProps
        {
            Queue = failureQueue,
            Comment = "Send Failure Message",
            MessageBody = TaskInput.FromJsonPathAt("$"),
        });



        StepFunctionTasks.SqsSendMessage sendSuccessState = new(this, "sendSuccessState", new SqsSendMessageProps
        {
            Queue = successQueue,
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
        textractState.Next(sendSuccessState);
        textractState.AddCatch(sendFailureState, new CatchProps
        {
            Errors = new[] { "States.ALL" },
            ResultPath = "$.error"
        });


        StateMachine docProcessingStepFunction = new(this, "docProcessing", new StateMachineProps
        {
            TracingEnabled = true,
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
            }
            }
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
            Role = eventRole,
        }));


        //Assign permissions
        docProcessingStepFunction.GrantStartExecution(eventRole);
        stepFunctionLogGroup.GrantWrite(docProcessingStepFunction);
        inputBucket.GrantReadWrite(initializeFunction);
        configTable.GrantDocumentObjectModelPermissions(initializeFunction);
        dataTable.GrantDocumentObjectModelPermissions(initializeFunction);
        dataTable.GrantDocumentObjectModelPermissions(textractFunction);

        textractFunction.Role.AddManagedPolicy(ManagedPolicy.FromAwsManagedPolicyName("AmazonTextractFullAccess"));
        
        // Outputs
        new CfnOutput(this, "inputBucketOutput", new CfnOutputProps
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

    private string GetLogGroupName(string baseName) => $"lg-{EnvironmentName}-{baseName}-{Account}";
}

