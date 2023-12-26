using Amazon.Lambda.Annotations;
using Amazon.Lambda.Core;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using AWS.Lambda.Powertools.Logging;
using AWS.Lambda.Powertools.Metrics;
using AWS.Lambda.Powertools.Tracing;
using System.Threading.Tasks;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
[assembly:LambdaGlobalProperties(GenerateMain = true)]

namespace ProcessTextractExpenseResults;

public class Function
{

    private ITextractService _textractService;
    private IDataService _dataService;

    public Function(ITextractService textractService, IDataService dataService)
    {
        _textractService = textractService;
        _dataService = dataService;    
        AWSSDKHandler.RegisterXRayForAllServices();
    }

    [Tracing]
    [Metrics(CaptureColdStart = true)]
    [Logging(LogEvent = true)]
    [LambdaFunction()]
    public async Task<IdMessage> FunctionHandler(IdMessage input, ILambdaContext context)
    {
        var processData = await _dataService.GetData<ProcessData>(input.Id).ConfigureAwait(false);

        // Get the step functions Result
        var textractExpenseModel = await _textractService.GetExpenses(processData.OutputBucket, processData.OutputKey).ConfigureAwait(false);

        // TODO - Get Expense Data and save to the DB
        //
        // CODE HERE
        //
        //

        // Save the query results back to the database, and clear the task token
        processData.TextractJobId = null;
        processData.TextractTaskToken = null;

        await _dataService.SaveData(processData).ConfigureAwait(false);        

        return input;

    }
}
