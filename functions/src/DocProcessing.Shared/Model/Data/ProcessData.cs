using Amazon.DynamoDBv2.DataModel;
using DocProcessing.Shared.Model.Data.Expense;
using DocProcessing.Shared.Model.Data.Query;
using System.Text.Json.Serialization;

namespace DocProcessing.Shared.Model.Data
{
    [DynamoDBTable(Constants.ResourceNames.PROCESS_DATA_TABLE)]
    public class ProcessData : IdMessage
    {
        public ProcessData(string id)
            : this()
        {
            Id = id;
        }

        public ProcessData()
        {

        }

        [DynamoDBHashKey("id")]
        [JsonPropertyName("id")]
        public override string Id { get; set; } = string.Empty;

        [DynamoDBProperty("externalId")]
        [JsonPropertyName("externalId")]
        public string ExternalId { get; set; }

        [DynamoDBProperty("queries")]
        [JsonPropertyName("queries")]
        public List<DocumentQuery> Queries { get; set; } = new();
        
        [DynamoDBProperty("expenseReports")]
        [JsonPropertyName("expenseReports")]
        public List<DocumentExpenseReport> ExpenseReports { get; set; } = new();

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

        [DynamoDBProperty("outputBucket")]
        [JsonPropertyName("outputBucket")]
        public string OutputBucket { get; set; } = string.Empty;

        [DynamoDBProperty("outputKey")]
        [JsonPropertyName("outputKey")]
        public string OutputKey { get; set; } = string.Empty;

    }
}