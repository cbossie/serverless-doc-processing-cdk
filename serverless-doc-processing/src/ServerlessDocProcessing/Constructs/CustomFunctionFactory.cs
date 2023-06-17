using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.S3.Assets;
using Amazon.CDK.AWS.SES;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerlessDocProcessing;

internal class CustomFunctionFactory
{
	Construct Scope { get; }
	public CustomFunctionFactory(Construct scope)
	{
		Scope = scope;
    }

	public CustomFunction CreateCustomFunction(string functionName, bool compileAot = false, bool useArm = false)
	{
		CustomFunctionProps customProps = new()
		{
			Tracing = Tracing.ACTIVE,
			FunctionName = functionName,
			Handler = "bootstrap",
			Architecture = useArm ? Architecture.ARM_64 : Architecture.X86_64,
			Code = Code.FromAsset($"./functions/{functionName}.zip"),
			Runtime = compileAot ? Runtime.PROVIDED_AL2 : Runtime.PROVIDED
		};

		return new CustomFunction(Scope, functionName, customProps);

	}
}
