using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DocProcessing.Shared.Model.Textract;

public class Polygon
{
    [JsonPropertyName("X")]
    public double X { get; set; }

    [JsonPropertyName("Y")]
    public double Y { get; set; }
}