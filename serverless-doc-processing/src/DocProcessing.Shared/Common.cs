using Amazon;
using Amazon.Auth.AccessControlPolicy.ActionIdentifiers;
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
    private static bool Initialized { get; set; } = false;

    public ServiceProvider ServiceProvider { get; private set; }

    public static Common Instance { get; } = new();

    public static ServiceProvider Services => Instance.ServiceProvider;
   
    static Common()
    {
        Instance = new Common();
    }

    public async Task Initialize()
    {
        if (Initialized)
        {
            return;
        }
        ConfigureServices();
        AWSSDKHandler.RegisterXRayForAllServices();
        await Prime();
    }

    private async Task Prime()
    {
        // TODO - Prime the AWS Services Here
        await Task.CompletedTask;
    }

    private void ConfigureServices()
    {
        ServiceCollection services = new ServiceCollection();

        ServiceProvider = services
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
