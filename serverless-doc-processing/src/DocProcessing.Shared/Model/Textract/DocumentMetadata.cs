using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DocProcessing.Shared.Model.Textract;

public class DocumentMetadata
{
    [JsonPropertyName("Pages")]
    public int Pages { get; set; }
}
