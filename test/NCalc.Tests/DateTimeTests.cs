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
        Assert.Equal(new TimeSpan(20, 42, 12), expr.Evaluate());
    }

    [Fact]
    public void Should_Parse_Date()
    {
        var dateSeparator = CultureInfo.CurrentCulture.DateTimeFormat.DateSeparator;
        string exprStr = "";
        switch (CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern[0])
        {
            case 'M': exprStr = $"#01{dateSeparator}01{dateSeparator}2001#"; break;
            case 'd': exprStr = $"#01{dateSeparator}01{dateSeparator}2001#"; break;
            case 'y': exprStr = $"#2001{dateSeparator}01{dateSeparator}01#"; break;
        }
        Assert.False(string.IsNullOrEmpty(exprStr));

        var expr = new Expression(exprStr);
        Assert.Equal(new DateTime(2001, 1, 1), expr.Evaluate());
    }

    [Fact]
    public void Should_Parse_Date_Time()
    {
        var timeSeparator = CultureInfo.CurrentCulture.DateTimeFormat.TimeSeparator;
        var dateSeparator = CultureInfo.CurrentCulture.DateTimeFormat.DateSeparator;
        string exprStr = "";
        switch (CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern[0])
        {
            case 'M': exprStr = $"#12{dateSeparator}31{dateSeparator}2022 08{timeSeparator}00{timeSeparator}00#"; break;
            case 'd': exprStr = $"#31{dateSeparator}12{dateSeparator}2022 08{timeSeparator}00{timeSeparator}00#"; break;
            case 'y': exprStr = $"#2022{dateSeparator}12{dateSeparator}31 08{timeSeparator}00{timeSeparator}00#"; break;
        }
        Assert.False(string.IsNullOrEmpty(exprStr));
        Assert.Equal(new DateTime(2022, 12, 31, 8, 0, 0), new Expression(exprStr).Evaluate());
    }

    [Fact]
    public void Should_Fail_With_Wrong_DateTime_Separator()
    {
        var trueTimeSeparator = CultureInfo.CurrentCulture.DateTimeFormat.TimeSeparator;
        var trueDateSeparator = CultureInfo.CurrentCulture.DateTimeFormat.DateSeparator;

        var timeSeparator = trueTimeSeparator == ":" ? "." : ":";
        var dateSeparator = trueDateSeparator == "." ? "/" : ".";

        string exprStr = "";
        switch (CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern[0])
        {
            case 'M': exprStr = $"#12{dateSeparator}31{dateSeparator}2022 08{timeSeparator}00{timeSeparator}00#"; break;
            case 'd': exprStr = $"#31{dateSeparator}12{dateSeparator}2022 08{timeSeparator}00{timeSeparator}00#"; break;
            case 'y': exprStr = $"#2022{dateSeparator}12{dateSeparator}31 08{timeSeparator}00{timeSeparator}00#"; break;
        }
        Assert.False(string.IsNullOrEmpty(exprStr));

        Assert.Throws<NCalcParserException>(() => new Expression(exprStr).Evaluate());
    }

    [Fact]
    public void ShouldHandleRuntimeCultureChange()
    {
        var oldCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");

        try
        {
            var expr = new Expression("#05/27/2025 12:00:00#", ExpressionOptions.None);
            var res = expr.Evaluate();

            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");
            var expr2 = new Expression("#27.05.2025 12:00:00#");
            var res2 = expr.Evaluate();

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
            var res = expr.Evaluate();

            var ruCulture = CultureInfo.GetCultureInfo("ru-RU");
            var expr2 = new Expression("#27.05.2025 12:00:00#", ExpressionOptions.None, ruCulture);
            var res2 = expr.Evaluate();

            var dt = new DateTime(2025, 05, 27, 12, 0, 0);

            Assert.Equal(dt, res);
            Assert.Equal(dt, res2);
        }
        finally
        {
            CultureInfo.CurrentCulture = oldCulture;
        }
    }
}