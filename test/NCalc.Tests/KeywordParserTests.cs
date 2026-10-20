using NCalc.Exceptions;

namespace NCalc.Tests;

[Property("Category", "Parser")]
public class KeywordParserTests
{
    [Test]
    [Arguments("TrUe aNd FaLsE", BinaryExpressionType.And, false)]
    [Arguments("FaLsE oR TrUe", BinaryExpressionType.Or, true)]
    [Arguments("1 iN (1, 2)", BinaryExpressionType.In, true)]
    [Arguments("1 NoT iN (2, 3)", BinaryExpressionType.NotIn, true)]
    [Arguments("'abc' LiKe 'a%'", BinaryExpressionType.Like, true)]
    [Arguments("'abc' NoT lIkE 'z%'", BinaryExpressionType.NotLike, true)]
    [Arguments("true and(false)", BinaryExpressionType.And, false)]
    [Arguments("false or(true)", BinaryExpressionType.Or, true)]
    [Arguments("1in(1, 2)", BinaryExpressionType.In, true)]
    [Arguments("1not in(2, 3)", BinaryExpressionType.NotIn, true)]
    [Arguments("'abc'like'a%'", BinaryExpressionType.Like, true)]
    [Arguments("'abc'not like'z%'", BinaryExpressionType.NotLike, true)]
    [Arguments("true&&false", BinaryExpressionType.And, false)]
    [Arguments("false||true", BinaryExpressionType.Or, true)]
    public async Task ShouldParseWordOperatorsWithCaseInsensitiveKeywordBoundaries(
        string text, BinaryExpressionType expectedType, bool expected)
    {
        foreach (var expression in ParseWithBothOverloads(text))
        {
            await Assert.That(expression).IsTypeOf<BinaryExpression>();
            await Assert.That(((BinaryExpression)expression).Type).IsEqualTo(expectedType);
            await Assert.That(new Expression(expression).Evaluate(CancellationToken.None)).IsEqualTo(expected);
        }
    }

    [Test]
    [Arguments("true and anderson", BinaryExpressionType.And, "anderson")]
    [Arguments("false or orElse", BinaryExpressionType.Or, "orElse")]
    [Arguments("1 in inside", BinaryExpressionType.In, "inside")]
    [Arguments("1 not in inside", BinaryExpressionType.NotIn, "inside")]
    [Arguments("'abc' like likeValue", BinaryExpressionType.Like, "likeValue")]
    [Arguments("'abc' not like likeValue", BinaryExpressionType.NotLike, "likeValue")]
    [Arguments("true and trueValue", BinaryExpressionType.And, "trueValue")]
    [Arguments("false or falsePositive", BinaryExpressionType.Or, "falsePositive")]
    public async Task ShouldParseKeywordPrefixedIdentifiersAsOperands(
        string text, BinaryExpressionType expectedType, string expectedName)
    {
        foreach (var expression in ParseWithBothOverloads(text))
        {
            await Assert.That(expression).IsTypeOf<BinaryExpression>();
            var binary = (BinaryExpression)expression;
            await Assert.That(binary.Type).IsEqualTo(expectedType);
            await Assert.That(binary.RightExpression).IsTypeOf<Identifier>();
            await Assert.That(((Identifier)binary.RightExpression).Name).IsEqualTo(expectedName);
        }
    }

    [Test]
    [Arguments("trueValue")]
    [Arguments("TrUeValue")]
    [Arguments("falsePositive")]
    [Arguments("FaLsEPositive")]
    [Arguments("trueOrFalse")]
    [Arguments("anderson")]
    [Arguments("orElse")]
    [Arguments("inside")]
    [Arguments("likeValue")]
    [Arguments("notable")]
    [Arguments("not_value")]
    [Arguments("not1")]
    public async Task ShouldPreserveKeywordPrefixedIdentifiers(string name)
    {
        foreach (var expression in ParseWithBothOverloads(name))
        {
            await Assert.That(expression).IsTypeOf<Identifier>();
            await Assert.That(((Identifier)expression).Name).IsEqualTo(name);
        }
    }

    [Test]
    [Arguments("trueValue")]
    [Arguments("falsePositive")]
    [Arguments("anderson")]
    [Arguments("orElse")]
    [Arguments("inside")]
    [Arguments("likeValue")]
    [Arguments("notable")]
    public async Task ShouldPreserveKeywordPrefixedFunctionNames(string name)
    {
        foreach (var expression in ParseWithBothOverloads($"{name}(42)"))
        {
            await Assert.That(expression).IsTypeOf<Function>();
            var function = (Function)expression;
            await Assert.That(function.Identifier.Name).IsEqualTo(name);
            await Assert.That(function.Parameters.Count).IsEqualTo(1);
            await Assert.That(function.Parameters[0]).IsTypeOf<ValueExpression>();
            await Assert.That(((ValueExpression)function.Parameters[0]).Value).IsEqualTo(42);
        }
    }

    [Test]
    [Arguments("true anderson")]
    [Arguments("true orElse")]
    [Arguments("1 inside")]
    [Arguments("1 not inside")]
    [Arguments("'abc' likeness")]
    [Arguments("'abc' not likeness")]
    [Arguments("TRUE aNDerson")]
    [Arguments("TRUE oRelse")]
    [Arguments("1 InSide")]
    [Arguments("1 NoT InSide")]
    [Arguments("'abc' LiKeness")]
    [Arguments("'abc' NoT LiKeness")]
    public void ShouldRejectWordOperatorsWithinLongerWords(string text)
    {
        Assert.Throws<NCalcParserException>(() => LogicalExpressionParser.Parse(text, culture: CultureInfo.InvariantCulture));
        Assert.Throws<NCalcParserException>(() => LogicalExpressionParser.Parse(
            new LogicalExpressionParseContext(text), CultureInfo.InvariantCulture));
    }

    [Test]
    [Arguments("NoT true")]
    [Arguments("not\ttrue")]
    [Arguments("nOt(true)")]
    [Arguments("not (true)")]
    [Arguments("!true")]
    public async Task ShouldPreserveUnaryNotOperandDelimiters(string text)
    {
        foreach (var expression in ParseWithBothOverloads(text))
        {
            await Assert.That(expression).IsTypeOf<UnaryExpression>();
            await Assert.That(((UnaryExpression)expression).Type).IsEqualTo(UnaryExpressionType.Not);
            await Assert.That(new Expression(expression).Evaluate<bool>(CancellationToken.None)).IsFalse();
        }
    }

    [Test]
    [Arguments("not[flag]")]
    [Arguments("not!true")]
    [Arguments("not'text'")]
    [Arguments("1 not  in (2, 3)")]
    [Arguments("'abc' not\tlike 'a%'")]
    public void ShouldRejectUnsupportedKeywordOperandDelimiters(string text)
    {
        Assert.Throws<NCalcParserException>(() => LogicalExpressionParser.Parse(text, culture: CultureInfo.InvariantCulture));
        Assert.Throws<NCalcParserException>(() => LogicalExpressionParser.Parse(
            new LogicalExpressionParseContext(text), CultureInfo.InvariantCulture));
    }

    private static LogicalExpression[] ParseWithBothOverloads(string text) =>
    [
        LogicalExpressionParser.Parse(text, culture: CultureInfo.InvariantCulture),
        LogicalExpressionParser.Parse(new LogicalExpressionParseContext(text), CultureInfo.InvariantCulture)
    ];
}
