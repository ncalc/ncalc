using NCalc.Exceptions;

namespace NCalc.Tests;

[Trait("Category", "DateTime")]
public class DateTimeTests
{
    [Fact]
    public void Should_Parse_Time()
    {
        var timeSeparator = CultureInfo.CurrentCulture.DateTimeFormat.TimeSeparator;
        var expr = new Expression($"#20{timeSeparator}42{timeSeparator}12#");
        Assert.Equal(new TimeSpan(20, 42, 12), expr.Evaluate(TestContext.Current.CancellationToken));
    }

    [Fact]
    public void Should_Parse_Date()
    {
        var dateSeparator = CultureInfo.CurrentCulture.DateTimeFormat.DateSeparator;
        var expr = new Expression($"#01{dateSeparator}01{dateSeparator}2001#");
        Assert.Equal(new DateTime(2001, 1, 1), expr.Evaluate(TestContext.Current.CancellationToken));
    }

    [Fact]
    public void Should_Parse_Date_Time()
    {
        var timeSeparator = CultureInfo.CurrentCulture.DateTimeFormat.TimeSeparator;
        var dateSeparator = CultureInfo.CurrentCulture.DateTimeFormat.DateSeparator;
        string exprStr = $"#2022{dateSeparator}12{dateSeparator}31 08{timeSeparator}00{timeSeparator}00#";
        Assert.Equal(new DateTime(2022, 12, 31, 8, 0, 0),
            new Expression(exprStr).Evaluate(TestContext.Current.CancellationToken));
    }

    [Fact]
    public void Should_Fail_With_Wrong_DateTime_Separator()
    {
        var trueTimeSeparator = CultureInfo.CurrentCulture.DateTimeFormat.TimeSeparator;
        var trueDateSeparator = CultureInfo.CurrentCulture.DateTimeFormat.DateSeparator;

        var timeSeparator = trueTimeSeparator == ":" ? "." : ":";
        var dateSeparator = trueDateSeparator == "." ? "/" : ".";

        string exprStr = $"#2022{dateSeparator}12{dateSeparator}31 08{timeSeparator}00{timeSeparator}00#";
        Assert.Throws<NCalcParserException>(() =>
            new Expression(exprStr).Evaluate(TestContext.Current.CancellationToken));
    }

    [Fact]
    public void ShouldHandleRuntimeCultureChange()
    {
        var oldCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");

        try
        {
            var expr = new Expression("#05/27/2025 12:00:00#", ExpressionOptions.None);
            var res = expr.Evaluate(TestContext.Current.CancellationToken);

            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");
            var expr2 = new Expression("#27.05.2025 12:00:00#");
            var res2 = expr2.Evaluate(TestContext.Current.CancellationToken);

            var dt = new DateTime(2025, 05, 27, 12, 0, 0);

            Assert.Equal(dt, res);
            Assert.Equal(dt, res2);
        }
        finally
        {
            CultureInfo.CurrentCulture = oldCulture;
        }
    }

    [Fact]
    public void ShouldHandleSpecifiedExpressionCulture()
    {
        var oldCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");

        try
        {
            var expr = new Expression("#05/27/2025 12:00:00#", ExpressionOptions.None);
            var res = expr.Evaluate(TestContext.Current.CancellationToken);

            var ruCulture = CultureInfo.GetCultureInfo("ru-RU");
            var expr2 = new Expression("#27.05.2025 12:00:00#", ExpressionOptions.None, ruCulture);
            var res2 = expr2.Evaluate(TestContext.Current.CancellationToken);

            var dt = new DateTime(2025, 05, 27, 12, 0, 0);

            Assert.Equal(dt, res);
            Assert.Equal(dt, res2);
        }
        finally
        {
            CultureInfo.CurrentCulture = oldCulture;
        }
    }

    [Fact]
    public void ShouldHandleTimespanFractionalPart()
    {
        var expected = new TimeSpan(0, 12, 0, 0, 333);

        var expr = new Expression("#12:00:00.333#", ExpressionOptions.None, CultureInfo.InvariantCulture);
        var res = expr.Evaluate(TestContext.Current.CancellationToken);

        Assert.Equal(expected, res);

        var deCulture = CultureInfo.GetCultureInfo("de-DE");
        var expr2 = new Expression("#12:00:00,333#", ExpressionOptions.None, deCulture);
        var res2 = expr2.Evaluate(TestContext.Current.CancellationToken);
        Assert.Equal(expected, res2);
    }

    [Fact]
    public void ShouldHandleDateTimeFractionalPart()
    {
        var expected = new DateTime(2025, 05, 27, 12, 0, 0, 333);

        var expr = new Expression("#05/27/2025 12:00:00.333#", ExpressionOptions.None, CultureInfo.InvariantCulture);
        var res = expr.Evaluate(TestContext.Current.CancellationToken);
        Assert.Equal(expected, res);

        var deCulture = CultureInfo.GetCultureInfo("de-DE");
        var expr2 = new Expression("#27.05.2025 12:00:00,333#", ExpressionOptions.None, deCulture);
        var res2 = expr2.Evaluate(TestContext.Current.CancellationToken);
        Assert.Equal(expected, res2);
    }
}