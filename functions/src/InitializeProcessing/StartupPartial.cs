﻿using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;
using Amazon.Lambda.Annotations;
using Amazon.S3;
using Amazon.StepFunctions;
using Amazon.Textract;

namespace DocProcessing.Shared;

[LambdaStartup]
public partial class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);
    }
}