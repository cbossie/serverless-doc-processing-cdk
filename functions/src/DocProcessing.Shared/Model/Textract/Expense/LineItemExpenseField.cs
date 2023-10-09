using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DocProcessing.Shared.Model.Textract.Expense;

public class LineItemExpenseField
{
    [JsonPropertyName("Type")]
    public Type Type { get; set; }

    [JsonPropertyName("LabelDetection")]
    public LabelDetection LabelDetection { get; set; }

    [JsonPropertyName("ValueDetection")]
    public ValueDetection ValueDetection { get; set; }

    [JsonPropertyName("PageNumber")]
    public int? PageNumber { get; set; }
}