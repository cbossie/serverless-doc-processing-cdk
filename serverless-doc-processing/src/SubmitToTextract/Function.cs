using Amazon.Lambda.CloudWatchEvents.S3Events;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using AWS.Lambda.Powertools.Logging;
using AWS.Lambda.Powertools.Metrics;
using AWS.Lambda.Powertools.Tracing;
using DocProcessing.Shared;
using System.Net.Http.Headers;

//Configure the Serializer
[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

await Common.Instance.Initialize();

[Tracing]
[Metrics(CaptureColdStart = true)]
[Logging]
async Task<Payload> FunctionHandler(Payload input, ILambdaContext context)
{  
    return input;
};


var functionHandlerDelegate = FunctionHandler;


// to .NET types.
await LambdaBootstrapBuilder.Create(functionHandlerDelegate, new DefaultLambdaJsonSerializer())
        .Build()
        .RunAsync();




