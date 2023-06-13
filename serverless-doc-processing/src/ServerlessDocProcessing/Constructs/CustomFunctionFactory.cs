using Amazon.CDK.AWS.SES;
using System;
using System.Collections.Generic;
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

	public CustomFunction CreateCustomFUnction(string functionName, bool compileAot = false, bool useArm = false)
	{
		CustomFunctionProps customProps = new()
		{
			Tracing = Tracing.ACTIVE,
			FunctionName = functionName,
			Handler = "bootstrap",
			Architecture = useArm ? Architecture.ARM_64 : Architecture.X86_64,
			Code = Code.FromAsset($"./src/{functionName}")
		};

		if(compileAot)
		{
			customProps.Runtime = Runtime.PROVIDED_AL2;
			customProps.BuildMethod = "dotnet7";
		}
		else
		{
			customProps.Runtime = Runtime.PROVIDED;
			customProps.BuildMethod = "makefile";
		}

		return new CustomFunction(Scope, functionName, customProps);

	}


}
