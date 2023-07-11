using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DocProcessing.Shared.Model.Data;

public class DocumentQueryResult
{
    [JsonPropertyName("confidence")]
    [DynamoDBProperty("confidence")]
    public double? Confidence { get; set; } = 0d;

    [JsonPropertyName("resultText")]
    [DynamoDBProperty("resultText")]
    public string ResultText { get; set; }

}
