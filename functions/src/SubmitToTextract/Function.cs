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
        private IAmazonTextract _textractClient;
        private IDataService _dataService;

        public Function(IAmazonTextract textractClient, IDataService dataService)
        {
            _textractClient = textractClient;
            _dataService = dataService;
            AWSSDKHandler.RegisterXRayForAllServices();
        }


        [Tracing]
        [Metrics(CaptureColdStart = true)]
        [Logging(ClearState = true, LogEvent = true)]
        [LambdaFunction]
        public async Task<IdMessage> SubmitToTextractForStandardAnalysis(IdMessage input, ILambdaContext context)
        {
            var data = await _dataService.GetData<ProcessData>(input.Id).ConfigureAwait(false);
            var textractRequest = new StartDocumentAnalysisRequest
            {
                ClientRequestToken = input.Id,
                JobTag = input.Id,
                FeatureTypes = new List<string> { FeatureType.TABLES, FeatureType.QUERIES, FeatureType.FORMS },
                NotificationChannel = new Amazon.Textract.Model.NotificationChannel
                {
                    SNSTopicArn = Environment.GetEnvironmentVariable(ConstantValues.TEXTRACT_TOPIC_KEY),
                    RoleArn = Environment.GetEnvironmentVariable(ConstantValues.TEXTRACT_ROLE_KEY),
                },
                DocumentLocation = new DocumentLocation
                {
                    S3Object = new Amazon.Textract.Model.S3Object
                    {
                        Bucket = data.InputDocBucket,
                        Name = data.InputDocKey
                    }
                },
                OutputConfig = new OutputConfig
                {
                    S3Bucket = Environment.GetEnvironmentVariable(ConstantValues.TEXTRACT_BUCKET_KEY),
                    S3Prefix = Environment.GetEnvironmentVariable(ConstantValues.TEXTRACT_OUTPUT_KEY_KEY)
                }
            };

            // Add query config if there are some in the DB
            List<Query> queries = (data.Queries != null 
                ? data.Queries.Select(q => new Query
                {
                    Alias = q.QueryId,
                    Pages = new List<string> { "*" },
                    Text = q.QueryText
                })
                : Enumerable.Empty<Query>()).ToList();
            if(queries.Any())
            {
                textractRequest.QueriesConfig = new QueriesConfig
                {
                    Queries = queries
                };            
            }
            else
            {
                Logger.LogInformation("No queries found in the database");
            }
                     
            // Submit to textract for analysis
            var textractResult = await _textractClient.StartDocumentAnalysisAsync(textractRequest).ConfigureAwait(false);
            data.TextractJobId = textractResult.JobId;
            data.TextractTaskToken = input.TaskToken;
            data.OutputKey = $"{Environment.GetEnvironmentVariable(ConstantValues.TEXTRACT_OUTPUT_KEY_KEY)}/{textractResult.JobId}";
            data.OutputBucket = Environment.GetEnvironmentVariable(ConstantValues.TEXTRACT_BUCKET_KEY);
            await _dataService.SaveData(data).ConfigureAwait(false);
            return IdMessage.Create(data.Id);
        }

        [Tracing]
        [Metrics(CaptureColdStart = true)]
        [Logging(ClearState = true, LogEvent = true)]
        [LambdaFunction]
        public async Task<IdMessage> SubmitToTextractForExpenseAnalysis(IdMessage input, ILambdaContext context)
        {
            var data = await _dataService.GetData<ProcessData>(input.Id).ConfigureAwait(false);

            var textractRequest = new StartExpenseAnalysisRequest
            {
                ClientRequestToken = input.Id,
                JobTag = input.Id,
                NotificationChannel = new Amazon.Textract.Model.NotificationChannel
                {
                    SNSTopicArn = Environment.GetEnvironmentVariable(ConstantValues.TEXTRACT_TOPIC_KEY),
                    RoleArn = Environment.GetEnvironmentVariable(ConstantValues.TEXTRACT_ROLE_KEY),
                },
                DocumentLocation = new DocumentLocation
                {
                    S3Object = new Amazon.Textract.Model.S3Object
                    {
                        Bucket = data.InputDocBucket,
                        Name = data.InputDocKey
                    }
                },
                OutputConfig = new OutputConfig
                {
                    S3Bucket = Environment.GetEnvironmentVariable(ConstantValues.TEXTRACT_BUCKET_KEY),
                    S3Prefix = Environment.GetEnvironmentVariable(ConstantValues.TEXTRACT_EXPENSE_OUTPUT_KEY_KEY)
                }
            };
            var textractResult = await _textractClient.StartExpenseAnalysisAsync(textractRequest).ConfigureAwait(false);
            data.TextractJobId = textractResult.JobId;
            data.TextractTaskToken = input.TaskToken;
            data.OutputKey = $"{Environment.GetEnvironmentVariable(ConstantValues.TEXTRACT_EXPENSE_OUTPUT_KEY_KEY)}/{textractResult.JobId}";
            data.OutputBucket = Environment.GetEnvironmentVariable(ConstantValues.TEXTRACT_BUCKET_KEY);

            await _dataService.SaveData(data).ConfigureAwait(false);

            return IdMessage.Create(data.Id);
        }

    }
}