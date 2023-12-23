using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DocProcessing.Shared.Model.Textract;

public class BoundingBox
{
    [JsonPropertyName("Height")]
    public double? Height { get; set; }

    [JsonPropertyName("Left")]
    public double? Left { get; set; }

    [JsonPropertyName("Top")]
    public double? Top { get; set; }

    [JsonPropertyName("Width")]
    public double? Width { get; set; }
}