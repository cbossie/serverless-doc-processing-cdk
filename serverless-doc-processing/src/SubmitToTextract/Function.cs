using Amazon.Lambda.CloudWatchEvents.S3Events;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using DocProcessing.Shared;
using System.Net.Http.Headers;

//Configure the Serializer
[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

await Common.Instance.Initialize();


var functionHandler = async (Payload input, ILambdaContext context) =>
{
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


// to .NET types.
await LambdaBootstrapBuilder.Create(functionHandler, new DefaultLambdaJsonSerializer())
        .Build()
        .RunAsync();




