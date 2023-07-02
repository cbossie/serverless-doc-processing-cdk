using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.Lambda.SNSEvents;
using Amazon.Runtime;
using Amazon.StepFunctions;
using Amazon.StepFunctions.Model;
using AWS.Lambda.Powertools.Logging;
using AWS.Lambda.Powertools.Metrics;
using AWS.Lambda.Powertools.Tracing;
using DocProcessing.Shared;
using DocProcessing.Shared.Service;
using Microsoft.Extensions.DependencyInjection;
using RestartStepFunction.Exceptions;
using RestartStepFunction.Model;
using System.Text.Json;

//Configure the Serializer
[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

await Common.Instance.Initialize();

[Tracing]
[Metrics(CaptureColdStart = true)]
[Logging(ClearState = true, LogEvent = true)]
async Task FunctionHandler(SNSEvent input, ILambdaContext context)
{
    var record = input.Records.FirstOrDefault();
    var dataSvc = Common.Instance.ServiceProvider.GetRequiredService<IDataService>();
    var stepFunctionCli = Common.Instance.ServiceProvider.GetRequiredService<IAmazonStepFunctions>();

    //If there is no message, throw an error
    if (record is null)
    {
        Logger.LogError(input);
        throw new RestartStepFunctionException("No message received");        
    }

    // Deserialize the Message
    var message = JsonSerializer.Deserialize<TextractCompletionModel>(record.Sns.Message);

    // Get the Task Token
    var processData = await dataSvc.GetData<ProcessData>(message.JobTag);

    if(processData.TextractTaskToken is null)
    {
        throw new RestartStepFunctionException("Missing Task Token");
    }

    var responseMessage = JsonSerializer.Serialize(new IdMessage 
    {
        Id = message.JobTag 
    });
    processData.OutputBucket = message.DocumentLocation.S3Object.Bucket;
    processData.OutputKey = message.DocumentLocation.S3Object.Name;

    AmazonWebServiceResponse response;

    if (message.IsSuccess)
    {
        Logger.LogInformation("Success!");
        response = await stepFunctionCli.SendTaskSuccessAsync(new() 
        {
            TaskToken = processData.TextractTaskToken,
            Output = JsonSerializer.Serialize(processData)
        });
    }
    else
    {
        Logger.LogInformation("Failure!");
        response = await stepFunctionCli.SendTaskFailureAsync(new()
        {
            TaskToken = processData.TextractTaskToken,
            Error = $"{message.API} {message.Status}",
            Cause = record.Sns.Message
        });
    }

    // Log the output
    if(response.HttpStatusCode == System.Net.HttpStatusCode.OK)
    {
        Logger.LogInformation($"Successfully sent Step Function Completion");
        Logger.LogInformation(response);
    }
    else
    {
        Logger.LogError($"Error sending Step Function Completion");
        Logger.LogError(response);
    }

    await dataSvc.SaveData(processData);


}

var functionHandlerDelegate = FunctionHandler;


// to .NET types.
await LambdaBootstrapBuilder.Create(functionHandlerDelegate, new DefaultLambdaJsonSerializer())
        .Build()
        .RunAsync();