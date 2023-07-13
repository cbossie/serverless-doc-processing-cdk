using System.Collections.Generic;

namespace ServerlessDocProcessing.Constructs;

internal class CustomFunctionFactory
{
    Construct Scope { get; }

    // Settings for all functions

    // 1 Environment variables to apply to all functions
    Dictionary<string, string> environmentVariables = new();

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
        fcn.AddEnvironment(Constants.ConstantValues.ENVIRONMENT_NAME_VARIABLE, EnvironmentName);

        foreach (var variable in environmentVariables)
        {
            fcn.AddEnvironment(variable.Key, variable.Value);
        }

        return fcn;
    }
}
