using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DocProcessing.Shared.Model.Textract;

public class Query
{
    [JsonPropertyName("Text")]
    public string? Text { get; set; }

    [JsonPropertyName("Alias")]
    public string? Alias { get; set; }

    [JsonPropertyName("Pages")]
    public List<string> Pages { get; set; }
}
