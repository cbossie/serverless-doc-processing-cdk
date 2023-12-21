using Amazon.Lambda.Annotations;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.S3;
using Amazon.Textract;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using AWS.Lambda.Powertools.Logging;
using AWS.Lambda.Powertools.Metrics;
using AWS.Lambda.Powertools.Tracing;
using DocProcessing.Shared;
using DocProcessing.Shared.AwsSdkUtilities;
using DocProcessing.Shared.Model.Data.Query;
using DocProcessing.Shared.Service;
using Microsoft.Extensions.DependencyInjection;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]
[assembly: LambdaGlobalProperties(GenerateMain = true)]
namespace ProcessTextractQueryResults
{
    public class Function
    {

        public Function()
        {
            AWSSDKHandler.RegisterXRayForAllServices();
        }

        [Tracing]
        [Metrics(CaptureColdStart = true)]
        [Logging(LogEvent = true)]
        [LambdaFunction]
        public async Task<IdMessage> FunctionHandler(IdMessage input, ILambdaContext context, [FromServices] ITextractService textractSvc, [FromServices] IDataService dataSvc)
        {
            var processData = await dataSvc.GetData<ProcessData>(input.Id).ConfigureAwait(false);

            // Get the step functions Result
            var textractModel = await textractSvc.GetBlocksForAnalysis(processData.OutputBucket, processData.OutputKey).ConfigureAwait(false);

            // Get the query Results
            foreach (var query in processData.Queries)
            {
                var queryResult = textractModel.GetQueryResults(query.QueryId);

                if (queryResult.Any())
                {
                    query.IsValid = false;
                }
                else
                {
                    query.IsValid = true;
                    query.Result.AddRange(queryResult.Select(r => new DocumentQueryResult() { Confidence = r.Confidence, ResultText = r.Text }));
                }
            }

            // Save the query results back to the databast
            await dataSvc.SaveData(processData).ConfigureAwait(false);

            Logger.LogInformation($"Blocks Found = {textractModel.BlockCount}");

            return input;
        }

    }
}