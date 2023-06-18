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


Dictionary<string, string> _defaultDimensions = new Dictionary<string, string>{
        {"Environment", "Prod"},
        {"Another", "One"}
    };


[Tracing]
[Metrics(CaptureColdStart = true)]
[Logging(LogEvent = true)]
async Task<Payload> FunctionHandler(Payload input, ILambdaContext context)
{
    Metrics.AddMetric("HandlerStartTime", context.RemainingTime.Milliseconds, MetricUnit.Milliseconds, MetricResolution.Standard);
    

    Tracing.AddAnnotation("fcn", context.FunctionName);

    Tracing.WithSubsegment("delaytime", async (subsegment) => 
    {
        await Task.Delay(DateTime.Now.Millisecond);
    });

    Tracing.WithSubsegment("runtime", async (subsegment) =>
    {
        await Task.Delay(DateTime.Now.Millisecond);
    });

    input.Queries.Add(new() 
    {
        QueryId = "poopy",
        QueryText = "What Time is it",
        IsValidQuery= true,
        Processed = false,
        Results = new[] { "CRIAG" }
    });
    
    return input;
};


var functionHandlerDelegate = FunctionHandler;


// to .NET types.
await LambdaBootstrapBuilder.Create(functionHandlerDelegate, new DefaultLambdaJsonSerializer())
        .Build()
        .RunAsync();




