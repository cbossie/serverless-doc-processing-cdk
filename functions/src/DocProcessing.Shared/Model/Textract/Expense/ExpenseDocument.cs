using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DocProcessing.Shared.Model.Textract.Expense;

public class ExpenseDocument
{
    [JsonPropertyName("ExpenseIndex")]
    public int? ExpenseIndex { get; set; }

    [JsonPropertyName("SummaryFields")]
    public List<SummaryField> SummaryFields { get; set; }

    [JsonPropertyName("LineItemGroups")]
    public List<LineItemGroup> LineItemGroups { get; set; }

    [JsonPropertyName("Blocks")]
    public List<Block> Blocks { get; set; }
}