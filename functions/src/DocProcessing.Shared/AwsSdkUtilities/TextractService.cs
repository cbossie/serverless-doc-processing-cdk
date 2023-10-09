using Amazon.S3;
using Amazon.S3.Model;
using DocProcessing.Shared.Model.Textract.Expense;
using DocProcessing.Shared.Model.Textract.QueryAnalysis;
using System.Text.Json;

namespace DocProcessing.Shared.AwsSdkUtilities;

public class TextractService : ITextractService
{
    IAmazonS3 S3Client { get; }

    public TextractService(IAmazonS3 s3Client)
    {
        S3Client = s3Client;
    }

    public async Task<TextractDataModel> GetBlocksForAnalysis(string bucket, string key)
    {
        // Get the S3 objects
        var objects = await S3Client.ListObjectsV2Async(new()
        {
            BucketName = bucket,
            Prefix = key
        }).ConfigureAwait(false);


        // Get all of the data and parse to keys
        List<Block> blocks = new();
        foreach (var item in objects.S3Objects.Where(a => !a.Key.EndsWith("_access_check")))
        {
            var s3data = await S3Client.GetObjectAsync(item.BucketName, item.Key).ConfigureAwait(false);
            var data = await JsonSerializer.DeserializeAsync<TextractAnalysisResult>(new BufferedStream(s3data.ResponseStream)).ConfigureAwait(false);
            if (data != null)
            {
                blocks.AddRange(data.Blocks);
            }
        }
        return new TextractDataModel(blocks);
    }

    public Task<ExpenseDataModel> GetExpenses(string bucket, string key)
    {
        
    }

    private async Task<IEnumerable<S3Object> GetS3Objects(string bucket, string key)
        {



}
