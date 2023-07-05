using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using DocProcessing.Shared;
using AWS.Lambda.Powertools.Logging;
using AWS.Lambda.Powertools.Metrics;
using AWS.Lambda.Powertools.Tracing;
using Microsoft.Extensions.DependencyInjection;
using DocProcessing.Shared.Service;
using Amazon.StepFunctions;
using Amazon.Textract;
using DocProcessing.Shared.Model.Data;
using DocProcessing.Shared.AwsSdkUtilities;

//Configure the Serializer
[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

await Common.Instance.Initialize();

[Tracing]
[Metrics(CaptureColdStart = true)]
[Logging(LogEvent = true)]
async Task<IdMessage> FunctionHandler(IdMessage input, ILambdaContext context) 
{

    var dataSvc = Common.Instance.ServiceProvider.GetRequiredService<IDataService>();
    var textractSvc = Common.Instance.ServiceProvider.GetRequiredService<ITextractService>();

    var processData = await dataSvc.GetData<ProcessData>(input.Id);

    // Get the step functions Result






    return input;
}

var functionHandlerDelegate = FunctionHandler;


// to .NET types.
await LambdaBootstrapBuilder.Create(functionHandlerDelegate, new DefaultLambdaJsonSerializer())
        .Build()
        .RunAsync();
