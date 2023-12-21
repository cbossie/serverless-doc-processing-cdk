using Amazon.Lambda.Annotations;
using Amazon.Lambda.CloudWatchEvents.S3Events;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.S3;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using AWS.Lambda.Powertools.Logging;
using AWS.Lambda.Powertools.Metrics;
using AWS.Lambda.Powertools.Tracing;
using DocProcessing.Shared.Model.Data.Query;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]
[assembly: LambdaGlobalProperties(GenerateMain = true)]
namespace InitializeProcessing
{
    public class Function
    {
        public Function()
        {
            AWSSDKHandler.RegisterXRayForAllServices();
        }

        [LambdaFunction()]
        [Tracing]
        [Metrics(CaptureColdStart = true)]
        [Logging(LogEvent = true, ClearState = true)]
        public async Task<IdMessage> FunctionHandler([FromServices] IAmazonS3 s3Client, [FromServices] IDataService dataSvc, S3ObjectCreateEvent input, ILambdaContext context)
        {

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
            var queryTagValue = data.Tagging.GetTagValueList(ConstantValues.QUERY_TAG);
            var queries = await dataSvc.GetQueries(queryTagValue).ConfigureAwait(false);

            pl.Queries.AddRange(queries.Select(q => new DocumentQuery
            {
                QueryId = q.QueryId,
                QueryText = q.QueryText,
                Processed = false,
                IsValid = true
            }));

            // If there is a tag for external id, get it. Otherwise, we won't use it
            pl.ExternalId = data.Tagging.GetTagValue(ConstantValues.ID_TAG) ?? Guid.NewGuid().ToString();


            //Save the payload
            await dataSvc.SaveData(pl).ConfigureAwait(false);

            return IdMessage.Create(pl.Id);
        }
    }


}