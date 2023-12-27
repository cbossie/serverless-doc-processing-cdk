using Amazon.CDK.AWS.SES.Actions;
using ServerlessDocProcessing;

var app = new App();


string environmentName = $"{app.Node.TryGetContext("environmentName")}" ?? "dev";
string stackName = $"{app.Node.TryGetContext("stackName")}-{environmentName}";
string functionBaseDir = $"{app.Node.TryGetContext("functionBaseDirectory")}";

_ = new ServerlessDocProcessingStack(app, "ServerlessDocProcessingStack", new ServerlessDocProcessingStackProps
{
    StackName = stackName ?? "ServerlessDocProcessing",
    EnvironmentName = environmentName ?? "dev",
    FunctionCodeBaseDirectory = !string.IsNullOrEmpty(functionBaseDir) ? functionBaseDir : "./functions"
});
app.Synth();