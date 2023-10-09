using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DocProcessing.Shared.Model.Textract.Expense;

public class LineItemGroup
{
    [JsonPropertyName("LineItemGroupIndex")]
    public int? LineItemGroupIndex { get; set; }

    [JsonPropertyName("LineItems")]
    public List<LineItem> LineItems { get; set; }
}