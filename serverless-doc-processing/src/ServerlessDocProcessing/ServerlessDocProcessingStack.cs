using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.Lambda;
using Constructs;

namespace ServerlessDocProcessing
{
    public class ServerlessDocProcessingStack : Stack
    {
        internal ServerlessDocProcessingStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            //new Function(this, "samplefunction", new FunctionProps
            //{
            //    Code = Code.FromAsset("./src/SampleFunction"), 
            //    Handler = "SampleFunction::SampleFunction.Function::FunctionHandler",
            //    Architecture = Architecture.ARM_64,
            //    Runtime = Runtime.DOTNET_6

            //});

            var fcn = new Function(this, "DotNet7Lambda", new FunctionProps
            {

                FunctionName = "DotNet7Lambda",
                Handler = "bootstrap",
                Runtime = Runtime.PROVIDED,
                Architecture = Architecture.X86_64,
                Code = Code.FromAsset("./src/DotNet7Lambda"),                
            });
                        
            var cfnFcn = (CfnFunction)fcn.Node.DefaultChild;
            cfnFcn.AddMetadata("BuildMethod", "makefile");
            cfnFcn.OverrideLogicalId("DotNet7Lambda");

        }
    }
}
