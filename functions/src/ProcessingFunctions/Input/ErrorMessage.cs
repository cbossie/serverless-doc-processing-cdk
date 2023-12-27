using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ProcessingFunctions.Input;

public class ErrorMessage
{
    [JsonPropertyName("Error")]
    public string Error { get; set; }
    
    [JsonPropertyName("Cause")]
    public string Cause { get; set; }
}
