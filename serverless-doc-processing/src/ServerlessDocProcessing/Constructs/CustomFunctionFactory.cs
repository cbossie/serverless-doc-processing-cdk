﻿using Amazon.CDK.AWS.Lambda;
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
	
	// Settings for all functions

	// 1 Environment variables to apply to all functions
	Dictionary<string, string> environmentVariables = new ();

	// 2 Timeout and Memory
	public int Memory { get; set; } = 1024;
    public int Timeout { get; set; } = 30;



    string EnvironmentName { get; }
	public CustomFunctionFactory(Construct scope, string environmentName)
	{
		Scope = scope;
		EnvironmentName = environmentName;
    }

	public void AddEnvironmentVariable(string key, string value) => environmentVariables[key] = value;


	public CustomFunction CreateCustomFunction(string functionName, bool compileAot = false, bool useArm = false)
	{
		CustomFunctionProps customProps = new()
		{
			Tracing = Tracing.ACTIVE,
			FunctionName = $"docProcessing{EnvironmentName}{functionName}",
			Handler = "bootstrap",
			Architecture = useArm ? Architecture.ARM_64 : Architecture.X86_64,
			Code = Code.FromAsset($"./functions/{functionName}.zip"),
			Runtime = compileAot ? Runtime.PROVIDED_AL2 : Runtime.PROVIDED,
			Timeout = Duration.Seconds(Timeout),
			MemorySize = Memory,
		};

		var fcn = new CustomFunction(Scope, functionName, customProps);
			fcn.AddEnvironment("ENVIRONMENT_NAME", EnvironmentName);

		foreach(var variable in environmentVariables) 
		{
			fcn.AddEnvironment(variable.Key, variable.Value);
		}

		return fcn;
	}
}
