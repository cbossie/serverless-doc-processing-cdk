﻿using System.Text.Json.Serialization;

namespace DocProcessing.Shared.Model.Textract;

public class Relationship
{
    [JsonPropertyName("Ids")]
    public List<string> Ids { get; set; } = new();

    [JsonPropertyName("Type")]
    public string? Type { get; set; }


}