using System;
using System.Globalization;

namespace NCalc.Playground.Helpers;

public static class VariableValueParser
{
    public static object? Parse(string value)
    {
        value = value.Trim();

        if (value.Length == 0)
            return "";

        if (IsQuoted(value))
            return value[1..^1];

        if (value.Equals("null", StringComparison.OrdinalIgnoreCase))
            return null;

        if (bool.TryParse(value, out var booleanValue))
            return booleanValue;

        if (long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var integerValue))
            return integerValue;

        if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var decimalValue))
            return decimalValue;

        return value;
    }

    private static bool IsQuoted(string value)
    {
        return value.Length >= 2
               && ((value[0] == '\'' && value[^1] == '\'')
                   || (value[0] == '"' && value[^1] == '"'));
    }
}
