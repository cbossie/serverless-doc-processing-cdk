using ServerlessDocProcessing;

var app = new App();


string environmentName = $"{app.Node.TryGetContext("environmentName")}" ?? "dev";
string stackName = $"{app.Node.TryGetContext("stackName")}" ?? $"ServerlessDocProcessingStack-{environmentName}";
string account = $"{app.Node.TryGetContext("account")}";
string region = $"{app.Node.TryGetContext("region")}";


new ServerlessDocProcessingStack(app, "ServerlessDocProcessingStack", new ServerlessDocProcessingStackProps
{
    StackName = stackName ?? "ServerlessDocProcessing",
    EnvironmentName = environmentName ?? "dev",
    // If you don't specify 'env', this stack will be environment-agnostic.
    // Account/Region-dependent features and context lookups will not work,
    // but a single synthesized template can be deployed anywhere.

    // Uncomment the next block to specialize this stack for the AWS Account
    // and Region that are implied by the current CLI configuration.

    Env = makeEnv(account, region)


    // Uncomment the next block if you know exactly what Account and Region you
    // want to deploy the stack to.
    /*
    Env = new Amazon.CDK.Environment
    {
        Account = "123456789012",
        Region = "us-east-1",
    }
    */

    // For more information, see https://docs.aws.amazon.com/cdk/latest/guide/environments.html
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