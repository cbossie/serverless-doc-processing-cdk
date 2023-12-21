using Amazon.DynamoDBv2.DataModel;
using Amazon.StepFunctions.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DocProcessing.Shared.Model.Data.Expense;

public class DocumentExpenseGroup
{
    [JsonPropertyName("group")]
    [DynamoDBProperty("group")]
    public string Group { get; set; }

    [JsonPropertyName("values")]
    [DynamoDBProperty("values")]
    public List<DocumentExpenseSummary> GroupValues { get; set; } = new();
}

