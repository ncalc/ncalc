namespace NCalc.Tests;

[Trait("Category","Strings")]
public class StringTests
{
    [Theory]
    [InlineData("経済協力開発機構", "'経済協力開発機構'")]
    [InlineData("Hello", @"'\u0048\u0065\u006C\u006C\u006F'")]
    [InlineData("だ", @"'\u3060'")]
    [InlineData("\u0100", @"'\u0100'")]
    public void ShouldHandleUnicode(string expected, string expression)
    {
        Assert.Equal(expected, new Expression(expression).Evaluate());
    }

    [Theory]
    [InlineData("'hello'", @"'\'hello\''")]
    [InlineData(" ' hel lo ' ", @"' \' hel lo \' '")]
    [InlineData("hel\nlo", @"'hel\nlo'")]
    public void ShouldEscapeCharacters(string expected, string expression)
    {
        Assert.Equal(expected, new Expression(expression).Evaluate());
    }
    
    [Theory]
    [InlineData("'to' + 'to'", "toto")]
    [InlineData("'one' + 2", "one2")]
    public void ShouldHandleStringConcatenation(string expression, object expected)
    {
        Assert.Equal(expected, new Expression(expression).Evaluate());
    }

    [Fact]
    public void ShouldHandleStringAddition()
    {
        Assert.Equal(3m, new Expression("1 + '2'", ExpressionOptions.DecimalAsDefault).Evaluate());
    }
}