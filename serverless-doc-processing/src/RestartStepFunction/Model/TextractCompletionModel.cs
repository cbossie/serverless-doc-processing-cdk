using Amazon.Textract.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RestartStepFunction.Model;

public class TextractCompletionModel
{

    public const string SUCCESS_STATUS = "SUCCEEDED";

    [JsonPropertyName("JobId")]
    public string JobId { get; set; }

    [JsonPropertyName("Status")]
    public string Status { get; set; }

    [JsonPropertyName("API")]
    public string API { get; set; }

    [JsonPropertyName("JobTag")]
    public string JobTag { get; set; }

    [JsonPropertyName("Timestamp")]
    public long Timestamp { get; set; }

    [JsonPropertyName("DocumentLocation")]
    public DocumentLocationModel DocumentLocation { get; set; }

    [JsonIgnore]
    public bool IsSuccess => Status == SUCCESS_STATUS;

}
