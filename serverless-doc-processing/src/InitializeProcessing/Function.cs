using Amazon.Lambda.CloudWatchEvents.S3Events;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using DocProcessing.Shared;


Common.ConfigureServices();


var functionHandler = async (S3ObjectCreateEvent input, ILambdaContext context) =>
{
    Payload pl = new();
    pl.Id = Guid.NewGuid().ToString();
    return pl;
};


// to .NET types.
await LambdaBootstrapBuilder.Create(functionHandler, new DefaultLambdaJsonSerializer())
        .Build()
        .RunAsync();






