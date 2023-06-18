using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.S3;
using Amazon.StepFunctions;
using Amazon.Textract;
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
    }


    private void ConfigureServices()
    {
        Services.AddAWSService<IAmazonS3>();
        Services.AddAWSService<IAmazonTextract>();
        Services.AddAWSService<IAmazonStepFunctions>();
        Services.AddAWSService<IAmazonDynamoDB>();
        Services.AddTransient<IDynamoDBContext>(c => new
            DynamoDBContext(c.GetService<IAmazonDynamoDB>(),
                new DynamoDBContextConfig
                {
                    TableNamePrefix = $"{Environment.GetEnvironmentVariable("ENVIRONMENT_NAME")}-"
                }));
        ServiceProvider = Services.BuildServiceProvider();
    }
}
