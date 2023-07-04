using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ThirdParty.Json.LitJson;

namespace DocProcessing.Shared.Model.Data;

public class IdMessage
{
    [JsonPropertyName("id")]
    public virtual string Id { get; set; }

    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; }

    [JsonPropertyName("taskToken")]
    public string TaskToken { get; set; }

    public static IdMessage Create(string id, bool success = true, string message = null) =>
        new IdMessage
        {
            Id = id,
            Success = success,
            Message = message
        };
}
