using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DocProcessing.Shared.Model.Textract;

public class Prediction
{
    [JsonPropertyName("Confidence")]
    public double? Confidence { get; set; }

    [JsonPropertyName("Value")]
    public string? Value { get; set; }

}
