using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerlessDocProcessing.Constructs;

public class CustomFunction : Function
{
	public CustomFunction(Construct scope, string id, CustomFunctionProps props)
		: base(scope, id, props)
	{
		var cfnFcn = (CfnFunction)Node.DefaultChild;
		cfnFcn.AddMetadata("BuildMethod", props.BuildMethod);
		cfnFcn.OverrideLogicalId(props.FunctionName);
	}
}