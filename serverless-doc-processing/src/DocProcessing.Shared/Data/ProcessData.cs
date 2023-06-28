using Amazon.DynamoDBv2.DataModel;
using DocProcessing.Shared.Query;
using System.Text.Json.Serialization;

namespace DocProcessing.Shared
{
    [DynamoDBTable(Constants.ResourceNames.PROCESS_DATA_TABLE)]
    public class ProcessData
    {
        public ProcessData()
        {

        }

        public ProcessData(string id)
        {
            Id = id;
        }

        [DynamoDBHashKey("id")]
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [DynamoDBProperty("externalId")]
        [JsonPropertyName("externalId")]
        public string ExternalId { get; set; }

        [DynamoDBProperty("queries")]
        [JsonPropertyName("queries")]
        public List<QueryConfig> Queries { get; set; } = new List<QueryConfig>();

        [DynamoDBProperty("isValid")]
        [JsonPropertyName("isValid")]
        public bool IsValid { get; set; } = true;


        [DynamoDBProperty("inputDocKey")]
        [JsonPropertyName("inputDocKey")]
        public string InputDocKey { get; set; }

        [DynamoDBProperty("inputDocBucket")]
        [JsonPropertyName("inputDocBucket")]
        public string InputDocBucket { get; set; }

        [DynamoDBProperty("fileExtension")]
        [JsonPropertyName("fileExtension")]
        public string FileExtension { get; set; }

        [DynamoDBProperty("textractTaskToken")]
        [JsonPropertyName("textractTaskToken")]
        public string TextractTaskToken { get; set; }

        [DynamoDBProperty("textractJobId")]
        [JsonPropertyName("textractJobId")]
        public string TextractJobId { get; set; }

    }
}