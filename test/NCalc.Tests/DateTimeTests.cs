using NCalc.Exceptions;

namespace NCalc.Tests;

[Property("Category", "DateTime")]
public class DateTimeTests
{
    [Test]
    public async Task Should_Parse_Time(CancellationToken cancellationToken)
    {
        var timeSeparator = CultureInfo.CurrentCulture.DateTimeFormat.TimeSeparator;
        var expr = new Expression($"#20{timeSeparator}42{timeSeparator}12#");
        await Assert.That(expr.Evaluate(cancellationToken)).IsEqualTo(new TimeSpan(20, 42, 12));
    }

    [Test]
    public async Task Should_Parse_Date(CancellationToken cancellationToken)
    {
        var dateSeparator = CultureInfo.CurrentCulture.DateTimeFormat.DateSeparator;
        var expr = new Expression($"#01{dateSeparator}01{dateSeparator}2001#");
        await Assert.That(expr.Evaluate(cancellationToken)).IsEqualTo(new DateTime(2001, 1, 1));
    }

    [Test]
    public async Task Should_Parse_Date_Time(CancellationToken cancellationToken)
    {
        var timeSeparator = CultureInfo.CurrentCulture.DateTimeFormat.TimeSeparator;
        var dateSeparator = CultureInfo.CurrentCulture.DateTimeFormat.DateSeparator;
        string exprStr = $"#2022{dateSeparator}12{dateSeparator}31 08{timeSeparator}00{timeSeparator}00#";
        await Assert.That(new Expression(exprStr).Evaluate(cancellationToken)).IsEqualTo(new DateTime(2022, 12, 31, 8, 0, 0));
    }

    [Test]
    public async Task Should_Fail_With_Wrong_DateTime_Separator(CancellationToken cancellationToken)
    {
        var trueTimeSeparator = CultureInfo.CurrentCulture.DateTimeFormat.TimeSeparator;
        var trueDateSeparator = CultureInfo.CurrentCulture.DateTimeFormat.DateSeparator;

        var timeSeparator = trueTimeSeparator == ":" ? "." : ":";
        var dateSeparator = trueDateSeparator == "." ? "/" : ".";

        string exprStr = $"#2022{dateSeparator}12{dateSeparator}31 08{timeSeparator}00{timeSeparator}00#";
        await Assert.That(() =>
            new Expression(exprStr).Evaluate(cancellationToken)).ThrowsExactly<NCalcParserException>();
    }

    [Test]
    public async Task ShouldHandleRuntimeCultureChange(CancellationToken cancellationToken)
    {
        var oldCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");

        try
        {
            var expr = new Expression("#05/27/2025 12:00:00#", ExpressionOptions.None);
            var res = expr.Evaluate(cancellationToken);

            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");
            var expr2 = new Expression("#27.05.2025 12:00:00#");
            var res2 = expr.Evaluate(cancellationToken);

            var dt = new DateTime(2025, 05, 27, 12, 0, 0);

            await Assert.That(res).IsEqualTo(dt);
            await Assert.That(res2).IsEqualTo(dt);
        }
        finally
        {
            CultureInfo.CurrentCulture = oldCulture;
        }
    }

    [Test]
    public async Task ShouldHandleSpecifiedExpressionCulture(CancellationToken cancellationToken)
    {
        var oldCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");

        try
        {
            var expr = new Expression("#05/27/2025 12:00:00#", ExpressionOptions.None);
            var res = expr.Evaluate(cancellationToken);

            var ruCulture = CultureInfo.GetCultureInfo("ru-RU");
            var expr2 = new Expression("#27.05.2025 12:00:00#", ExpressionOptions.None, ruCulture);
            var res2 = expr.Evaluate(cancellationToken);

            var dt = new DateTime(2025, 05, 27, 12, 0, 0);

            await Assert.That(res).IsEqualTo(dt);
            await Assert.That(res2).IsEqualTo(dt);
        }
        finally
        {
            CultureInfo.CurrentCulture = oldCulture;
        }
    }

    [Test]
    public async Task ShouldHandleTimespanFractionalPart(CancellationToken cancellationToken)
    {
        var expected = new TimeSpan(0, 12, 0, 0, 333);

        var expr = new Expression("#12:00:00.333#", ExpressionOptions.None, CultureInfo.InvariantCulture);
        var res = expr.Evaluate(cancellationToken);

        await Assert.That(res).IsEqualTo(expected);

        var deCulture = CultureInfo.GetCultureInfo("de-DE");
        var expr2 = new Expression("#12:00:00,333#", ExpressionOptions.None, deCulture);
        var res2 = expr2.Evaluate(cancellationToken);
        await Assert.That(res2).IsEqualTo(expected);
    }

    [Test]
    public async Task ShouldHandleDateTimeFractionalPart(CancellationToken cancellationToken)
    {
        var expected = new DateTime(2025, 05, 27, 12, 0, 0, 333);

        var expr = new Expression("#05/27/2025 12:00:00.333#", ExpressionOptions.None, CultureInfo.InvariantCulture);
        var res = expr.Evaluate(cancellationToken);
        await Assert.That(res).IsEqualTo(expected);

        var deCulture = CultureInfo.GetCultureInfo("de-DE");
        var expr2 = new Expression("#27.05.2025 12:00:00,333#", ExpressionOptions.None, deCulture);
        var res2 = expr2.Evaluate(cancellationToken);
        await Assert.That(res2).IsEqualTo(expected);
    }
}