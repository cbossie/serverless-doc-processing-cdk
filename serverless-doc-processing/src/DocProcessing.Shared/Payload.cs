using System.Text.Json.Serialization;

namespace DocProcessing.Shared
{
    public class Payload
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
    }
}