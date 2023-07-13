namespace ServerlessDocProcessing.Constructs;

public class CustomFunction : Function
{
    public CustomFunction(Construct scope, string id, CustomFunctionProps props)
        : base(scope, id, props)
    {
        var cfnFcn = (CfnFunction)Node.DefaultChild;
        // For Future Use with SAM
        cfnFcn.AddMetadata("BuildMethod", props.BuildMethod);
        cfnFcn.OverrideLogicalId(props.FunctionName);
    }
}