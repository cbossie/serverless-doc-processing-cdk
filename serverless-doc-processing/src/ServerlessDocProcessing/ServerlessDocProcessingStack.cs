using Amazon.CDK;
using Amazon.CDK.AWS.Lambda;
using Constructs;

namespace ServerlessDocProcessing
{
    public class ServerlessDocProcessingStack : Stack
    {
        internal ServerlessDocProcessingStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            new Function(this, "samplefunction", new FunctionProps
            {
                Code = Code.FromAsset("./src/SampleFunction"), 
                Handler = "SampleFunction::SampleFunction.Function::FunctionHandler",
                Architecture = Architecture.ARM_64,
                Runtime = Runtime.DOTNET_6

            });
        }
    }
}
