using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DocProcessing.Shared.Model.Textract;

public class Geometry
{
    [JsonPropertyName("BoundingBox")]
    public BoundingBox? BoundingBox { get; set; }

    [JsonPropertyName("Polygon")] 
    public List<Polygon> Polygon { get; set; } = new();
}
