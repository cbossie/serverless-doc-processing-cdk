using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.S3;
using Amazon.CDK.AWS.SNS;
using Amazon.CDK.AWS.SQS;

namespace ServerlessDocProcessing;

public class ServerlessDocProcessingStack : Stack
{
    CustomFunctionFactory FunctionFactory { get; }

    public string EnvironmentName { get; set; } = "dev";

    internal ServerlessDocProcessingStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
    {
        

        FunctionFactory = new(this);

        // Functions

        var fcn1 = FunctionFactory.CreateCustomFUnction("DotNet7Lambda");


        // Tables
        Table configTable = new(this, "configTable", new TableProps
        {
            TableName = GetTableName("config"),
            PartitionKey = new Amazon.CDK.AWS.DynamoDB.Attribute() { Name = "query", Type = AttributeType.STRING },
            BillingMode = BillingMode.PAY_PER_REQUEST
        });

        Table dataTable = new(this, "dataTable", new TableProps
        {
            TableName = GetTableName("data"),
            PartitionKey = new Amazon.CDK.AWS.DynamoDB.Attribute() { Name = "id", Type = AttributeType.STRING },
            BillingMode = BillingMode.PAY_PER_REQUEST
        });

        // Buckets
        Bucket inputBucket = new (this, "inputBucket", new BucketProps
        {
            Encryption = BucketEncryption.S3_MANAGED,
            BucketName = GetBucketName("input-bucket")
        });

        Bucket extractedDataBucket = new (this, "extractedData", new BucketProps
        {
            Encryption = BucketEncryption.S3_MANAGED,
            BucketName = GetBucketName("extracted-data-bucket")
        });

        // Messaging (SQS / SNS / Event Bridge)
        Queue successQueue = new (this, "successQueue", new QueueProps
        {
            QueueName = GetQueueName("successQueue")
        }) ;

        Queue failureQueue = new (this, "failureQueue", new QueueProps
        {
            QueueName = GetQueueName("failureQueue")
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

        

        // Logging1



    }

    private string GetBucketName(string baseName) => $"{baseName}-{Account}-{EnvironmentName}";


    private string GetQueueName(string baseName) => $"{baseName}-q{Account}-{EnvironmentName}";

    private string GetTopicname(string baseName) => $"{baseName}-t{Account}-{EnvironmentName}";

    private string GetTableName(string baseName) => $"{EnvironmentName}-{baseName}-{Account}";


}
