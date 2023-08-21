using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.Events;
using Amazon.CDK.AWS.Events.Targets;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Logs;
using Amazon.CDK.AWS.S3;
using Amazon.CDK.AWS.SNS;
using Amazon.CDK.AWS.SNS.Subscriptions;
using Amazon.CDK.AWS.SQS;
using Amazon.CDK.AWS.StepFunctions;
using Amazon.CDK.AWS.StepFunctions.Tasks;
using Constants;
using System.Collections.Generic;

namespace ServerlessDocProcessing;

public class ServerlessDocProcessingStack : Stack
{
    CustomFunctionFactory FunctionFactory { get; }

    public string EnvironmentName { get; set; }

    internal ServerlessDocProcessingStack(Construct scope, string id, ServerlessDocProcessingStackProps props = null) : base(scope, id, props)
    {
        EnvironmentName = props.EnvironmentName;

        // Function Factory
        FunctionFactory = new(this, EnvironmentName)
        {
            Timeout = FunctionParamaters.TIMEOUT,
            Memory = FunctionParamaters.MEMORY
        };
        FunctionFactory.AddEnvironmentVariable("POWERTOOLS_SERVICE_NAME", $"docprocessing-{EnvironmentName}");
        FunctionFactory.AddEnvironmentVariable("POWERTOOLS_LOG_LEVEL", $"Debug");
        FunctionFactory.AddEnvironmentVariable("POWERTOOLS_LOGGER_CASE", $"SnakeCase");
        FunctionFactory.AddEnvironmentVariable("POWERTOOLS_LOGGER_LOG_EVENT", $"true");
        FunctionFactory.AddEnvironmentVariable("POWERTOOLS_LOGGER_SAMPLE_RATE", $"0");
        FunctionFactory.AddEnvironmentVariable("POWERTOOLS_TRACE_DISABLED", $"false");
        FunctionFactory.AddEnvironmentVariable("POWERTOOLS_TRACER_CAPTURE_RESPONSE", $"true");
        FunctionFactory.AddEnvironmentVariable("POWERTOOLS_TRACER_CAPTURE_ERROR", $"true");
        FunctionFactory.AddEnvironmentVariable("POWERTOOLS_METRICS_NAMESPACE", $"SubmitToTextract-{EnvironmentName}");


        // Tables
        Table configTable = new(this, "queryData", new TableProps
        {
            TableName = GetTableName(ResourceNames.QUERY_DATA_TABLE),
            PartitionKey = new Attribute() { Name = "query", Type = AttributeType.STRING },
            BillingMode = BillingMode.PAY_PER_REQUEST,
            RemovalPolicy = RemovalPolicy.DESTROY
        });

        Table dataTable = new(this, "dataTable", new TableProps
        {
            TableName = GetTableName(ResourceNames.PROCESS_DATA_TABLE),
            PartitionKey = new Attribute() { Name = "id", Type = AttributeType.STRING },
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

        Bucket textractBucket = new(this, "textractBucket", new BucketProps
        {
            Encryption = BucketEncryption.S3_MANAGED,
            BucketName = GetBucketName("textract-data-bucket"),
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

        Topic textractTopic = new(this, "textractSuccessTopic", new TopicProps
        {
            Fifo = false,
            TopicName = GetTopicname("TextractSuccess"),
            DisplayName = "Textract Success Topic"
        });

        Queue inputDlq = new(this, "inputDlq", new QueueProps
        {
            Encryption = QueueEncryption.SQS_MANAGED,
            EnforceSSL = true,
        });


        //IAM
        Role eventRole = new(this, "inputEventRole", new RoleProps
        {
            AssumedBy = new ServicePrincipal("events.amazonaws.com")
        });

        Role textractRole = new(this, "textractRole", new RoleProps
        {
            AssumedBy = new ServicePrincipal("textract.amazonaws.com")
        });

        textractRole.AddToPolicy(new PolicyStatement(new PolicyStatementProps
        {
            Actions = new[] { "sns:Publish" },
            Resources = new[] { textractTopic.TopicArn },
            Effect = Effect.ALLOW
        }));

        textractRole.AddToPolicy(new PolicyStatement(new PolicyStatementProps
        {
            Actions = new[] { "s3:Get*", "s3:Write*" },
            Resources = new[] { textractBucket.BucketArn },
            Effect = Effect.ALLOW
        }));

        // Logging
        LogGroup stepFunctionLogGroup = new(this, "stepFunctionLogGroup", new Amazon.CDK.AWS.Logs.LogGroupProps
        {
            LogGroupName = GetLogGroupName("docProcessingWorkflow"),
            RemovalPolicy = RemovalPolicy.DESTROY
        });


        // Functions
        var initializeFunction = FunctionFactory.CreateCustomFunction("InitializeProcessing")
            .AddEnvironment(Constants.ConstantValues.QUERY_TAG_KEY, Constants.ConstantValues.QUERY_TAG);

        var submitToTextractFunction = FunctionFactory.CreateCustomFunction("SubmitToTextract")
            .AddEnvironment(ConstantValues.TEXTRACT_BUCKET_KEY, textractBucket.BucketName)
            .AddEnvironment(ConstantValues.TEXTRACT_TOPIC_KEY, textractTopic.TopicArn)
            .AddEnvironment(ConstantValues.TEXTRACT_OUTPUT_KEY_KEY, ConstantValues.TEXTRACT_OUTPUT_KEY)
            .AddEnvironment(ConstantValues.TEXTRACT_ROLE_KEY, textractRole.RoleArn);

        submitToTextractFunction.AddToRolePolicy(new PolicyStatement(new PolicyStatementProps
        {
            Actions = new[] { "s3:Get*" },
            Resources = new[]
            {
                inputBucket.BucketArn,
                inputBucket.ArnForObjects("*")
            },
            Effect = Effect.ALLOW
        }));

        submitToTextractFunction.AddToRolePolicy(new PolicyStatement(new PolicyStatementProps
        {
            Actions = new[] { "s3:Put*", "s3:Get*" },
            Resources = new[]
    {
                textractBucket.BucketArn,
                textractBucket.ArnForObjects("*")
            },
            Effect = Effect.ALLOW
        }));

        var processTextractResultFunction = FunctionFactory.CreateCustomFunction("ProcessTextractResults");

        var restartStepFunction = FunctionFactory.CreateCustomFunction("RestartStepFunction");
        restartStepFunction.AddToRolePolicy(new PolicyStatement(new PolicyStatementProps
        {
            Effect = Effect.ALLOW,
            Actions = new[] { "states:SendTaskSuccess", "states:SendTaskFailure" },
            Resources = new[] { "*" }
        }));

        textractTopic.AddSubscription(new LambdaSubscription(restartStepFunction));
        textractTopic.AddSubscription(new EmailSubscription("cbossie@gmail.com", new EmailSubscriptionProps
        {
            Json = false
        }));

        // Step Functions Tasks
        LambdaInvoke initializeState = new(this, "initializeState", new LambdaInvokeProps
        {
            LambdaFunction = initializeFunction,
            Comment = "Initializes the Document Processing Workflow",
            OutputPath = "$.Payload",
            Payload = TaskInput.FromJsonPathAt("$"),
        });

        LambdaInvoke textractState = new(this, "textractState", new LambdaInvokeProps
        {
            IntegrationPattern = IntegrationPattern.WAIT_FOR_TASK_TOKEN,
            TaskTimeout = Timeout.Duration(Duration.Seconds(StepFunctionDefaults.TEXTRACT_STEP_TIME_OUT)),
            LambdaFunction = submitToTextractFunction,
            Comment = "Function to send document to textract asynchronously",
            Payload = TaskInput.FromObject(new Dictionary<string, object> {
                { "id", JsonPath.StringAt("$.id") },
                { "taskToken", JsonPath.TaskToken}
                })
        });

        LambdaInvoke processTextractResultsState = new(this, "processTextractResults", new LambdaInvokeProps
        {
            LambdaFunction = processTextractResultFunction,
            Comment = "Function to process textract results asynchronously",
            OutputPath = "$.Payload",
        });

        SqsSendMessage sendFailureState = new(this, "sendFailureState", new SqsSendMessageProps
        {
            Queue = failureQueue,
            Comment = "Send Failure Message",
            MessageBody = TaskInput.FromJsonPathAt("$"),
        });

        SqsSendMessage sendSuccessState = new(this, "sendSuccessState", new SqsSendMessageProps
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


        StateMachine docProcessingStepFunction = new(this, "docProcessing", new StateMachineProps
        {
            TracingEnabled = true,
            Logs = new LogOptions
            {
                Destination = stepFunctionLogGroup,
                IncludeExecutionData = true,
                Level = LogLevel.ALL
            },
            DefinitionBody = DefinitionBody.FromChainable(initializeState)
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

        rule.AddTarget(new SfnStateMachine(docProcessingStepFunction, new SfnStateMachineProps
        {
            DeadLetterQueue = inputDlq,
            RetryAttempts = 3,
            Role = eventRole,
        }));


        //Assign permissions to resources
        docProcessingStepFunction.GrantStartExecution(eventRole);
        stepFunctionLogGroup.GrantWrite(docProcessingStepFunction);
        inputBucket.GrantReadWrite(initializeFunction);
        textractBucket.GrantRead(processTextractResultFunction);
        configTable.GrantDocumentObjectModelPermissions(initializeFunction);
        dataTable.GrantDocumentObjectModelPermissions(initializeFunction);
        dataTable.GrantDocumentObjectModelPermissions(submitToTextractFunction);
        dataTable.GrantDocumentObjectModelPermissions(restartStepFunction);
        dataTable.GrantDocumentObjectModelPermissions(processTextractResultFunction);
        submitToTextractFunction.Role.AddManagedPolicy(ManagedPolicy.FromAwsManagedPolicyName("AmazonTextractFullAccess"));

        // Outputs
        _ = new CfnOutput(this, "inputBucketOutput", new CfnOutputProps
        {
            Description = "Input Bucket",
            Value = inputBucket.BucketName
        });

        _ = new CfnOutput(this, "textractBucketOutput", new CfnOutputProps
        {
            Description = "Textract Output Bucket",
            Value = textractBucket.BucketName
        });

    }

    // Functions to create unique names
    private string GetBucketName(string baseName) => $"{baseName}-{EnvironmentName}-{Account}";
    private string GetQueueName(string baseName) => $"{baseName}-{EnvironmentName}-{Account}";
    private string GetTopicname(string baseName) => $"{baseName}-{EnvironmentName}-{Account}";
    private string GetTableName(string baseName) => $"{EnvironmentName}-{baseName}";
    private string GetLogGroupName(string baseName) => $"lg-{EnvironmentName}-{baseName}-{Account}";
}

