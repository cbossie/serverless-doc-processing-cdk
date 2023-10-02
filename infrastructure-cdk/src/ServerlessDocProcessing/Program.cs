using ServerlessDocProcessing;

var app = new App();


string environmentName = $"{app.Node.TryGetContext("environmentName")}" ?? "dev";
string stackName = $"{app.Node.TryGetContext("stackName")}-{environmentName}";
string account = $"{app.Node.TryGetContext("account")}";
string region = $"{app.Node.TryGetContext("region")}";
string functionBaseDir = $"{app.Node.TryGetContext("functionBaseDirectory")}";


new ServerlessDocProcessingStack(app, "ServerlessDocProcessingStack", new ServerlessDocProcessingStackProps
{
    StackName = stackName ?? "ServerlessDocProcessing",
    EnvironmentName = environmentName ?? "dev",
    FunctionCodeBaseDirectory = functionBaseDir ?? "../functions"
});
app.Synth();