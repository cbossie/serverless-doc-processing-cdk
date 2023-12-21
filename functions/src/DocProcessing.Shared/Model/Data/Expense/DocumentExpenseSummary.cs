using Amazon.DynamoDBv2.DataModel;
using Amazon.S3.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DocProcessing.Shared.Model.Data.Expense;

public class DocumentExpenseSummary
{
    [JsonPropertyName("type")]
    [DynamoDBProperty("type")]
    public string Type { get; set; }

    [JsonPropertyName("label")]
    [DynamoDBProperty("label")]
    public string Label { get; set; }

    [JsonPropertyName("value")]
    [DynamoDBProperty("value")]
    public string Value { get; set; }

    [JsonPropertyName("currency")]
    [DynamoDBProperty("currency")]
    public string Currency { get; set; }
}

