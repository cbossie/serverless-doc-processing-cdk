using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;
using Amazon.Lambda.Annotations;
using Amazon.S3;
using Amazon.StepFunctions;
using Amazon.Textract;
using DocProcessing.Shared.AwsSdkUtilities;
using DocProcessing.Shared.Service;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocProcessing.Shared;

[LambdaStartup]
public partial class LambdaStartup
{
    public void ConfigureServices(IServiceCollection services)
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
                        TableNamePrefix = $"{Environment.GetEnvironmentVariable(Constants.ConstantValues.ENVIRONMENT_NAME_VARIABLE)}-"
                    }))
            .BuildServiceProvider();
    }
}
