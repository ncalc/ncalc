using System.Text.Json.Serialization;

namespace NCalc.SourceGenerators.Models;

internal sealed class NumericTypeDefinition
{
    [JsonPropertyName("keyword")]
    public string Keyword { get; set; } = string.Empty;

    [JsonPropertyName("typeCode")]
    public string TypeCode { get; set; } = string.Empty;

    [JsonPropertyName("isReal")]
    public bool IsReal { get; set; }

    [JsonPropertyName("bitSize")]
    public int BitSize { get; set; }

    [JsonPropertyName("supportsNaN")]
    public bool SupportsNaN { get; set; }

    [JsonPropertyName("isUnsigned")]
    public bool IsUnsigned { get; set; }

    [JsonPropertyName("supportsBinaryOperators")]
    public bool SupportsBinaryOperators { get; set; }

    [JsonPropertyName("supportsSameTypeComparison")]
    public bool SupportsSameTypeComparison { get; set; }

    [JsonPropertyName("expandBitsTypeCode")]
    public string? ExpandBitsTypeCode { get; set; }

    [JsonPropertyName("implicitConversions")]
    public string[] ImplicitConversions { get; set; } = [];
}
