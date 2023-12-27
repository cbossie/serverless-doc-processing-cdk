using DocProcessing.Shared.Model.Data.Expense;
using DocProcessing.Shared.Model.Data.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ProcessingFunctions.Output;

public class SuccessOutput : StepFunctionOutputBase
{
    [JsonPropertyName("Success")]
    public override bool Success => true;

    [JsonPropertyName("expenseReports")]
    public List<DocumentExpenseReport> ExpenseReports { get; set; } = new();

    [JsonPropertyName("queries")]
    public List<DocumentQuery> Queries { get; set; } = new();
}
