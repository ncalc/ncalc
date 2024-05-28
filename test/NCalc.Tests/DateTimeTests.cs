namespace NCalc.Tests;

[Trait("Category","DateTime")]
public class DateTimeTests
{
    [Fact]
    public void Should_Parse_Time()
    {
        Assert.Equal(new TimeSpan(20,42,12), new Expression("#20:42:12#").Evaluate());
    }
    
    [Fact]
    public void Should_Parse_Date()
    {
        Assert.Equal(new DateTime(2001,1,1), new Expression("#01/01/2001#").Evaluate());
    }

    [Fact]
    public void Should_Parse_Date_Time()
    {
        Assert.Equal(new DateTime(2022,12,31,8,0,0), new Expression("#2022/12/31 08:00:00#").Evaluate());
    }
}