namespace NCalc.Tests;

[Property("Category", "Strings")]
public class StringTests
{
    [Test]
    [Arguments("経済協力開発機構", "'経済協力開発機構'")]
    [Arguments("Hello", @"'\u0048\u0065\u006C\u006C\u006F'")]
    [Arguments("だ", @"'\u3060'")]
    [Arguments("\u0100", @"'\u0100'")]
    public void ShouldHandleUnicode(string expected, string expression)
    {
        Assert.Expression(expected, expression);
    }

    [Test]
    [Arguments("'hello'", @"'\'hello\''")]
    [Arguments(" ' hel lo ' ", @"' \' hel lo \' '")]
    [Arguments("hel\nlo", @"'hel\nlo'")]
    public void ShouldEscapeCharacters(string expected, string expression)
    {
        Assert.Expression(expected, expression);
    }

    [Test]
    [Arguments("'to' + 'to'", "toto")]
    [Arguments("'one' + 2", "one2")]
    [Arguments("2 + 'one'", "2one")]
    [Arguments("'1' + '2'", "12")]
    public void ShouldHandleStringConcatenation(string expression, object expected)
    {
        var e = new Expression(expression, ExpressionOptions.StringConcat);
        Assert.Expression(expected, e);
    }

    [Test]
    [Arguments("1 + '2'")]
    [Arguments("'1' + 2")]
    [Arguments("'1' + '2'")]
    public void ShouldHandleStringAddition(string expr)
    {
        var e = new Expression(expr, ExpressionOptions.DecimalAsDefault);
        Assert.Expression(3m, e);
    }
}