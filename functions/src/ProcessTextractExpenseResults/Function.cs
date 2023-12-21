using Amazon.Lambda.Annotations;
using Amazon.Lambda.Core;
using Amazon.XRay.Recorder.Handlers.AwsSdk;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
[assembly:LambdaGlobalProperties(GenerateMain = true)]

namespace ProcessTextractExpenseResults;

public class Function
{
    public Function()
    {
        AWSSDKHandler.RegisterXRayForAllServices();
    }

    [LambdaFunction()]
    public string FunctionHandler(string input, ILambdaContext context)
    {
        return input.ToUpper();
    }
}
