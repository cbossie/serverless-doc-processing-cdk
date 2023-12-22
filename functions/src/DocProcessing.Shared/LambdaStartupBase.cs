using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;
using Amazon.S3;
using Amazon.StepFunctions;
using Amazon.Textract;
using DocProcessing.Shared.AwsSdkUtilities;
using DocProcessing.Shared.Service;
using Microsoft.Extensions.DependencyInjection;

public abstract class StartupBase
{
    public virtual void ConfigureServices(IServiceCollection services)
    {

        services.AddTransient<IDataService, DataService>()
            .AddTransient<ITextractService, TextractService>()
            .AddAWSService<IAmazonS3>()
            .AddAWSService<IAmazonTextract>()
            .AddAWSService<IAmazonStepFunctions>()
            .AddAWSService<IAmazonDynamoDB>()
            .AddTransient<IDynamoDBContext>(c => new
                DynamoDBContext(c.GetService<IAmazonDynamoDB>(),
                    new DynamoDBContextConfig
                    {
                        TableNamePrefix = $"{Environment.GetEnvironmentVariable(DocProcessing.Constants.ConstantValues.ENVIRONMENT_NAME_VARIABLE)}-"
                    }))
            .BuildServiceProvider();
    }
}
