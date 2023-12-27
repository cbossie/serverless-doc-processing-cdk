using Amazon.DynamoDBv2.DataModel;
using DocProcessing.Shared.Model.Data.Expense;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ProcessingFunctions.Output;

public abstract class StepFunctionOutputBase
{
    
    public abstract bool Success { get; }

    [JsonPropertyName("ExternalId")]
    public string ExternalId { get; set; }

    [JsonPropertyName("Execution")]
    public string Execution { get; set; }

    [JsonPropertyName("InputBucket")]
    public string InputBucket { get; set; }

    [JsonPropertyName("InputKey")]
    public string InputKey { get; set; }

}
