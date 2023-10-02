using ServerlessDocProcessing;

var app = new App();


string environmentName = $"{app.Node.TryGetContext("environmentName")}" ?? "dev";
string stackName = $"{app.Node.TryGetContext("stackName")}" ?? $"ServerlessDocProcessingStack-{environmentName}";
string account = $"{app.Node.TryGetContext("account")}";
string region = $"{app.Node.TryGetContext("region")}";
string functionBaseDir = $"{app.Node.TryGetContext("functionBaseDirectory")}";


new ServerlessDocProcessingStack(app, "ServerlessDocProcessingStack", new ServerlessDocProcessingStackProps
{
    StackName = stackName ?? "ServerlessDocProcessing",
    EnvironmentName = environmentName ?? "dev",
    FunctionCodeBaseDirectory = functionBaseDir ?? "../functions"
//    Env = makeEnv(account, region)
});
app.Synth();


Amazon.CDK.Environment makeEnv(string account = null, string region = null)
{
    return new Amazon.CDK.Environment
    {
        Account = account ??
            System.Environment.GetEnvironmentVariable("CDK_DEPLOY_ACCOUNT") ??
            System.Environment.GetEnvironmentVariable("CDK_DEFAULT_ACCOUNT"),
        Region = region ??
            System.Environment.GetEnvironmentVariable("CDK_DEPLOY_REGION") ??
            System.Environment.GetEnvironmentVariable("CDK_DEFAULT_REGION")
    };
}