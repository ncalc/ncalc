using System;
using System.Globalization;

namespace NCalc.Playground.Helpers;

public static class ValueFormatter
{
    public static string Format(object? value)
    {
        return value switch
        {
            null => "null",
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString() ?? ""
        };
    }
}
