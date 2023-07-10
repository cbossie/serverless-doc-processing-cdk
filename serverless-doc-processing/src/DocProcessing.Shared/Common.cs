using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.S3;
using Amazon.StepFunctions;
using Amazon.Textract;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using DocProcessing.Shared.AwsSdkUtilities;
using DocProcessing.Shared.Service;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocProcessing.Shared;

public class Common
{
    private ServiceCollection Services { get; } = new();


    public ServiceProvider? ServiceProvider { get; private set; }


    public static Common Instance { get; } = new();

    private Common()
    {
        Services = new ServiceCollection();
    }

    public async Task Initialize()
    {
        ConfigureServices();
        AWSSDKHandler.RegisterXRayForAllServices();
    }


    private void ConfigureServices()
    {
        ServiceProvider = 
        Services
            .AddTransient<IDataService, DataService>()
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
