using System.Text;

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

    // The string parsers return the text between the quotes as it is, so that the escape sequences,
    // \uXXXX included, are decoded in a single pass and an escaped backslash is never read again.
    private static string DecodeString(ReadOnlySpan<char> span)
    {
        var index = span.IndexOf('\\');
        if (index == -1)
            return span.ToString();

        var builder = new StringBuilder(span.Length);

        for (var i = 0; i < index; i++)
            builder.Append(span[i]);

        // The parser only accepts a backslash that is followed by another char
        for (var i = index; i < span.Length; i++)
        {
            var c = span[i];

            if (c != '\\')
            {
                builder.Append(c);
                continue;
            }

            var next = span[++i];

            switch (next)
            {
                case '\\':
                case '\'':
                case '"':
                    builder.Append(next);
                    break;
                case 'n':
                    builder.Append('\n');
                    break;
                case 'r':
                    builder.Append('\r');
                    break;
                case 't':
                    builder.Append('\t');
                    break;
                case 'b':
                    builder.Append('\b');
                    break;
                case 'f':
                    builder.Append('\f');
                    break;
                case 'u' when TryReadHex4(span, i + 1, out var code):
                    builder.Append(code);
                    i += 4;
                    break;
                default:
                    builder.Append('\\').Append(next);
                    break;
            }
        }

        return builder.ToString();
    }

    private static bool TryReadHex4(ReadOnlySpan<char> span, int start, out char value)
    {
        value = default;

        if (start + 4 > span.Length)
            return false;

        var code = 0;
        for (var i = start; i < start + 4; i++)
        {
            var c = span[i];
            int digit;

            if (c is >= '0' and <= '9')
                digit = c - '0';
            else if (c is >= 'a' and <= 'f')
                digit = c - 'a' + 10;
            else if (c is >= 'A' and <= 'F')
                digit = c - 'A' + 10;
            else
                return false;

            code = code * 16 + digit;
        }

        value = (char)code;
        return true;
    }

    private static LogicalExpression ParseSingleQuotedString(ReadOnlySpan<char> span, bool allowCharValues)
    {
        var text = DecodeString(span);

        return allowCharValues && text.Length == 1
            ? new ValueExpression(text[0])
            : new ValueExpression(text);
    }

    private static LogicalExpression ParseDoubleQuotedString(ReadOnlySpan<char> span)
    {
        return new ValueExpression(DecodeString(span));
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
