using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using DocProcessing.Shared;
using AWS.Lambda.Powertools.Logging;
using AWS.Lambda.Powertools.Metrics;
using AWS.Lambda.Powertools.Tracing;
using Microsoft.Extensions.DependencyInjection;

//Configure the Serializer
[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

//await Common.Instance.Initialize();

[Tracing]
[Metrics(CaptureColdStart = true)]
[Logging(LogEvent = true)]
async Task<string> FunctionHandler(string input, ILambdaContext context) 
{
    return input;
}

var functionHandlerDelegate = FunctionHandler;


// to .NET types.
await LambdaBootstrapBuilder.Create(functionHandlerDelegate, new DefaultLambdaJsonSerializer())
        .Build()
        .RunAsync();
