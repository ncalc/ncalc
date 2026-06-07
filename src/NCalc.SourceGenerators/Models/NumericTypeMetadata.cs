using System.Text.Json.Serialization;

namespace NCalc.SourceGenerators.Models;

internal sealed class NumericTypeMetadata
{
    [JsonPropertyName("builtInTypeOrder")]
    public string[] BuiltInTypeOrder { get; init; } = [];

    [JsonPropertyName("numberPrecedence")]
    public string[] NumberPrecedence { get; init; } = [];

    [JsonPropertyName("types")]
    public NumericTypeDefinition[] Types { get; init; } = [];

    public NumericTypeDefinition[] BinaryOperatorTypes =>
        Types.Where(type => type.SupportsBinaryOperators).ToArray();

    public NumericTypeDefinition[] PrimitiveConversionTypes =>
        Types.Where(type => type.ImplicitConversions is { Length: > 0 }).ToArray();

    public NumericTypeDefinition[] TypeCodeExpandBitsTypes =>
        Types.Where(type => type.ExpandBitsTypeCode != null).ToArray();
}