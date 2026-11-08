using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NCalc;

/// <summary>
/// Preserves expression values, including arbitrary-precision JSON integer tokens.
/// </summary>
public sealed class ValueExpressionValueJsonConverter : JsonConverter<object>
{
    public override bool HandleNull => true;

    public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        // Type may appear before or after Value. Defer conversion until the
        // owning expression can distinguish exact integers from legacy values.
        return new DeferredValue(document.RootElement.Clone(), options);
    }

    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        if (value is BigInteger integer)
        {
            writer.WriteRawValue(integer.ToString(CultureInfo.InvariantCulture));
            return;
        }

        JsonSerializer.Serialize(writer, value, options.GetTypeInfo(typeof(object)));
    }

    internal sealed class DeferredValue(JsonElement element, JsonSerializerOptions options)
    {
        internal JsonElement Element { get; } = element;

        internal object? ReadLegacyValue() => Element.Deserialize(options.GetTypeInfo(typeof(object)));
    }
}
