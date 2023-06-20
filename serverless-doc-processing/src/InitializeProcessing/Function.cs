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

//Configure the Serializer
[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]


await Common.Instance.Initialize();

[Tracing]
[Metrics(CaptureColdStart = true)]
[Logging(LogEvent = true)]
async Task<Payload> FunctionHandler (S3ObjectCreateEvent input, ILambdaContext context)
{
    var s3Client = Common.Instance.ServiceProvider.GetRequiredService<IAmazonS3>();

    var id = Guid.NewGuid().ToString();
    Metrics.AddMetric("HandlerStartTime", context.RemainingTime.Milliseconds, MetricUnit.Milliseconds, MetricResolution.High);




    Payload pl = new();

    pl.InputDocKey = input.Detail.Object.Key;

    var data = await s3Client.GetObjectMetadataAsync(new Amazon.S3.Model.GetObjectMetadataRequest
    { 
        BucketName= input.Detail.Bucket.Name,
        Key = input.Detail.Object.Key
    });

    var md = data.Metadata;
    foreach(var l in md.Keys)
    {
        Logger.LogInformation($"Key {l} : {md[l]}");
    }

    
    pl.Id = id;

    

    return pl;
};

var functionHandlerDelegate = FunctionHandler;



// to .NET types.
await LambdaBootstrapBuilder.Create(functionHandlerDelegate, new DefaultLambdaJsonSerializer())
        .Build()
        .RunAsync();






