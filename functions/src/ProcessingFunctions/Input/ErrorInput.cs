using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ProcessingFunctions.Input;

public class ErrorInput
{
    [JsonPropertyName("execution")]
    public string Execution { get; set; }

    [JsonPropertyName("error")]
    public string Error { get; set; }

    [JsonPropertyName("cause")]
    public string Cause{ get; set; }
}
