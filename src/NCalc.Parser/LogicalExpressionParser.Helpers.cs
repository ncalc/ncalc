using System.Text.RegularExpressions;

#if PARLOT_FLUENT
namespace NCalc.Benchmarks;

internal static partial class FluentExpressionParser
#else
namespace NCalc;

public static partial class LogicalExpressionParser
#endif
{
    private static readonly LogicalExpression True = new ValueExpression(true);
    private static readonly LogicalExpression False = new ValueExpression(false);

    private const double MinDecDouble = (double)decimal.MinValue;
    private const double MaxDecDouble = (double)decimal.MaxValue;

    private static readonly Regex UnicodeEscapeRegex = new(@"\\u([0-9a-fA-F]{4})", RegexOptions.Compiled);

    private static string DecodeUnicodeIfNeeded(ReadOnlySpan<char> span)
    {
        var text = span.ToString();
        if (!text.Contains("\\u", StringComparison.Ordinal))
            return text;

        return UnicodeEscapeRegex.Replace(text, static match =>
            ((char)Convert.ToInt32(match.Groups[1].Value, 16)).ToString());
    }

    private static LogicalExpression ParseSingleQuotedString(ReadOnlySpan<char> span, bool allowCharValues)
    {
        var text = DecodeUnicodeIfNeeded(span);

        return allowCharValues && text.Length == 1
            ? new ValueExpression(text[0])
            : new ValueExpression(text);
    }

    private static LogicalExpression ParseDoubleQuotedString(ReadOnlySpan<char> span)
    {
        return new ValueExpression(DecodeUnicodeIfNeeded(span));
    }

    private static LogicalExpression CreateValueExpression(int value) => new ValueExpression(value);
    private static LogicalExpression CreateValueExpression(long value) => new ValueExpression(value);
    private static LogicalExpression CreateValueExpression(decimal value) => new ValueExpression(value);
    private static LogicalExpression CreateValueExpression(double value) => new ValueExpression(value);
    private static LogicalExpression CreateValueExpression(string value) => new ValueExpression(value);

    private static LogicalExpression ParseBasedInteger(long value)
    {
        return value is > int.MaxValue or < int.MinValue
            ? new ValueExpression(value)
            : new ValueExpression((int)value);
    }

    private static LogicalExpression ParseDecimalFallback(double value)
    {
        return value switch
        {
            > MaxDecDouble => new ValueExpression(double.PositiveInfinity),
            < MinDecDouble => new ValueExpression(double.NegativeInfinity),
            _ => new ValueExpression((decimal)value)
        };
    }

    private static LogicalExpression ParseGuid(ReadOnlySpan<char> value)
    {
        return new ValueExpression(Guid.Parse(value));
    }

    private static LogicalExpression ParseDate(ReadOnlySpan<char> first, ReadOnlySpan<char> second, ReadOnlySpan<char> third, CultureInfo culture)
    {
        var separator = culture.DateTimeFormat.DateSeparator;

#if NET6_0_OR_GREATER
        var value = $"{first}{separator}{second}{separator}{third}";
#else
        var value = $"{first.ToString()}{separator}{second.ToString()}{separator}{third.ToString()}";
#endif

        if (DateTime.TryParse(value, culture, DateTimeStyles.None, out var result))
            return new ValueExpression(result);

        throw new FormatException("Invalid DateTime format.");
    }

    private static LogicalExpression ParseTime(ReadOnlySpan<char> hour, ReadOnlySpan<char> minute, ReadOnlySpan<char> second, ReadOnlySpan<char> fraction, CultureInfo culture)
    {
        var timeSeparator = culture.DateTimeFormat.TimeSeparator;
        var decSeparator = fraction.IsEmpty ? string.Empty : culture.NumberFormat.NumberDecimalSeparator;

#if NET6_0_OR_GREATER
        var value = $"{hour}{timeSeparator}{minute}{timeSeparator}{second}{decSeparator}{fraction}";
#else
        string fractionVal = fraction.IsEmpty ? string.Empty : fraction.ToString();
        var value = $"{hour.ToString()}{timeSeparator}{minute.ToString()}{timeSeparator}{second.ToString()}{decSeparator}{fractionVal}";
#endif

        if (TimeSpan.TryParse(value, culture, out var result))
            return new ValueExpression(result);

        throw new FormatException("Invalid TimeSpan format.");
    }

    private static LogicalExpression ParseDateAndTime(
        ReadOnlySpan<char> first, ReadOnlySpan<char> second, ReadOnlySpan<char> third, ReadOnlySpan<char> hour, ReadOnlySpan<char> minute, ReadOnlySpan<char> seconds, ReadOnlySpan<char> fraction,
        CultureInfo culture)
    {
        var dateSeparator = culture.DateTimeFormat.DateSeparator;
        var timeSeparator = culture.DateTimeFormat.TimeSeparator;
        var decSeparator = fraction.IsEmpty ? string.Empty : culture.NumberFormat.NumberDecimalSeparator;

#if NET6_0_OR_GREATER
        var value = $"{first}{dateSeparator}{second}{dateSeparator}{third} {hour}{timeSeparator}{minute}{timeSeparator}{seconds}{decSeparator}{fraction}";
#else
        string fractionVal = fraction.IsEmpty ? string.Empty : fraction.ToString();
        var value =  $"{first.ToString()}{dateSeparator}{second.ToString()}{dateSeparator}{third.ToString()} {hour.ToString()}{timeSeparator}{minute.ToString()}{timeSeparator}{seconds.ToString()}{decSeparator}{fractionVal}";
#endif

        if (DateTime.TryParse(value, culture, DateTimeStyles.None, out var result))
            return new ValueExpression(result);

        throw new FormatException("Invalid DateTime format.");
    }

    private static LogicalExpression ThrowUnknownOperatorSequence(LogicalExpression _, LogicalExpression __)
    {
        throw new InvalidOperationException("Unknown operator sequence.");
    }
}
