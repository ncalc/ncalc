using NCalc.Domain;
using NCalc.Factories;
using NCalc.Parser;
using Parlot;
using Parlot.Fluent;

namespace NCalc.Tests;

[Trait("Category", "Parser")]
public class GeneratedParserTests
{
    [Fact]
    public void FactoryShouldReturnGeneratedParserAcrossAssemblies()
    {
        var parser = LogicalExpressionParser.CreateExpressionParser();

        Assert.Equal(typeof(LogicalExpressionParser), parser.GetType().DeclaringType);
        Assert.StartsWith("GeneratedParser", parser.GetType().Name);
        Assert.Equal(42, Assert.IsType<ValueExpression>(parser.Parse("42", cancellationToken: TestContext.Current.CancellationToken)).Value);
    }

    [Theory]
    [InlineData("42", ExpressionOptions.LongAsDefault, typeof(int), typeof(long))]
    [InlineData("1.5", ExpressionOptions.DecimalAsDefault, typeof(double), typeof(decimal))]
    [InlineData("'x'", ExpressionOptions.AllowCharValues, typeof(string), typeof(char))]
    public void ReusedParsersShouldRetainIndependentExpressionOptions(
        string text, ExpressionOptions options, Type defaultType, Type configuredType)
    {
        var defaultParser = LogicalExpressionParser.CreateExpressionParser();
        var configuredParser = LogicalExpressionParser.CreateExpressionParser(options);

        for (var i = 0; i < 2; i++)
        {
            Assert.IsType(defaultType, Assert.IsType<ValueExpression>(defaultParser.Parse(text, cancellationToken: TestContext.Current.CancellationToken)).Value);
            Assert.IsType(configuredType, Assert.IsType<ValueExpression>(configuredParser.Parse(text, cancellationToken: TestContext.Current.CancellationToken)).Value);
        }
    }

    [Theory]
    [InlineData(ArgumentSeparator.Comma, "('x', (42, 1.5))")]
    [InlineData(ArgumentSeparator.Semicolon, "('x'; (42; 1.5))")]
    [InlineData(ArgumentSeparator.Colon, "('x': (42: 1.5))")]
    public void RecursiveParsingShouldUseBoundOptions(ArgumentSeparator separator, string text)
    {
        var parser = LogicalExpressionParser.CreateExpressionParser(
            ExpressionOptions.AllowCharValues | ExpressionOptions.LongAsDefault | ExpressionOptions.DecimalAsDefault,
            LogicalExpressionParserOptions.FromArgumentSeparator(separator));

        var list = Assert.IsType<LogicalExpressionList>(parser.Parse(text, cancellationToken: TestContext.Current.CancellationToken));
        Assert.Equal('x', Assert.IsType<ValueExpression>(list[0]).Value);
        var nested = Assert.IsType<LogicalExpressionList>(list[1]);
        Assert.Equal(42L, Assert.IsType<ValueExpression>(nested[0]).Value);
        Assert.Equal(1.5m, Assert.IsType<ValueExpression>(nested[1]).Value);
    }

    [Fact]
    public void ReusedParsersShouldRetainIndependentCulturesAndSeparators()
    {
        var commaParser = LogicalExpressionParser.CreateExpressionParser(parserOptions:
            LogicalExpressionParserOptions.Create(CultureInfo.GetCultureInfo("en-US"), ArgumentSeparator.Comma));
        var semicolonParser = LogicalExpressionParser.CreateExpressionParser(parserOptions:
            LogicalExpressionParserOptions.Create(CultureInfo.GetCultureInfo("en-GB"), ArgumentSeparator.Semicolon));

        for (var i = 0; i < 2; i++)
        {
            var usDates = Assert.IsType<LogicalExpressionList>(commaParser.Parse("(#01/02/2025#, #03/04/2025#)", cancellationToken: TestContext.Current.CancellationToken));
            Assert.Equal(new DateTime(2025, 1, 2), Assert.IsType<ValueExpression>(usDates[0]).Value);
            Assert.Equal(new DateTime(2025, 3, 4), Assert.IsType<ValueExpression>(usDates[1]).Value);

            var gbDates = Assert.IsType<LogicalExpressionList>(semicolonParser.Parse("(#01/02/2025#; #03/04/2025#)", cancellationToken: TestContext.Current.CancellationToken));
            Assert.Equal(new DateTime(2025, 2, 1), Assert.IsType<ValueExpression>(gbDates[0]).Value);
            Assert.Equal(new DateTime(2025, 4, 3), Assert.IsType<ValueExpression>(gbDates[1]).Value);

            Assert.False(commaParser.TryParse("Max(1; 2)", out _, out _));
            Assert.False(semicolonParser.TryParse("Max(1, 2)", out _, out _));
        }
    }

    [Fact]
    public void DefaultOptionsShouldUseCultureAtFactoryCall()
    {
        var originalCulture = CultureInfo.CurrentCulture;

        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
            var usParser = LogicalExpressionParser.CreateExpressionParser();
            var usContext = new LogicalExpressionParserContext("#01/02/2025#", ct: TestContext.Current.CancellationToken);

            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-GB");
            var gbParser = LogicalExpressionParser.CreateExpressionParser();
            var gbContext = new LogicalExpressionParserContext("#01/02/2025#", ct: TestContext.Current.CancellationToken);

            Assert.Equal(new DateTime(2025, 1, 2), Assert.IsType<ValueExpression>(usParser.Parse("#01/02/2025#", cancellationToken: TestContext.Current.CancellationToken)).Value);
            Assert.Equal(new DateTime(2025, 2, 1), Assert.IsType<ValueExpression>(gbParser.Parse("#01/02/2025#", cancellationToken: TestContext.Current.CancellationToken)).Value);
            Assert.Equal(new DateTime(2025, 1, 2), Assert.IsType<ValueExpression>(LogicalExpressionParser.Parse(usContext)).Value);
            Assert.Equal(new DateTime(2025, 2, 1), Assert.IsType<ValueExpression>(LogicalExpressionParser.Parse(gbContext)).Value);
            Assert.Equal(new DateTime(2025, 2, 1),
                Assert.IsType<ValueExpression>(LogicalExpressionFactory.Create("#01/02/2025#", ct: TestContext.Current.CancellationToken)).Value);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public void GeneratedParserShouldHonorCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var context = new LogicalExpressionParserContext("1 + 2", ct: cancellation.Token);
        var parser = LogicalExpressionParser.CreateExpressionParser();
        Assert.Throws<OperationCanceledException>(() =>
        {
            var result = new ParseResult<LogicalExpression>();
            parser.Parse(context, ref result);
        });
    }
}
