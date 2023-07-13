using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DocProcessing.Shared.Model.Textract;

public class Relationship
{
    [JsonPropertyName("Ids")]
    public List<string> Ids { get; set; } = new();

    [JsonPropertyName("Type")]
    public string? Type { get; set; }


}