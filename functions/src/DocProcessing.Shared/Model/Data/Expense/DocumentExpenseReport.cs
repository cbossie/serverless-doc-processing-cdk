using Amazon.DynamoDBv2.DataModel;
using Amazon.S3.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DocProcessing.Shared.Model.Data.Expense;

public class DocumentExpenseReport
{
    [JsonPropertyName("expenseGroups")]
    [DynamoDBProperty("expenseGroups")]
    public List<DocumentExpenseGroup> ExpenseGroups { get; set; } = new();

    [JsonPropertyName("expenseSummaryValues")]
    [DynamoDBProperty("expenseSummaryValues")]
    public List<DocumentExpenseSummary> ExpenseSummaryValues { get; set; } = new();

    [JsonPropertyName("expenseSummaryValues")]
    [DynamoDBProperty("expenseSummaryValues")]
    public Dictionary<int, List<DocumentExpenseLineItem>> LineItems { get; set; } = new();
}
