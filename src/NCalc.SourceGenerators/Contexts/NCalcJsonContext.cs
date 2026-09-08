using System.Text.Json.Serialization;
using NCalc.SourceGenerators.Models;

[JsonSerializable(typeof(NumericTypeMetadata))]
internal partial class NCalcJsonContext : JsonSerializerContext
{
}
