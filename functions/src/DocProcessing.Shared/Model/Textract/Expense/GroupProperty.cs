using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DocProcessing.Shared.Model.Textract.Expense;

public class GroupProperty
{
    [JsonPropertyName("Types")]
    public List<string> Types { get; set; }

    [JsonPropertyName("Id")]
    public string Id { get; set; }
}