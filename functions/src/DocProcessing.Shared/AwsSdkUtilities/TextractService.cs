using Amazon.DynamoDBv2;
using Amazon.S3;
using Amazon.S3.Model;
using DocProcessing.Shared.Model.Textract.Expense;
using DocProcessing.Shared.Model.Textract.QueryAnalysis;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using System.Net.Sockets;
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
        // Get all of the data and parse to keys
        List<Block> blocks = new();
        // Get the S3 objects
        var objects = GetS3Objects<TextractAnalysisResult>(bucket, key);
        
        await foreach (var data in objects)
        {
            if (data != null)
            {
                blocks.AddRange(data.Blocks);
            }
        }
        return new TextractDataModel(blocks);
    }

    public async Task<ExpenseDataModel> GetExpenses(string bucket, string key)
    {
        // Get all of the data and parse to keys
        List<ExpenseDocument> expenseDocs = new();

        // Get the S3 objects
        var objects = GetS3Objects<ExpenseResult>(bucket, key);

        await foreach (var data in objects)
        {
            if (data != null)
            {
                expenseDocs.AddRange(data.ExpenseDocuments);
            }
        }
        return new ExpenseDataModel(expenseDocs);
    }

    private async IAsyncEnumerable<TObjectType?> GetS3Objects<TObjectType>(string bucket, string key) 
    {
        var response = await S3Client.ListObjectsV2Async(new()
        {
            BucketName = bucket,
            Prefix = key
        }).ConfigureAwait(false);

        foreach(var item in response.S3Objects.Where(a => !a.Key.EndsWith("_access_check")))
        {
            var s3data = await S3Client.GetObjectAsync(bucket, item.Key).ConfigureAwait(false);
            var data = await JsonSerializer.DeserializeAsync<TObjectType>(new BufferedStream(s3data.ResponseStream)).ConfigureAwait(false);
            yield return data;
        }
    }
}



