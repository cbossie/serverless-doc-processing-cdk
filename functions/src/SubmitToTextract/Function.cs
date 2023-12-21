using Amazon.Lambda.Annotations;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.Textract;
using Amazon.Textract.Model;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using AWS.Lambda.Powertools.Logging;
using AWS.Lambda.Powertools.Metrics;
using AWS.Lambda.Powertools.Tracing;
using DocProcessing.Shared;
using DocProcessing.Shared.Service;
using Microsoft.Extensions.DependencyInjection;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]
[assembly: LambdaGlobalProperties(GenerateMain = true)]

namespace SubmitToTextract
{
    public class Function
    {        
        public Function()
        {
            AWSSDKHandler.RegisterXRayForAllServices();
        }

        [Tracing]
        [Metrics(CaptureColdStart = true)]
        [Logging(ClearState = true, LogEvent = true)]
        [LambdaFunction]
        public async Task<IdMessage> FunctionHandler(IdMessage input, ILambdaContext context, [FromServices]IAmazonTextract textractCli, IDataService dataSvc)
        {
            var data = await dataSvc.GetData<ProcessData>(input.Id).ConfigureAwait(false);

            var textractRequest = new Amazon.Textract.Model.StartDocumentAnalysisRequest
            {
                ClientRequestToken = input.Id,
                JobTag = input.Id,
                FeatureTypes = new List<string> { FeatureType.TABLES, FeatureType.QUERIES, FeatureType.FORMS },
                NotificationChannel = new Amazon.Textract.Model.NotificationChannel
                {
                    SNSTopicArn = Environment.GetEnvironmentVariable(ConstantValues.TEXTRACT_TOPIC_KEY),
                    RoleArn = Environment.GetEnvironmentVariable(ConstantValues.TEXTRACT_ROLE_KEY),
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
                    S3Bucket = Environment.GetEnvironmentVariable(ConstantValues.TEXTRACT_BUCKET_KEY),
                    S3Prefix = Environment.GetEnvironmentVariable(ConstantValues.TEXTRACT_OUTPUT_KEY_KEY)
                }
            };

            var textractResult = await textractCli.StartDocumentAnalysisAsync(textractRequest).ConfigureAwait(false);

            data.TextractJobId = textractResult.JobId;
            data.TextractTaskToken = input.TaskToken;
            data.OutputKey = $"{Environment.GetEnvironmentVariable(ConstantValues.TEXTRACT_OUTPUT_KEY_KEY)}/{textractResult.JobId}";
            data.OutputBucket = Environment.GetEnvironmentVariable(ConstantValues.TEXTRACT_BUCKET_KEY);

            await dataSvc.SaveData(data).ConfigureAwait(false);

            return IdMessage.Create(data.Id);
        }
    }
}