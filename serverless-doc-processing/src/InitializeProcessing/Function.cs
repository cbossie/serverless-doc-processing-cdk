using Amazon.Lambda.CloudWatchEvents.S3Events;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.S3;
using AWS.Lambda.Powertools.Logging;
using AWS.Lambda.Powertools.Metrics;
using AWS.Lambda.Powertools.Tracing;
using DocProcessing.Shared;
using DocProcessing.Shared.AwsSdkUtilities;
using DocProcessing.Shared.Model.Data;
using DocProcessing.Shared.Service;
using Microsoft.Extensions.DependencyInjection;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

public class Function
{
    private static async Task Main(string[] args)
    {
        //Initialize common functionality
        await Common.Instance.Initialize().ConfigureAwait(false);

        Func<S3ObjectCreateEvent, ILambdaContext, Task<IdMessage>> handler = FunctionHandler;
        await LambdaBootstrapBuilder.Create(handler, new DefaultLambdaJsonSerializer())
            .Build()
            .RunAsync();
    }

    [Tracing]
    [Metrics(CaptureColdStart = true)]
    [Logging(LogEvent = true, ClearState = true)]
    static async Task<IdMessage> FunctionHandler(S3ObjectCreateEvent input, ILambdaContext context)
    {
        var s3Client = Common.Instance.ServiceProvider.GetRequiredService<IAmazonS3>();
        IDataService dataSvc = Common.Instance.ServiceProvider.GetRequiredService<IDataService>();

        //Initialize the Payload
        ProcessData pl = new(dataSvc.GenerateId())
        {
            // Get the S3 item to query
            InputDocKey = input.Detail.Object.Key,
            InputDocBucket = input.Detail.Bucket.Name
        };
        pl.FileExtension = Path.GetExtension(pl.InputDocKey);

        // Retreive the Tags for the S3 object
        var data = await s3Client.GetObjectTaggingAsync(new Amazon.S3.Model.GetObjectTaggingRequest
        {
            BucketName = input.Detail.Bucket.Name,
            Key = input.Detail.Object.Key
        }).ConfigureAwait(false);

        // If there is a tag for queries get them
        var queryTagValue = data.Tagging.GetTagValueList(Constants.ConstantValues.QUERY_TAG);
        var queries = await dataSvc.GetQueries(queryTagValue).ConfigureAwait(false);

        pl.Queries.AddRange(queries.Select(q => new DocumentQuery
        {
            QueryId = q.QueryId,
            QueryText = q.QueryText,
            Processed = false,
            IsValid = true
        }));

        // If there is a tag for external id, get it. Otherwise, we won't use it
        pl.ExternalId = data.Tagging.GetTagValue(Constants.ConstantValues.ID_TAG) ?? Guid.NewGuid().ToString();


        //Save the payload
        await dataSvc.SaveData(pl).ConfigureAwait(false);

        return IdMessage.Create(pl.Id);
    }
}


