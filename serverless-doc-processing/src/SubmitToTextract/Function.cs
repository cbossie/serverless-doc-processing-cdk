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
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;

//Configure the Serializer
[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

await Common.Instance.Initialize();

[Tracing]
[Metrics(CaptureColdStart = true)]
[Logging]
async Task<ProcessData> FunctionHandler(ProcessData input, ILambdaContext context)
{
    IAmazonTextract textract = Common.Instance.ServiceProvider.GetRequiredService<IAmazonTextract>();

    await textract.StartDocumentAnalysisAsync(new Amazon.Textract.Model.StartDocumentAnalysisRequest
    {
        ClientRequestToken = input.Id,
        JobTag = input.Id,
        NotificationChannel = new Amazon.Textract.Model.NotificationChannel
        {
            SNSTopicArn = Environment.GetEnvironmentVariable(Constants.ConstantValues.TEXTRACT_TOPIC_KEY),
            RoleArn = Environment.GetEnvironmentVariable(Constants.ConstantValues.TEXTRACT_TOPIC_KEY)
        },
        DocumentLocation = new Amazon.Textract.Model.DocumentLocation
        {
            S3Object = new Amazon.Textract.Model.S3Object
            {
                Bucket = input.InputDocBucket,
                Name = input.InputDocKey
            }
        },
        QueriesConfig = new Amazon.Textract.Model.QueriesConfig
        {
            Queries = input.Queries.Select(q => new Query
            {
                Alias = q.QueryId,
                Pages = new List<string> { "*" },
                Text = q.QueryText
            }).ToList(),
        },
        OutputConfig = new OutputConfig
        {
            S3Bucket = Environment.GetEnvironmentVariable(Constants.ConstantValues.TEXTRACT_BUCKET_KEY)
        }
    }) ;



    return input;
};


var functionHandlerDelegate = FunctionHandler;


// to .NET types.
await LambdaBootstrapBuilder.Create(functionHandlerDelegate, new DefaultLambdaJsonSerializer())
        .Build()
        .RunAsync();




