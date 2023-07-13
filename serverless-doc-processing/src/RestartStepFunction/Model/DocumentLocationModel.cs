using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RestartStepFunction.Model;

public class DocumentLocationModel
{
    [JsonPropertyName("S3ObjectName")]
    public string? S3ObjectName { get; set; }

    [JsonPropertyName("S3Bucket")]
    public string? S3Bucket { get; set; }
}
