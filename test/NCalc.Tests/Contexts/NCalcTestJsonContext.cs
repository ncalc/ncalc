using System.Text.Json.Serialization;
using NCalc;

[JsonSerializable(typeof(LogicalExpression))]
internal partial class NCalcTestJsonContext: JsonSerializerContext
{
}