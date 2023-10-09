using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DocProcessing.Shared.Model.Textract.Expense;

public class ExpenseResult
{
    [JsonPropertyName("DocumentMetadata")]
    public DocumentMetadata DocumentMetadata { get; set; }

    [JsonPropertyName("ExpenseDocuments")]
    public List<ExpenseDocument> ExpenseDocuments { get; set; }
}
