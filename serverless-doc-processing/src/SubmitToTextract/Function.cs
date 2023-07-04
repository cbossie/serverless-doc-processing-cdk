using Amazon.DynamoDBv2.Model.Internal.MarshallTransformations;
using Amazon.Lambda.CloudWatchEvents.S3Events;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.Textract;
using Amazon.Textract.Model;
using AWS.Lambda.Powertools.Logging;
using AWS.Lambda.Powertools.Metrics;
using AWS.Lambda.Powertools.Tracing;
using DocProcessing.Shared;
using DocProcessing.Shared.Model.Data;
using DocProcessing.Shared.Service;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;

//Configure the Serializer
[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

await Common.Instance.Initialize();

[Tracing]
[Metrics(CaptureColdStart = true)]
[Logging(ClearState = true, LogEvent = true)]
async Task<IdMessage> FunctionHandler(IdMessage input, ILambdaContext context)
{
    IAmazonTextract textractCli = Common.Instance.ServiceProvider.GetRequiredService<IAmazonTextract>();
    IDataService dataSvc = Common.Instance.ServiceProvider.GetService<IDataService>();

    var data = await dataSvc.GetData<ProcessData>(input.Id);

    var textractRequest = new Amazon.Textract.Model.StartDocumentAnalysisRequest
    {
        ClientRequestToken = input.Id,
        JobTag = input.Id,
        FeatureTypes = new List<string> { FeatureType.TABLES, FeatureType.QUERIES, FeatureType.FORMS },
        NotificationChannel = new Amazon.Textract.Model.NotificationChannel
        {
            SNSTopicArn = Environment.GetEnvironmentVariable(Constants.ConstantValues.TEXTRACT_TOPIC_KEY),
            RoleArn = Environment.GetEnvironmentVariable(Constants.ConstantValues.TEXTRACT_ROLE_KEY),
        },
        DocumentLocation = new Amazon.Textract.Model.DocumentLocation
        {
            S3Object = new Amazon.Textract.Model.S3Object
            {
                Bucket = data.InputDocBucket,
                Name = data.InputDocKey
            }
        },
        QueriesConfig = new Amazon.Textract.Model.QueriesConfig
        {
            Queries = data.Queries.Select(q => new Query
            {
                Alias = q.QueryId,
                Pages = new List<string> { "*" },
                Text = q.QueryText
            }).ToList(),
        },
        OutputConfig = new OutputConfig
        {
            S3Bucket = Environment.GetEnvironmentVariable(Constants.ConstantValues.TEXTRACT_BUCKET_KEY),
            S3Prefix = "output"
        }
    };

    var textractResult = await textractCli.StartDocumentAnalysisAsync(textractRequest);

    data.TextractJobId = textractResult.JobId;
    data.TextractTaskToken = input.TaskToken;
    data.OutputKey = textractResult.JobId;
    data.OutputBucket = Environment.GetEnvironmentVariable(Constants.ConstantValues.TEXTRACT_BUCKET_KEY);

    await dataSvc.SaveData(data);

    return IdMessage.Create(data.Id);
};


var functionHandlerDelegate = FunctionHandler;


// to .NET types.
await LambdaBootstrapBuilder.Create(functionHandlerDelegate, new DefaultLambdaJsonSerializer())
        .Build()
        .RunAsync();




