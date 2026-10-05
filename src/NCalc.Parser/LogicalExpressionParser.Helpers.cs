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

    private static LogicalExpression ParseSingleQuotedString(string value, bool allowCharValues)
    {
        return allowCharValues && value.Length == 1
            ? new ValueExpression(value[0])
            : new ValueExpression(value);
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
#if NET6_0_OR_GREATER
        return new ValueExpression(Guid.Parse(value));
#else
        return new ValueExpression(Guid.Parse(value.ToString()));
#endif
    }

    private static LogicalExpression ParseDate(string first, string second, string third, CultureInfo culture)
    {
        var separator = culture.DateTimeFormat.DateSeparator;
        if (DateTime.TryParse($"{first}{separator}{second}{separator}{third}",
                culture, DateTimeStyles.None, out var result))
        {
            return new ValueExpression(result);
        }

        throw new FormatException("Invalid DateTime format.");
    }

    private static LogicalExpression ParseTime(string hour, string minute, string second, string fraction, CultureInfo culture)
    {
        var separator = culture.DateTimeFormat.TimeSeparator;
        var value = $"{hour}{separator}{minute}{separator}{second}";
        if (fraction.Length == 0 && TimeSpan.TryParse(value, culture, out var result))
            return new ValueExpression(result);

        if (TimeSpan.TryParse($"{value}{culture.NumberFormat.NumberDecimalSeparator}{fraction}", culture, out result))
            return new ValueExpression(result);

        throw new FormatException("Invalid TimeSpan format.");
    }

    private static LogicalExpression ParseDateAndTime(
        string first, string second, string third, string hour, string minute, string seconds, string fraction,
        CultureInfo culture)
    {
        var dateSeparator = culture.DateTimeFormat.DateSeparator;
        var timeSeparator = culture.DateTimeFormat.TimeSeparator;
        var value = $"{first}{dateSeparator}{second}{dateSeparator}{third} {hour}{timeSeparator}{minute}{timeSeparator}{seconds}";
        if (fraction.Length == 0 && DateTime.TryParse(value, culture, DateTimeStyles.None, out var result))
            return new ValueExpression(result);

        if (DateTime.TryParse($"{value}{culture.NumberFormat.NumberDecimalSeparator}{fraction}", culture, DateTimeStyles.None, out result))
            return new ValueExpression(result);

        throw new FormatException("Invalid DateTime format.");
    }

    private static LogicalExpression ThrowUnknownOperatorSequence(LogicalExpression _, LogicalExpression __)
    {
        throw new InvalidOperationException("Unknown operator sequence.");
    }

    private static LogicalExpression ParseCoalescingExpression((LogicalExpression Left, IReadOnlyList<LogicalExpression> Right) expression)
    {
        if (expression.Right.Count == 0)
            return expression.Left;

        var result = expression.Right[expression.Right.Count - 1];
        for (var i = expression.Right.Count - 2; i >= 0; i--)
            result = new BinaryExpression(BinaryExpressionType.Coalesce, expression.Right[i], result);

        return new BinaryExpression(BinaryExpressionType.Coalesce, expression.Left, result);
    }

    private static LogicalExpression ParseBinaryExpression((LogicalExpression, IReadOnlyList<(BinaryExpressionType, LogicalExpression)>) x)
    {
        var result = x.Item1;

        foreach (var op in x.Item2)
        {
            result = new BinaryExpression(op.Item1, result, op.Item2);
        }

        return result;
    }
}
