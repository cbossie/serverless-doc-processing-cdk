namespace ServerlessDocProcessing;

public class ServerlessDocProcessingStack : Stack
{
    internal ServerlessDocProcessingStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
    {
        var fcn = new CustomFunction(this, "DotNet7Lambda", new CustomFunctionProps
        {

            FunctionName = "DotNet7Lambda",
            Handler = "bootstrap",
            Runtime = Runtime.PROVIDED,
            Architecture = Architecture.X86_64,
            Code = Code.FromAsset("./src/DotNet7Lambda"),
            BuildMethod = "makefile"
        });
    }
}
