using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DocProcessing.Shared.Model.Textract.Expense
{
    public class Type
    {
        [JsonPropertyName("Text")]
        public string? Text { get; set; }

        [JsonPropertyName("Confidence")]
        public double? Confidence { get; set; }        
    }
}
