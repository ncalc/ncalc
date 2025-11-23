namespace NCalc.Tests;

[Trait("Category", "Strings")]
public class StringTests
{
    [Theory]
    [InlineData("経済協力開発機構", "'経済協力開発機構'")]
    [InlineData("Hello", @"'\u0048\u0065\u006C\u006C\u006F'")]
    [InlineData("だ", @"'\u3060'")]
    [InlineData("\u0100", @"'\u0100'")]
    public void ShouldHandleUnicode(string expected, string expression)
    {
        Assert.Equal(expected, new Expression(expression).Evaluate(TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData("'hello'", @"'\'hello\''")]
    [InlineData(" ' hel lo ' ", @"' \' hel lo \' '")]
    [InlineData("hel\nlo", @"'hel\nlo'")]
    public void ShouldEscapeCharacters(string expected, string expression)
    {
        Assert.Equal(expected, new Expression(expression).Evaluate(TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData("'to' + 'to'", "toto")]
    [InlineData("'one' + 2", "one2")]
    [InlineData("2 + 'one'", "2one")]
    [InlineData("'1' + '2'", "12")]
    public void ShouldHandleStringConcatenation(string expression, object expected)
    {
        var e = new Expression(expression, ExpressionOptions.StringConcat);
        Assert.Equal(expected, e.Evaluate(TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData("1 + '2'")]
    [InlineData("'1' + 2")]
    [InlineData("'1' + '2'")]
    public void ShouldHandleStringAddition(string expr)
    {
        var e = new Expression(expr, ExpressionOptions.DecimalAsDefault);
        Assert.Equal(3m, e.Evaluate(TestContext.Current.CancellationToken));
    }
}