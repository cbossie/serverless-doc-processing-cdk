using DocProcessing.Shared.Query;
using System.Text.Json.Serialization;

namespace DocProcessing.Shared
{
    public class Payload
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("externalId")]
        public string ExternalId { get; set; }

        [JsonPropertyName("queries")]
        public List<QueryConfig> Queries { get; set; } = new List<QueryConfig>();

        [JsonPropertyName("isValid")]
        public bool IsValid { get; set; } = true;

        [JsonPropertyName("inputDocKey")]
        public string InputDocKey { get; set; }
    }
}