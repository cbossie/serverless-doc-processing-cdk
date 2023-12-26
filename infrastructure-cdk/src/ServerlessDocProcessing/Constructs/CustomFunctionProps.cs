using System.Collections.Generic;

namespace ServerlessDocProcessing.Constructs;

public class CustomFunctionProps
{

    // Static Properties used across functions
    public static int GlobalTimeout { get; set; } = 30;

    public static int GlobalMemory { get; set; } = 1024;

    public static Dictionary<string, string> GlobalEnvironment { get; } = new();

    public static string EnvironmentName { get; set; }

    public static string FunctionBaseDirectory { get; set; }  

    // Per-Instance properties

    public int? Memory { get; set; }
    public int? Timeout { get; set; }
    public string BuildMethod { get; set; }

    public string FunctionNameBase { get; set; }

    public string CodeBaseDirectory { get; set; }
    public string FunctionCodeDirectory { get; set; }    

    public string Description { get; set; }

    public string FunctionName => $"docProcessing{FunctionNameBase}{EnvironmentName}";

    public FunctionProps FunctionProps => new FunctionProps
    {
        Tracing = Tracing.ACTIVE,
        Handler = "bootstrap",
        FunctionName = FunctionName,
        Architecture = Architecture.X86_64,
        Runtime = Runtime.PROVIDED_AL2,
        Timeout = Duration.Seconds(Timeout ?? GlobalTimeout),
        MemorySize = Memory ?? GlobalMemory,
        Code = Code.FromAsset($"{FunctionBaseDirectory}/{FunctionCodeDirectory ?? FunctionNameBase}.zip"),
        Description = Description, 
    };

    public static void AddGlobalEnvironment(string key, string  value) => GlobalEnvironment[key] = value;

    

}
