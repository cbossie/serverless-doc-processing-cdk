using Amazon.Lambda.CloudWatchEvents.S3Events;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.S3;

using AWS.Lambda.Powertools.Logging;
using AWS.Lambda.Powertools.Metrics;
using AWS.Lambda.Powertools.Tracing;
using DocProcessing.Shared;
using Microsoft.Extensions.DependencyInjection;
using DocProcessing.Shared.AwsSdkUtilities;
using DocProcessing.Shared.Service;
using DocProcessing.Shared.Query;


//Configure the Serializer
[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]


await Common.Instance.Initialize();

[Tracing]
[Metrics(CaptureColdStart = true)]
[Logging(LogEvent = true)]
async Task<ProcessData> FunctionHandler (S3ObjectCreateEvent input, ILambdaContext context)
{
    var s3Client = Common.Instance.ServiceProvider.GetRequiredService<IAmazonS3>();
    IDataService dataSvc = Common.Instance.ServiceProvider.GetRequiredService<IDataService>();

    //Initialize the Payload
    ProcessData pl = new(dataSvc.GenerateId());

    // Get the S3 item to query
    pl.InputDocKey = input.Detail.Object.Key;
    pl.InputDocBucket = input.Detail.Bucket.Name;
    pl.FileExtension = Path.GetExtension(pl.InputDocKey);


    // Retreive the Tags for the S3 object
    var data = await s3Client.GetObjectTaggingAsync(new Amazon.S3.Model.GetObjectTaggingRequest
    { 
        BucketName= input.Detail.Bucket.Name,
        Key = input.Detail.Object.Key
    });

    // If there is a tag for queries get them
    var queryTagValue = data.Tagging.GetTagValueList(Constants.ConstantValues.QUERY_TAG);
    var queries = await dataSvc.GetQueries(queryTagValue);

    pl.Queries.AddRange(queries.Select(q => new QueryConfig
    {
        QueryId = q.QueryId,
        QueryText = q.QueryText,
        Processed = false,
        IsValidQuery= true
    }));

    // If there is a tag for external id, get it. Otherwise, we won't use it
    pl.ExternalId = data.Tagging.GetTagValue(Constants.ConstantValues.ID_TAG) ?? Guid.NewGuid().ToString();


    //Save the payload
    return await dataSvc.SaveData(pl);
};

var functionHandlerDelegate = FunctionHandler;



// to .NET types.
await LambdaBootstrapBuilder.Create(functionHandlerDelegate, new DefaultLambdaJsonSerializer())
        .Build()
        .RunAsync();






