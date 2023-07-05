using Amazon.Runtime.Internal.Util;
using Amazon.S3;
using Amazon.Textract;
using DocProcessing.Shared.Model.Data;
using DocProcessing.Shared.Model.Textract;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DocProcessing.Shared.AwsSdkUtilities;

public class TextractService : ITextractService
{
    IAmazonTextract TextractClient { get; }

    IAmazonS3 S3Client { get; }

	public TextractService(IAmazonTextract textractClient, IAmazonS3 s3Client)
	{
		TextractClient = textractClient;
	}

    public async Task<TextractDataModel> GetBlocksForAnalysis(string jobId, string bucket, string prefix)
    {
        // Get the S3 objects
        var objects = await S3Client.ListObjectsV2Async(new() 
        {
             BucketName = bucket,
             Prefix = $"{prefix}/{jobId}"
        });


        // Get all of the data and parse to keys
        List<Block> blocks = new();
        foreach (var item in objects.S3Objects.Where(a => !a.Key.StartsWith(".")))
        {
            var s3data = await S3Client.GetObjectAsync(bucket, $"{prefix}/{jobId}");
            var data = await JsonSerializer.DeserializeAsync<TextractAnalysisResult>(new BufferedStream(s3data.ResponseStream));
            blocks.AddRange(data.Blocks);
        }
        return new TextractDataModel(blocks);
    }
}
