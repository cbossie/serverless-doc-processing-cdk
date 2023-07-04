using Amazon.S3;
using Amazon.Textract;
using DocProcessing.Shared.Model.Data;
using DocProcessing.Shared.Model.Textract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

    public async Task<List<Block>> GetBlocksForAnalysis(string jobId, string bucket, string prefix)
    {
        List<Block> blocks = new();

        // Get the S3 objects
        var objects = await S3Client.ListObjectsV2Async(new() 
        {
             BucketName = bucket,
             Prefix = $"{prefix}/{jobId}"
        });

        // Get all of the data and parse to keys
        foreach(var item in objects.S3Objects.Where(a => !a.Key.StartsWith(".")))
        {
            



        }








        return blocks;

    }
}
