using Amazon.DynamoDBv2.DataModel;
using Amazon.S3.Model;
using Amazon.StepFunctions.Model;
using DocProcessing.Shared.Model.Textract.Expense;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
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

    [JsonPropertyName("scalarSummaryItems")]
    [DynamoDBProperty("scalarSummaryItems")]
    public List<DocumentExpenseSummary> ScalarExpenseSummaryValues { get; set; } = new();

    public void AddScalarExpenseSummaryValue(string currency, string label, string type, string value) =>
        ScalarExpenseSummaryValues.Add(new DocumentExpenseSummary { Currency = currency, Label = label, Type = type, Value = value });







}