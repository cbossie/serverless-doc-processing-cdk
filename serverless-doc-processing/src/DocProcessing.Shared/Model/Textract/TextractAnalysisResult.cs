﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DocProcessing.Shared.Model.Textract;

public class TextractAnalysisResult
{
    [JsonPropertyName("AnalyzeDocumentModelVersion")]
    public string? AnalyzeDocumentModelVersion { get; set; }

    [JsonPropertyName("Blocks")]
    public List<Block> Blocks { get; set; } = new();

    [JsonPropertyName("DocumentMetadata")]
    public DocumentMetadata? DocumentMetadata { get; set; }

    [JsonPropertyName("JobStatus")]
    public string? JobStatus { get; set; }

    [JsonPropertyName("StatusMessage")]
    public string? StatusMessage { get; set; }

    public int GetBlockCount() => Blocks.Count;
}
