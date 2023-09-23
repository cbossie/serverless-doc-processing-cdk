using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace DocProcessing.Shared.Model.Textract;

public class Block
{


    [JsonPropertyName("BlockType")]
    public string BlockType { get; set; } = string.Empty;

    [JsonPropertyName("ColumnIndex")]
    public int? ColumnIndex { get; set; }

    [JsonPropertyName("ColumnSpan")]
    public int? ColumnSpan { get; set; }

    [JsonPropertyName("Confidence")]
    public double? Confidence { get; set; }

    [JsonPropertyName("EntityTypes")]
    public List<string> EntityTypes { get; set; } = new();

    [JsonPropertyName("Geometry")]
    public Geometry? Geometry { get; set; }

    [JsonPropertyName("Hint")]
    public string? Hint { get; set; }

    [JsonPropertyName("Id")]
    [NotNull]
    public string? Id { get; set; }

    [JsonPropertyName("Page")]
    public int? Page { get; set; }

    [JsonPropertyName("PageClassification")]
    public PageClassification? PageClassification { get; set; }

    [JsonPropertyName("Query")]
    public Query Query { get; set; } = new();

    [JsonPropertyName("Relationships")]
    public List<Relationship> Relationships { get; set; } = new();

    [JsonPropertyName("RowIndex")]
    public int? RowIndex { get; set; }

    [JsonPropertyName("RowSpan")]
    public int? RowSpan { get; set; }

    [JsonPropertyName("SelectionStatus")]
    public string? SelectionStatus { get; set; }

    [JsonPropertyName("Text")]
    public string? Text { get; set; }

    [JsonPropertyName("TextType")]
    public string? TextType { get; set; }


    public List<string> GetRelationshipsByType(string relationshipType) => Relationships?.Where(r => r.Type == relationshipType).SelectMany(r => r.Ids).ToList() ?? new();

}
