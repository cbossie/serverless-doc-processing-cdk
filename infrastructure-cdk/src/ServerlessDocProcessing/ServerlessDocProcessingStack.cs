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
using System.Collections.Generic;

namespace ServerlessDocProcessing;

public class ServerlessDocProcessingStack : Stack
{
    public string EnvironmentName { get; set; }

    internal ServerlessDocProcessingStack(Construct scope, string id, ServerlessDocProcessingStackProps props = null)
        : base(scope, id, props)
    {

        // Set up the function properties
        EnvironmentName = props.EnvironmentName;
        CustomFunctionProps.EnvironmentName = props.EnvironmentName;
        CustomFunctionProps.FunctionBaseDirectory = props.FunctionCodeBaseDirectory;
        CustomFunctionProps.AddGlobalEnvironment("POWERTOOLS_SERVICE_NAME", $"docprocessing-{EnvironmentName}");
        CustomFunctionProps.AddGlobalEnvironment("POWERTOOLS_LOG_LEVEL", $"Debug");
        CustomFunctionProps.AddGlobalEnvironment("POWERTOOLS_LOGGER_CASE", $"SnakeCase");
        CustomFunctionProps.AddGlobalEnvironment("POWERTOOLS_LOGGER_LOG_EVENT", $"true");
        CustomFunctionProps.AddGlobalEnvironment("POWERTOOLS_LOGGER_SAMPLE_RATE", $"0");
        CustomFunctionProps.AddGlobalEnvironment("POWERTOOLS_TRACE_DISABLED", $"false");
        CustomFunctionProps.AddGlobalEnvironment("POWERTOOLS_TRACER_CAPTURE_RESPONSE", $"true");
        CustomFunctionProps.AddGlobalEnvironment("POWERTOOLS_TRACER_CAPTURE_ERROR", $"true");
        CustomFunctionProps.AddGlobalEnvironment("POWERTOOLS_METRICS_NAMESPACE", $"SubmitToTextract-{EnvironmentName}");
        CustomFunctionProps.AddGlobalEnvironment("ALLOWED_FILE_EXTENSIONS", $".pdf");
        CustomFunctionProps.AddGlobalEnvironment("ENVIRONMENT_NAME", EnvironmentName);
        
        // Tables
        Table configTable = new(this, "queryData", new TableProps
        {
            TableName = GetTableName("QueryData"),
            PartitionKey = new Attribute() { Name = "query", Type = AttributeType.STRING },
            BillingMode = BillingMode.PAY_PER_REQUEST,
            RemovalPolicy = RemovalPolicy.DESTROY
        });

        Table dataTable = new(this, "dataTable", new TableProps
        {
            TableName = GetTableName("ProcessData"),
            PartitionKey = new Attribute() { Name = "id", Type = AttributeType.STRING },
            BillingMode = BillingMode.PAY_PER_REQUEST,
            RemovalPolicy = RemovalPolicy.DESTROY
        });

        // Secondary index for querying the table by executionname
        dataTable.AddGlobalSecondaryIndex(new GlobalSecondaryIndexProps
        {
            IndexName = "executionIndex",
            PartitionKey = new Attribute() { Name = "execution", Type = AttributeType.STRING },
            ProjectionType = ProjectionType.ALL            
        });


        // Buckets
        Bucket inputBucket = new(this, "inputBucket", new BucketProps
        {
            Encryption = BucketEncryption.S3_MANAGED,
            BucketName = GetBucketName("input"),
            EventBridgeEnabled = true,
            RemovalPolicy = RemovalPolicy.DESTROY,
            AutoDeleteObjects = true
        });

        Bucket textractBucket = new(this, "textractBucket", new BucketProps
        {
            Encryption = BucketEncryption.S3_MANAGED,
            BucketName = GetBucketName("textract"),
            RemovalPolicy = RemovalPolicy.DESTROY,
            AutoDeleteObjects = true
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


        //IAM Roles
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


        // Function to initialize the process. It will create the relevant data structures etc.
        var initializeFunction = new CustomFunction(this, "InitializeProcessing", new CustomFunctionProps
        {
            FunctionNameBase = "InitializeProcessing",
            Description = "Initializes the document processing",
            FunctionCodeDirectory = "ProcessingFunctions",
        }).AddAnnotationsHandler("InitializeHandler");

        // Allow the function read from the input bucket
        inputBucket.GrantReadWrite(initializeFunction);

        // Policy that allows a function full actextcess to the textract bucket ARN (Needed for enabling textract to write out results to S3
        PolicyStatement allowTextractBucketStatement = new(new PolicyStatementProps
        {
            Actions = new[] { "s3:Put*", "s3:Get*" },
            Resources = new[]
            {
                textractBucket.BucketArn,
                textractBucket.ArnForObjects("*")
            },
            Effect = Effect.ALLOW
        });

        PolicyStatement allowInputBucketStatement = new(new PolicyStatementProps
        {
            Actions = new[] { "s3:Get*" },
            Resources = new[]
            {
                inputBucket.BucketArn,
                inputBucket.ArnForObjects("*")
            },
            Effect = Effect.ALLOW
        });

        // Functions that send a specially formatted message to the SQS queue for both success and failure
        var successFunction = new CustomFunction(this, "SuccessFunction", new CustomFunctionProps
        {

            FunctionNameBase = "SuccessFunction",
            FunctionCodeDirectory = "ProcessingFunctions",
            Description = "Sends a success message to the SQS queue"
        }).AddAnnotationsHandler("SuccessOutputHandler");

        // Functions that send a specially formatted message to the SQS queue for both success and failure
        var failFunction = new CustomFunction(this, "FailFunction", new CustomFunctionProps
        {

            FunctionNameBase = "FailFunction",
            FunctionCodeDirectory = "ProcessingFunctions",
            Description = "Sends a faliure message to the SQS queue"
        }).AddAnnotationsHandler("FailOutputHandler");


        // Function that submits the document to the textract service
        var submitToTextractFunction = new CustomFunction(this, "SubmitToTextract", new CustomFunctionProps
        {
            FunctionNameBase = "SubmitToTextract",
            FunctionCodeDirectory = "SubmitToTextract",
            Description = "Submits the document to Textract for analysis"
        })
            .AddAnnotationsHandler("SubmitToTextractForStandardAnalysis")
            .AddEnvironment("TEXTRACT_BUCKET", textractBucket.BucketName)
            .AddEnvironment("TEXTRACT_TOPIC", textractTopic.TopicArn)
            .AddEnvironment("TEXTRACT_OUTPUT_KEY", "results")
            .AddEnvironment("TEXTRACT_ROLE", textractRole.RoleArn);

        // Allows the function to retrieve the document from S3
        submitToTextractFunction.AddToRolePolicy(allowInputBucketStatement);

        // Allows textract to write out the data to S3
        submitToTextractFunction.AddToRolePolicy(allowTextractBucketStatement);

        // Allow function to call Textract
        submitToTextractFunction.Role.AddManagedPolicy(ManagedPolicy.FromAwsManagedPolicyName("AmazonTextractFullAccess"));

        // Function that submits the document to the textract service for Expense Analysis
        var submitToTextractExpenseFunction = new CustomFunction(this, "SubmitToTextractExpense", new CustomFunctionProps
        {
            FunctionNameBase = "SubmitToTextractExpense",
            FunctionCodeDirectory = "SubmitToTextract",
            Description = "Submits the document to Textract for expense analysis"
        })
            .AddAnnotationsHandler("SubmitToTextractForExpenseAnalysis")
            .AddEnvironment("TEXTRACT_BUCKET", textractBucket.BucketName)
            .AddEnvironment("TEXTRACT_TOPIC", textractTopic.TopicArn)
            .AddEnvironment("TEXTRACT_EXPENSE_OUTPUT_KEY", "expensresults")
            .AddEnvironment("TEXTRACT_ROLE", textractRole.RoleArn);

        // Allows the function to retrieve the document from S3
        submitToTextractExpenseFunction.AddToRolePolicy(allowInputBucketStatement);

        // Allows textract to write out the data to S3
        submitToTextractExpenseFunction.AddToRolePolicy(allowTextractBucketStatement);

        // Allow functions to call Textract
        submitToTextractFunction.Role.AddManagedPolicy(ManagedPolicy.FromAwsManagedPolicyName("AmazonTextractFullAccess"));
        submitToTextractExpenseFunction.Role.AddManagedPolicy(ManagedPolicy.FromAwsManagedPolicyName("AmazonTextractFullAccess"));

        // Function that process the textract data and outputs results to DynamoDB
        var processTextractQueryResultFunction = new CustomFunction(this, "ProcessTextractQueryResults", new CustomFunctionProps
        {
            FunctionNameBase = "ProcessTextractQueryResults",
            Description = "Processes the general Textract results and extracts query information"
        });

        // The query processing function needs to be able to read from the S3 bucket.
        textractBucket.GrantRead(processTextractQueryResultFunction);

        // Function that process the textract Expense data and outputs results to DynamoDB
        var processTextractExpenseResultFunction = new CustomFunction(this, "ProcessTextractExpenseResults", new CustomFunctionProps
        {
            FunctionNameBase = "ProcessTextractExpenseResults",
            Description = "Processes textract expense results"
        });

        // The query processing function needs to be able to read from the S3 bucket.
        textractBucket.GrantRead(processTextractExpenseResultFunction);

        // Function that restarts the step function following the aynchronous completion of Textract
        var restartStepFunction = new CustomFunction(this, "RestartStepFunction", new CustomFunctionProps
        {
            FunctionNameBase = "RestartStepFunction"
        });

        // Allows the function to restart the step function
        restartStepFunction.AddToRolePolicy(new PolicyStatement(new PolicyStatementProps
        {
            Effect = Effect.ALLOW,
            Actions = new[] { "states:SendTaskSuccess", "states:SendTaskFailure" },
            Resources = new[] { "*" }
        }));


        // Add a subscription to the lambda function that will be restarting the step function, to the topic that
        // Textract published to
        textractTopic.AddSubscription(new LambdaSubscription(restartStepFunction));        


        // Step function that coordinates processing
        DocProcessingStepFunction docProcessingStepFunction = new(this, "docProcessing", new DocProcessingStepFunctionProps
        {
            TracingEnabled = true,
            StateMachineType = StateMachineType.STANDARD,
            Logs = new LogOptions
            {
                Destination = stepFunctionLogGroup,
                IncludeExecutionData = true,
                Level = LogLevel.ALL
            },
            InitializeFunction = initializeFunction,
            SubmitToTextractFunction = submitToTextractFunction,
            SubmitToTextractExpenseFunction = submitToTextractExpenseFunction,
            ProcessTextractQueryFunction = processTextractQueryResultFunction,
            ProcessTextractExpenseFunction = processTextractExpenseResultFunction,
            SuccessFunction = successFunction,
            FailureFunction = failFunction,
            SendFailureQueue = failureQueue,
            SendSuccessQueue = successQueue
        });

        // EventBridge Rule that reacts to S3
        Rule rule = new(this, "inputBucketRule", new RuleProps
        {
            Enabled = true,
            RuleName = GetEventbridgeRuleName("input"),
            EventPattern = new EventPattern
            {

                Source = new[] { "aws.s3" },
                DetailType = new[] { "Object Created" },
                Detail = new Dictionary<string, object> {
                         {
                             "bucket", new Dictionary<string, object> { {"name", new [] {inputBucket.BucketName } }
                         }
                    }
                }
            }
        });

        // Start the above Step Function state machine from EventBridge
        rule.AddTarget(new SfnStateMachine(docProcessingStepFunction.StateMachine, new SfnStateMachineProps
        {
            DeadLetterQueue = inputDlq,
            RetryAttempts = 3,
            Role = eventRole,
        }));

        //Permssions required
        docProcessingStepFunction.StateMachine.GrantStartExecution(eventRole);

        // Allow the state machine to write to CloudWatch Logs
        stepFunctionLogGroup.GrantWrite(docProcessingStepFunction.StateMachine);

        // Grant permissions to functions to use Object Persistence Model in DynamoDB
        configTable.GrantDocumentObjectModelPermissions(initializeFunction);
        dataTable.GrantDocumentObjectModelPermissions(initializeFunction);
        dataTable.GrantDocumentObjectModelPermissions(submitToTextractFunction);
        dataTable.GrantDocumentObjectModelPermissions(submitToTextractExpenseFunction);
        dataTable.GrantDocumentObjectModelPermissions(restartStepFunction);
        dataTable.GrantDocumentObjectModelPermissions(processTextractQueryResultFunction);
        dataTable.GrantDocumentObjectModelPermissions(processTextractExpenseResultFunction);
        dataTable.GrantDocumentObjectModelPermissions(successFunction);
        dataTable.GrantDocumentObjectModelPermissions(failFunction);

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
    private string GetEventbridgeRuleName(string basename) => $"docprocessing-{EnvironmentName}";
    private string GetBucketName(string baseName) => $"docprocessing-{baseName}-{EnvironmentName}-{Aws.ACCOUNT_ID}-{Aws.REGION}";
    private string GetQueueName(string baseName) => $"docProceesing-{baseName}-{EnvironmentName}-{Aws.ACCOUNT_ID}";
    private string GetTopicname(string baseName) => $"docProcesing{baseName}-{EnvironmentName}-{Aws.ACCOUNT_ID}";
    private string GetTableName(string baseName) => $"{EnvironmentName}-{baseName}";
    private string GetLogGroupName(string baseName) => $"docprocessing-{EnvironmentName}-{baseName}-{Aws.ACCOUNT_ID}";
}

