using NCalc.Factories;
using Parlot;
using Parlot.Fluent;

namespace NCalc.Tests;

[Property("Category", "Parser")]
public class GeneratedParserTests
{
    [Test]
    public async Task ShouldReturnGeneratedParserAcrossAssemblies()
    {
        var options = new LogicalExpressionParserOptions();
        var parser = LogicalExpressionParser.GetOrCreateExpressionParser(options, CultureInfo.InvariantCulture);

        await Assert.That(parser.GetType().DeclaringType).IsEqualTo(typeof(LogicalExpressionParser));
        await Assert.That(parser.GetType().Name.StartsWith("GeneratedParser", StringComparison.Ordinal)).IsTrue();
        await Assert.That(parser.Name).IsEqualTo("Expression");
        await AssertValue(Parse(parser, "42", options), 42);
    }

    [Test]
    [Arguments(IntegerNumberType.Int32, FloatingPointNumberType.Double, false)]
    [Arguments(IntegerNumberType.Int64, FloatingPointNumberType.Double, false)]
    [Arguments(IntegerNumberType.Int32, FloatingPointNumberType.Decimal, false)]
    [Arguments(IntegerNumberType.Int32, FloatingPointNumberType.Double, true)]
    [Arguments(IntegerNumberType.Int64, FloatingPointNumberType.Decimal, false)]
    [Arguments(IntegerNumberType.Int64, FloatingPointNumberType.Double, true)]
    [Arguments(IntegerNumberType.Int32, FloatingPointNumberType.Decimal, true)]
    [Arguments(IntegerNumberType.Int64, FloatingPointNumberType.Decimal, true)]
    public async Task ShouldRetainIndependentParseTimeOptions(
        IntegerNumberType integerType, FloatingPointNumberType floatingPointType, bool allowCharValues)
    {
        var defaultOptions = new LogicalExpressionParserOptions();
        var configuredOptions = new LogicalExpressionParserOptions
        {
            IntegerNumberType = integerType,
            FloatingPointNumberType = floatingPointType,
            AllowCharValues = allowCharValues
        };
        var defaultParser = LogicalExpressionParser.GetOrCreateExpressionParser(defaultOptions, CultureInfo.InvariantCulture);
        var configuredParser = LogicalExpressionParser.GetOrCreateExpressionParser(configuredOptions, CultureInfo.InvariantCulture);

        foreach (var parser in new[] { defaultParser, configuredParser })
        {
            await Assert.That(parser.GetType().DeclaringType).IsEqualTo(typeof(LogicalExpressionParser));

            for (var i = 0; i < 2; i++)
            {
                await AssertValue(Parse(parser, "42", defaultOptions), 42);
                await AssertValue(Parse(parser, "1.5", defaultOptions), 1.5d);
                await AssertValue(Parse(parser, "'x'", defaultOptions), "x");

                await AssertValue(Parse(parser, "42", configuredOptions), integerType == IntegerNumberType.Int64 ? (object)42L : 42);
                await AssertValue(Parse(parser, "1.5", configuredOptions), floatingPointType == FloatingPointNumberType.Decimal ? (object)1.5m : 1.5d);
                await AssertValue(Parse(parser, "'x'", configuredOptions), allowCharValues ? (object)'x' : "x");
                await AssertValue(Parse(parser, "\"x\"", configuredOptions), "x");
            }
        }
    }

    [Test]
    [Arguments(ArgumentSeparator.Default, "('x', (42, 1.5))")]
    [Arguments(ArgumentSeparator.Comma, "('x', (42, 1.5))")]
    [Arguments(ArgumentSeparator.Semicolon, "('x'; (42; 1.5))")]
    [Arguments(ArgumentSeparator.Colon, "('x': (42: 1.5))")]
    [Arguments(ArgumentSeparator.Comma | ArgumentSeparator.Semicolon, "('x', (42; 1.5))")]
    [Arguments(ArgumentSeparator.Comma | ArgumentSeparator.Colon, "('x', (42: 1.5))")]
    [Arguments(ArgumentSeparator.Semicolon | ArgumentSeparator.Colon, "('x'; (42: 1.5))")]
    public async Task ShouldUseParseTimeOptionsRecursively(ArgumentSeparator separator, string text)
    {
        var parser = LogicalExpressionParser.GetOrCreateExpressionParser(
            new LogicalExpressionParserOptions { ArgumentSeparator = separator }, CultureInfo.InvariantCulture);
        var options = new LogicalExpressionParserOptions
        {
            ArgumentSeparator = separator,
            AllowCharValues = true,
            IntegerNumberType = IntegerNumberType.Int64,
            FloatingPointNumberType = FloatingPointNumberType.Decimal
        };

        var expression = Parse(parser, text, options);
        await Assert.That(parser.GetType().DeclaringType).IsEqualTo(typeof(LogicalExpressionParser));
        await Assert.That(expression).IsTypeOf<LogicalExpressionList>();
        var list = (LogicalExpressionList)expression;
        await Assert.That(list.Count).IsEqualTo(2);
        await AssertValue(list[0], 'x');
        await Assert.That(list[1]).IsTypeOf<LogicalExpressionList>();
        var nested = (LogicalExpressionList)list[1];
        await Assert.That(nested.Count).IsEqualTo(2);
        await AssertValue(nested[0], 42L);
        await AssertValue(nested[1], 1.5m);
    }

    [Test]
    public async Task ShouldUseAllSeparatorFlagsInNestedFunctions()
    {
        var options = new LogicalExpressionParserOptions
        {
            ArgumentSeparator = ArgumentSeparator.Comma | ArgumentSeparator.Semicolon | ArgumentSeparator.Colon,
            AllowCharValues = true,
            IntegerNumberType = IntegerNumberType.Int64,
            FloatingPointNumberType = FloatingPointNumberType.Decimal
        };
        var parser = LogicalExpressionParser.GetOrCreateExpressionParser(options, CultureInfo.InvariantCulture);
        var expression = Parse(parser, "Outer('x', Inner(42; 1.5): 2)", options);

        await Assert.That(parser.GetType().DeclaringType).IsEqualTo(typeof(LogicalExpressionParser));
        await Assert.That(expression).IsTypeOf<Function>();
        var function = (Function)expression;
        await Assert.That(function.Identifier.Name).IsEqualTo("Outer");
        await Assert.That(function.Parameters.Count).IsEqualTo(3);
        await AssertValue(function.Parameters[0], 'x');
        await Assert.That(function.Parameters[1]).IsTypeOf<Function>();
        var nested = (Function)function.Parameters[1];
        await Assert.That(nested.Identifier.Name).IsEqualTo("Inner");
        await Assert.That(nested.Parameters.Count).IsEqualTo(2);
        await AssertValue(nested.Parameters[0], 42L);
        await AssertValue(nested.Parameters[1], 1.5m);
        await AssertValue(function.Parameters[2], 2L);
    }

    [Test]
    public async Task ShouldRetainIndependentCulturesAndSeparators()
    {
        var commaOptions = new LogicalExpressionParserOptions { ArgumentSeparator = ArgumentSeparator.Comma };
        var semicolonOptions = new LogicalExpressionParserOptions { ArgumentSeparator = ArgumentSeparator.Semicolon };
        var usCulture = CultureInfo.GetCultureInfo("en-US");
        var gbCulture = CultureInfo.GetCultureInfo("en-GB");
        var usCommaParser = LogicalExpressionParser.GetOrCreateExpressionParser(commaOptions, usCulture);
        var gbCommaParser = LogicalExpressionParser.GetOrCreateExpressionParser(commaOptions, gbCulture);
        var gbSemicolonParser = LogicalExpressionParser.GetOrCreateExpressionParser(semicolonOptions, gbCulture);

        await Assert.That(usCommaParser).IsNotSameReferenceAs(gbCommaParser);
        await Assert.That(gbCommaParser).IsNotSameReferenceAs(gbSemicolonParser);
        await Assert.That(usCommaParser.GetType().DeclaringType).IsEqualTo(typeof(LogicalExpressionParser));
        await Assert.That(gbCommaParser.GetType().DeclaringType).IsEqualTo(typeof(LogicalExpressionParser));
        await Assert.That(gbSemicolonParser.GetType().DeclaringType).IsEqualTo(typeof(LogicalExpressionParser));

        for (var i = 0; i < 2; i++)
        {
            var usDates = Parse(usCommaParser, "(#01/02/2025#, #03/04/2025#)", commaOptions);
            await Assert.That(usDates).IsTypeOf<LogicalExpressionList>();
            await AssertValue(((LogicalExpressionList)usDates)[0], new DateTime(2025, 1, 2));
            await AssertValue(((LogicalExpressionList)usDates)[1], new DateTime(2025, 3, 4));

            await AssertValue(Parse(gbCommaParser, "#01/02/2025#", commaOptions), new DateTime(2025, 2, 1));
            var gbDates = Parse(gbSemicolonParser, "(#01/02/2025#; #03/04/2025#)", semicolonOptions);
            await Assert.That(gbDates).IsTypeOf<LogicalExpressionList>();
            await AssertValue(((LogicalExpressionList)gbDates)[0], new DateTime(2025, 2, 1));
            await AssertValue(((LogicalExpressionList)gbDates)[1], new DateTime(2025, 4, 3));

            await Assert.That(usCommaParser.TryParse(
                new LogicalExpressionParseContext("Max(1; 2)", commaOptions), out _, out _)).IsFalse();
            await Assert.That(gbSemicolonParser.TryParse(
                new LogicalExpressionParseContext("Max(1, 2)", semicolonOptions), out _, out _)).IsFalse();
        }
    }

    [Test]
    [Arguments("/")]
    [Arguments("@@")]
    public async Task ShouldKeepCustomCulturesWithTheSameNameIndependent(string separator)
    {
        var monthFirst = (CultureInfo)CultureInfo.InvariantCulture.Clone();
        monthFirst.DateTimeFormat.ShortDatePattern = "MM/dd/yyyy";
        monthFirst.DateTimeFormat.DateSeparator = separator;
        monthFirst = CultureInfo.ReadOnly(monthFirst);

        var dayFirst = (CultureInfo)CultureInfo.InvariantCulture.Clone();
        dayFirst.DateTimeFormat.ShortDatePattern = "dd/MM/yyyy";
        dayFirst.DateTimeFormat.DateSeparator = separator;
        dayFirst = CultureInfo.ReadOnly(dayFirst);

        var options = new LogicalExpressionParserOptions();
        var monthFirstParser = LogicalExpressionParser.GetOrCreateExpressionParser(options, monthFirst);
        var dayFirstParser = LogicalExpressionParser.GetOrCreateExpressionParser(options, dayFirst);
        var text = $"#01{separator}02{separator}2025#";

        await Assert.That(monthFirst.Name).IsEqualTo(dayFirst.Name);
        await Assert.That(monthFirstParser).IsNotSameReferenceAs(dayFirstParser);
        await Assert.That(LogicalExpressionParser.GetOrCreateExpressionParser(options, monthFirst))
            .IsSameReferenceAs(monthFirstParser);

        for (var i = 0; i < 2; i++)
        {
            await AssertValue(Parse(monthFirstParser, text, options), new DateTime(2025, 1, 2));
            await AssertValue(Parse(dayFirstParser, text, options), new DateTime(2025, 2, 1));
        }
    }

    [Test]
    public async Task ShouldBindParserCultureButUseCurrentCultureAtPublicParseCall()
    {
        var originalCulture = CultureInfo.CurrentCulture;

        try
        {
            var options = new LogicalExpressionParserOptions();
            var usCulture = CultureInfo.GetCultureInfo("en-US");
            CultureInfo.CurrentCulture = usCulture;
            var usParser = LogicalExpressionParser.GetOrCreateExpressionParser(options, CultureInfo.CurrentCulture);
            var context = new LogicalExpressionParseContext("#01/02/2025#", options);
            await AssertValue(LogicalExpressionFactory.Create("#01/02/2025#", options), new DateTime(2025, 1, 2));

            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-GB");
            var gbParser = LogicalExpressionParser.GetOrCreateExpressionParser(options, CultureInfo.CurrentCulture);

            await AssertValue(Parse(usParser, "#01/02/2025#", options), new DateTime(2025, 1, 2));
            await AssertValue(Parse(gbParser, "#01/02/2025#", options), new DateTime(2025, 2, 1));
            await AssertValue(LogicalExpressionParser.Parse(context), new DateTime(2025, 2, 1));
            await AssertValue(LogicalExpressionParser.Parse(
                new LogicalExpressionParseContext("#01/02/2025#", options), usCulture), new DateTime(2025, 1, 2));
            await AssertValue(LogicalExpressionFactory.Create("#01/02/2025#", options), new DateTime(2025, 2, 1));
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Test]
    [Arguments("2 + 3 = 5", "2 + 3 == 5")]
    [Arguments("(5!) = 120", "(5!) == 120")]
    public async Task ShouldRetainIndependentSingleEqualsOptions(string singleEquals, string doubleEquals)
    {
        var defaultOptions = new LogicalExpressionParserOptions();
        var strictOptions = new LogicalExpressionParserOptions { DisallowSingleEquals = true };
        var defaultParser = LogicalExpressionParser.GetOrCreateExpressionParser(defaultOptions, CultureInfo.InvariantCulture);
        var strictParser = LogicalExpressionParser.GetOrCreateExpressionParser(strictOptions, CultureInfo.InvariantCulture);

        await Assert.That(strictParser).IsNotSameReferenceAs(defaultParser);
        await Assert.That(strictParser.GetType().DeclaringType).IsEqualTo(typeof(LogicalExpressionParser));

        for (var i = 0; i < 2; i++)
        {
            await Assert.That(new Expression(Parse(defaultParser, singleEquals, defaultOptions))
                .Evaluate<bool>(CancellationToken.None)).IsTrue();
            await Assert.That(strictParser.TryParse(
                new LogicalExpressionParseContext(singleEquals, strictOptions), out _, out _)).IsFalse();
            await Assert.That(new Expression(Parse(strictParser, doubleEquals, strictOptions))
                .Evaluate<bool>(CancellationToken.None)).IsTrue();
            await Assert.That(new Expression(Parse(strictParser, "5 != 3", strictOptions))
                .Evaluate<bool>(CancellationToken.None)).IsTrue();
        }
    }

    [Test]
    [Arguments(@"'\u0048\u0065\u006C\u006C\u006F'", "Hello")]
    [Arguments(@"'\'hello\''", "'hello'")]
    [Arguments(@"'hel\nlo'", "hel\nlo")]
    [Arguments(@"'\q'", @"\q")]
    [Arguments("\"\\u0041\\t\\\\\"", "A\t\\")]
    public async Task ShouldPreserveStringEscaping(string text, string expected)
    {
        var options = new LogicalExpressionParserOptions { AllowCharValues = true };
        var parser = LogicalExpressionParser.GetOrCreateExpressionParser(options, CultureInfo.InvariantCulture);

        await AssertValue(Parse(parser, text, options), expected);
    }

    [Test]
    [Arguments(2)]
    [Arguments(3)]
    [Arguments(4)]
    public async Task ShouldPreserveNullCoalescingAssociativity(int operatorCount)
    {
        var options = new LogicalExpressionParserOptions();
        var parser = LogicalExpressionParser.GetOrCreateExpressionParser(options, CultureInfo.InvariantCulture);
        var identifiers = new[] { "a", "b", "c", "d", "e" }.Take(operatorCount + 1).ToArray();
        var text = string.Join(" ?? ", identifiers.Select(name => $"[{name}]"));
        var expression = Parse(parser, text, options);

        await Assert.That(parser.GetType().DeclaringType).IsEqualTo(typeof(LogicalExpressionParser));

        var current = expression;
        for (var i = 0; i < operatorCount; i++)
        {
            var binary = await AssertBinary(current, BinaryExpressionType.Coalesce);
            await AssertIdentifier(binary.LeftExpression, identifiers[i]);
            current = binary.RightExpression;
        }
        await AssertIdentifier(current, identifiers[^1]);

        var evaluation = new Expression(expression);
        foreach (var identifier in identifiers)
        {
            evaluation.Parameters[identifier] = null;
        }
        evaluation.Parameters[identifiers[^1]] = 42;
        await Assert.That(evaluation.Evaluate(CancellationToken.None)).IsEqualTo(42);
    }

    [Test]
    [Arguments("or", "and")]
    [Arguments("||", "&&")]
    public async Task ShouldPreserveCoalescingPrecedenceWithTernaryAndLogic(string orOperator, string andOperator)
    {
        var options = new LogicalExpressionParserOptions();
        var parser = LogicalExpressionParser.GetOrCreateExpressionParser(options, CultureInfo.InvariantCulture);
        var expression = Parse(parser,
            $"[a] {orOperator} [b] ?? [c] {andOperator} [d] ?? [e] ?? [f] ? [g] ?? [h] : [i] ?? [j]", options);

        await Assert.That(parser.GetType().DeclaringType).IsEqualTo(typeof(LogicalExpressionParser));
        await Assert.That(expression).IsTypeOf<TernaryExpression>();
        var ternary = (TernaryExpression)expression;

        var condition = await AssertBinary(ternary.LeftExpression, BinaryExpressionType.Coalesce);
        var logicalOr = await AssertBinary(condition.LeftExpression, BinaryExpressionType.Or);
        await AssertIdentifier(logicalOr.LeftExpression, "a");
        await AssertIdentifier(logicalOr.RightExpression, "b");

        var conditionMiddle = await AssertBinary(condition.RightExpression, BinaryExpressionType.Coalesce);
        var logicalAnd = await AssertBinary(conditionMiddle.LeftExpression, BinaryExpressionType.And);
        await AssertIdentifier(logicalAnd.LeftExpression, "c");
        await AssertIdentifier(logicalAnd.RightExpression, "d");

        var conditionTail = await AssertBinary(conditionMiddle.RightExpression, BinaryExpressionType.Coalesce);
        await AssertIdentifier(conditionTail.LeftExpression, "e");
        await AssertIdentifier(conditionTail.RightExpression, "f");

        var whenTrue = await AssertBinary(ternary.MiddleExpression, BinaryExpressionType.Coalesce);
        await AssertIdentifier(whenTrue.LeftExpression, "g");
        await AssertIdentifier(whenTrue.RightExpression, "h");

        var whenFalse = await AssertBinary(ternary.RightExpression, BinaryExpressionType.Coalesce);
        await AssertIdentifier(whenFalse.LeftExpression, "i");
        await AssertIdentifier(whenFalse.RightExpression, "j");
    }

    [Test]
    public void ShouldHonorCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var options = new LogicalExpressionParserOptions();
        var context = new LogicalExpressionParseContext("1 + 2", options, cancellation.Token);
        var parser = LogicalExpressionParser.GetOrCreateExpressionParser(options, CultureInfo.InvariantCulture);
        Assert.Throws<OperationCanceledException>(() =>
        {
            var result = new ParseResult<LogicalExpression>();
            parser.Parse(context, ref result);
        });
    }

    private static LogicalExpression Parse(
        Parser<LogicalExpression> parser, string text, LogicalExpressionParserOptions options)
    {
        var context = new LogicalExpressionParseContext(text, options, CancellationToken.None);
        if (parser.TryParse(context, out var expression, out var error))
        {
            return expression;
        }

        throw new InvalidOperationException($"Failed to parse '{text}': {error?.Message}");
    }

    private static async Task<BinaryExpression> AssertBinary(LogicalExpression expression, BinaryExpressionType expectedType)
    {
        await Assert.That(expression).IsTypeOf<BinaryExpression>();
        var binary = (BinaryExpression)expression;
        await Assert.That(binary.Type).IsEqualTo(expectedType);
        return binary;
    }

    private static async Task AssertIdentifier(LogicalExpression expression, string expectedName)
    {
        await Assert.That(expression).IsTypeOf<Identifier>();
        await Assert.That(((Identifier)expression).Name).IsEqualTo(expectedName);
    }

    private static async Task AssertValue(LogicalExpression expression, object expected)
    {
        await Assert.That(expression).IsTypeOf<ValueExpression>();
        var value = ((ValueExpression)expression).Value;
        await Assert.That(value.GetType()).IsEqualTo(expected.GetType());
        await Assert.That(value).IsEqualTo(expected);
    }
}
