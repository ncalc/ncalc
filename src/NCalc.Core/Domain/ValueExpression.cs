using System.Text.Json;
using System.Text.Json.Serialization;
using NCalc.Exceptions;
using NCalc.Visitors;

namespace NCalc.Domain;

public sealed class ValueExpression : LogicalExpression , IJsonOnDeserialized
{
    public object? Value { get; set; }
    public ValueType Type { get; set; }

    public ValueExpression()
    {
    }

    public ValueExpression(object value)
    {
        Type = value switch
        {
            bool => ValueType.Boolean,
            DateTime => ValueType.DateTime,
            TimeSpan => ValueType.TimeSpan,
            Guid => ValueType.Guid,
            char => ValueType.Char,
            decimal or double or float => ValueType.Float,
            byte or sbyte or short or int or long or ushort or uint or ulong => ValueType.Integer,
            string => ValueType.String,
            _ => throw new NCalcException("This value could not be handled: " + value)
        };

        Value = value;
    }

    public ValueExpression(string? value)
    {
        Value = value;
        Type = ValueType.String;
    }

    public ValueExpression(char value)
    {
        Value = value;
        Type = ValueType.Char;
    }

    public ValueExpression(int value)
    {
        Value = value;
        Type = ValueType.Integer;
    }

    public ValueExpression(long value)
    {
        Value = value;
        Type = ValueType.Integer;
    }

    public ValueExpression(double value)
    {
        Value = value;
        Type = ValueType.Float;
    }

    public ValueExpression(decimal value)
    {
        Value = value;
        Type = ValueType.Float;
    }

    public ValueExpression(DateTime value)
    {
        Value = value;
        Type = ValueType.DateTime;
    }

    public ValueExpression(TimeSpan value)
    {
        Value = value;
        Type = ValueType.TimeSpan;
    }

    public ValueExpression(bool value)
    {
        Value = value;
        Type = ValueType.Boolean;
    }

    public ValueExpression(Guid value)
    {
        Value = value;
        Type = ValueType.Guid;
    }

    void IJsonOnDeserialized.OnDeserialized()
    {
        if (Value is not JsonElement element)
            return;

        Value = ConvertElementValue(element, Type);
    }

    private static object? ConvertElementValue(JsonElement element, ValueType type)
    {
        if (element.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
            return null;

        return type switch
        {
            ValueType.Boolean => element.GetBoolean(),
            ValueType.Integer => element.GetInt64(),
            ValueType.Float => element.GetDouble(),
            ValueType.String => element.GetString(),
            ValueType.Char => ReadChar(element),
            ValueType.Guid => element.GetGuid(),
            ValueType.DateTime => element.GetDateTime(),
            ValueType.TimeSpan => TimeSpan.Parse(element.GetString()!, CultureInfo.InvariantCulture),
            _ => throw new NCalcException($"This value type could not be handled: {type}")
        };
    }

    private static char ReadChar(JsonElement element)
    {
        var value = element.GetString();
        if (value is null || value.Length != 1)
            throw new NCalcException("Serialized char values must be a one-character string.");

        return value[0];
    }

    public override T Accept<T>(ILogicalExpressionVisitor<T> visitor, CancellationToken ct = default)
    {
        return visitor.Visit(this, ct);
    }
}
