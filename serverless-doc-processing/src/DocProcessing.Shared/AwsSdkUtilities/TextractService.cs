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
        S3Client = s3Client;
	}

    public async Task<TextractDataModel> GetBlocksForAnalysis(string bucket, string key)
    {
        // Get the S3 objects
        var objects = await S3Client.ListObjectsV2Async(new() 
        {
             BucketName = bucket,
             Prefix = key
        });


        // Get all of the data and parse to keys
        List<Block> blocks = new();
        foreach (var item in objects.S3Objects.Where(a => !a.Key.EndsWith("_access_check")))
        {
            var s3data = await S3Client.GetObjectAsync(item.BucketName, item.Key);
            var data = await JsonSerializer.DeserializeAsync<TextractAnalysisResult>(new BufferedStream(s3data.ResponseStream));
            blocks.AddRange(data.Blocks);
        }
        return new TextractDataModel(blocks);
    }
}
