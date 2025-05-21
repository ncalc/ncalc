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
        var expr = new Expression($"#01{dateSeparator}01{dateSeparator}2001#");
        Assert.Equal(new DateTime(2001, 1, 1), expr.Evaluate());
    }

    [Fact]
    public void Should_Parse_Date_Time()
    {
        var timeSeparator = CultureInfo.CurrentCulture.DateTimeFormat.TimeSeparator;
        var dateSeparator = CultureInfo.CurrentCulture.DateTimeFormat.DateSeparator;
        string exprStr = $"#2022{dateSeparator}12{dateSeparator}31 08{timeSeparator}00{timeSeparator}00#";
        Assert.Equal(new DateTime(2022, 12, 31, 8, 0, 0), new Expression(exprStr).Evaluate());
    }

    [Fact]
    public void Should_Fail_With_Wrong_DateTime_Separator()
    {
        var trueTimeSeparator = CultureInfo.CurrentCulture.DateTimeFormat.TimeSeparator;
        var trueDateSeparator = CultureInfo.CurrentCulture.DateTimeFormat.DateSeparator;

        var timeSeparator = trueTimeSeparator == ":" ? "." : ":";
        var dateSeparator = trueDateSeparator == "." ? "/" : ".";

        string exprStr = $"#2022{dateSeparator}12{dateSeparator}31 08{timeSeparator}00{timeSeparator}00#";
        Assert.Throws<NCalcParserException>(() => new Expression(exprStr).Evaluate());
    }
}