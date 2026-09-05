namespace NCalc.Tests;

[Property("Category", "Strings")]
public class StringTests
{
    [Test]
    [Arguments("経済協力開発機構", "'経済協力開発機構'")]
    [Arguments("Hello", @"'\u0048\u0065\u006C\u006C\u006F'")]
    [Arguments("だ", @"'\u3060'")]
    [Arguments("\u0100", @"'\u0100'")]
    public async Task ShouldHandleUnicode(string expected, string expression)
    {
        await Assert.That(expression).Expression().IsEqualTo(expected);
    }

    [Test]
    [Arguments("'hello'", @"'\'hello\''")]
    [Arguments(" ' hel lo ' ", @"' \' hel lo \' '")]
    [Arguments("hel\nlo", @"'hel\nlo'")]
    public async Task ShouldEscapeCharacters(string expected, string expression)
    {
        await Assert.That(expression).Expression().IsEqualTo(expected);
    }

    [Test]
    [Arguments("'to' + 'to'", "toto")]
    [Arguments("'one' + 2", "one2")]
    [Arguments("2 + 'one'", "2one")]
    [Arguments("'1' + '2'", "12")]
    public async Task ShouldHandleStringConcatenation(string expression, object expected)
    {
        var e = new Expression(expression, ExpressionOptions.StringConcat);
        await Assert.That(e).Expression().IsEqualTo(expected);
    }

    [Test]
    [Arguments("1 + '2'")]
    [Arguments("'1' + 2")]
    [Arguments("'1' + '2'")]
    public async Task ShouldHandleStringAddition(string expr)
    {
        var e = new Expression(expr, ExpressionOptions.DecimalAsDefault);
        await Assert.That(e).Expression().IsEqualTo(3m);
    }
}
