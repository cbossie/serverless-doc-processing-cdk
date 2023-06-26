using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DocProcessing.Shared.Query
{
    public class QueryConfig
    {
        [JsonPropertyName("queryId")]
        public string QueryId { get; set; }

        [JsonPropertyName("queryText")]
        public string QueryText { get; set; }

        [JsonPropertyName("isValidQuery")]
        public bool IsValidQuery { get; set; }

        [JsonPropertyName("processed")]
        public bool Processed { get; set; }

        [JsonPropertyName("queryResults")]
        public List<string> Results { get; set; } = new List<string>();
    }
}
