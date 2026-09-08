using NCalc.Exceptions;
using NCalc.Factories;

namespace NCalc.Tests;

[Property("Category", "Parser")]
public class GeneratedParserTests
{
    [Test]
    public async Task ShouldParseAndEvaluateGeneratedExpressionsAcrossAssemblies()
    {
        await AssertValue(LogicalExpressionParser.Parse("42", culture: CultureInfo.InvariantCulture), 42);

        var expression = LogicalExpressionParser.Parse("[value] + 2", culture: CultureInfo.InvariantCulture);
        var sum = await AssertBinary(expression, BinaryExpressionType.Plus);
        await AssertIdentifier(sum.LeftExpression, "value");
        await AssertValue(sum.RightExpression, 2);

        var evaluation = new Expression(expression);
        evaluation.Parameters["value"] = 40;
        await Assert.That(evaluation.Evaluate(CancellationToken.None)).IsEqualTo(42);
    }

    [Test]
    [Arguments("42", 42)]
    [Arguments("1.5", 1.5d)]
    [Arguments("'x'", "x")]
    [Arguments("\"x\"", "x")]
    [Arguments(" 42 \t\r\n", 42)]
    public async Task ShouldUseDefaultOptionsForBothPublicParseOverloads(string text, object expected)
    {
        await AssertValue(LogicalExpressionParser.Parse(text), expected);
        await AssertValue(LogicalExpressionParser.Parse(text, new LogicalExpressionParserOptions()), expected);
        await AssertValue(LogicalExpressionParser.Parse(new LogicalExpressionParseContext(text)), expected);
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

        for (var i = 0; i < 4; i++)
        {
            await AssertValue(LogicalExpressionParser.Parse("42", defaultOptions, CultureInfo.InvariantCulture), 42);
            await AssertValue(LogicalExpressionParser.Parse("1.5", defaultOptions, CultureInfo.InvariantCulture), 1.5d);
            await AssertValue(LogicalExpressionParser.Parse("'x'", defaultOptions, CultureInfo.InvariantCulture), "x");

            await AssertValue(LogicalExpressionParser.Parse("42", configuredOptions, CultureInfo.InvariantCulture),
                integerType == IntegerNumberType.Int64 ? (object)42L : 42);
            await AssertValue(LogicalExpressionParser.Parse("1.5", configuredOptions, CultureInfo.InvariantCulture),
                floatingPointType == FloatingPointNumberType.Decimal ? (object)1.5m : 1.5d);
            await AssertValue(LogicalExpressionParser.Parse("'x'", configuredOptions, CultureInfo.InvariantCulture),
                allowCharValues ? (object)'x' : "x");
            await AssertValue(LogicalExpressionParser.Parse("\"x\"", configuredOptions, CultureInfo.InvariantCulture), "x");
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
        var options = new LogicalExpressionParserOptions
        {
            ArgumentSeparator = separator,
            AllowCharValues = true,
            IntegerNumberType = IntegerNumberType.Int64,
            FloatingPointNumberType = FloatingPointNumberType.Decimal
        };

        foreach (var expression in new[]
        {
            LogicalExpressionParser.Parse(text, options, CultureInfo.InvariantCulture),
            LogicalExpressionParser.Parse(new LogicalExpressionParseContext(text, options), CultureInfo.InvariantCulture)
        })
        {
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
        var expression = LogicalExpressionParser.Parse("Outer('x', Inner(42; 1.5): 2)", options, CultureInfo.InvariantCulture);

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

        for (var i = 0; i < 2; i++)
        {
            var usDates = LogicalExpressionParser.Parse("(#01/02/2025#, #03/04/2025#)", commaOptions, usCulture);
            await Assert.That(usDates).IsTypeOf<LogicalExpressionList>();
            await AssertValue(((LogicalExpressionList)usDates)[0], new DateTime(2025, 1, 2));
            await AssertValue(((LogicalExpressionList)usDates)[1], new DateTime(2025, 3, 4));

            await AssertValue(LogicalExpressionParser.Parse("#01/02/2025#", commaOptions, gbCulture), new DateTime(2025, 2, 1));
            var gbDates = LogicalExpressionParser.Parse("(#01/02/2025#; #03/04/2025#)", semicolonOptions, gbCulture);
            await Assert.That(gbDates).IsTypeOf<LogicalExpressionList>();
            await AssertValue(((LogicalExpressionList)gbDates)[0], new DateTime(2025, 2, 1));
            await AssertValue(((LogicalExpressionList)gbDates)[1], new DateTime(2025, 4, 3));

            Assert.Throws<NCalcParserException>(() => LogicalExpressionParser.Parse("Max(1; 2)", commaOptions, usCulture));
            Assert.Throws<NCalcParserException>(() => LogicalExpressionParser.Parse("Max(1, 2)", semicolonOptions, gbCulture));
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
        var text = $"#01{separator}02{separator}2025#";

        await Assert.That(monthFirst.Name).IsEqualTo(dayFirst.Name);

        for (var i = 0; i < 2; i++)
        {
            await AssertValue(LogicalExpressionParser.Parse(text, options, monthFirst), new DateTime(2025, 1, 2));
            await AssertValue(LogicalExpressionParser.Parse(text, options, dayFirst), new DateTime(2025, 2, 1));
        }
    }

    [Test]
    public async Task ShouldResolveCurrentCultureAtEachPublicParseCallIndependentlyOfExplicitCulture()
    {
        var originalCulture = CultureInfo.CurrentCulture;

        try
        {
            var options = new LogicalExpressionParserOptions();
            var usCulture = CultureInfo.GetCultureInfo("en-US");
            var gbCulture = CultureInfo.GetCultureInfo("en-GB");
            CultureInfo.CurrentCulture = usCulture;
            const string text = "#01/02/2025#";
            var context = new LogicalExpressionParseContext(text, options);

            for (var i = 0; i < 4; i++)
            {
                var useUsCulture = i % 2 == 0;
                CultureInfo.CurrentCulture = useUsCulture ? usCulture : gbCulture;
                var expected = useUsCulture ? new DateTime(2025, 1, 2) : new DateTime(2025, 2, 1);

                await AssertValue(LogicalExpressionParser.Parse(text), expected);
                await AssertValue(LogicalExpressionParser.Parse(context), expected);
                await AssertValue(LogicalExpressionParser.Parse(text, options, usCulture), new DateTime(2025, 1, 2));
                await AssertValue(LogicalExpressionParser.Parse(context, usCulture), new DateTime(2025, 1, 2));
                await AssertValue(LogicalExpressionParser.Parse(text, options, gbCulture), new DateTime(2025, 2, 1));
                await AssertValue(LogicalExpressionParser.Parse(context, gbCulture), new DateTime(2025, 2, 1));
                await AssertValue(LogicalExpressionParser.Parse(text, options), expected);
                await AssertValue(LogicalExpressionFactory.Create(text, options), expected);
            }
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

        for (var i = 0; i < 2; i++)
        {
            await Assert.That(new Expression(LogicalExpressionParser.Parse(singleEquals, defaultOptions, CultureInfo.InvariantCulture))
                .Evaluate<bool>(CancellationToken.None)).IsTrue();
            Assert.Throws<NCalcParserException>(() => LogicalExpressionParser.Parse(singleEquals, strictOptions, CultureInfo.InvariantCulture));
            await Assert.That(new Expression(LogicalExpressionParser.Parse(doubleEquals, strictOptions, CultureInfo.InvariantCulture))
                .Evaluate<bool>(CancellationToken.None)).IsTrue();
            await Assert.That(new Expression(LogicalExpressionParser.Parse("5 != 3", strictOptions, CultureInfo.InvariantCulture))
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

        await AssertValue(LogicalExpressionParser.Parse(text, options, CultureInfo.InvariantCulture), expected);
    }

    [Test]
    [Arguments(2)]
    [Arguments(3)]
    [Arguments(4)]
    public async Task ShouldPreserveNullCoalescingAssociativity(int operatorCount)
    {
        var identifiers = new[] { "a", "b", "c", "d", "e" }.Take(operatorCount + 1).ToArray();
        var text = string.Join(" ?? ", identifiers.Select(name => $"[{name}]"));
        var expression = LogicalExpressionParser.Parse(text, culture: CultureInfo.InvariantCulture);

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
        var expression = LogicalExpressionParser.Parse(
            $"[a] {orOperator} [b] ?? [c] {andOperator} [d] ?? [e] ?? [f] ? [g] ?? [h] : [i] ?? [j]",
            culture: CultureInfo.InvariantCulture);

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
    public async Task ShouldReportInvalidTokenPositionFromBothPublicParseOverloads()
    {
        var exception = Assert.Throws<NCalcParserException>(() => LogicalExpressionParser.Parse("42a"));
        var contextException = Assert.Throws<NCalcParserException>(() =>
            LogicalExpressionParser.Parse(new LogicalExpressionParseContext("42a")));

        await Assert.That(exception.Message).IsEqualTo("Invalid token in expression at position (1:3)");
        await Assert.That(contextException.Message).IsEqualTo(exception.Message);
    }

    [Test]
    [Arguments("[value", "Brace not closed. at position ")]
    [Arguments("{value", "Brace not closed. at position ")]
    [Arguments("(1 + 2", "Parenthesis not closed. at position ")]
    [Arguments("Max(1, 2", "Parenthesis not closed. at position ")]
    public async Task ShouldPreserveUnmatchedDelimiterMessages(string text, string expectedMessage)
    {
        var exception = Assert.Throws<NCalcParserException>(() => LogicalExpressionParser.Parse(text));
        var contextException = Assert.Throws<NCalcParserException>(() =>
            LogicalExpressionParser.Parse(new LogicalExpressionParseContext(text)));

        await Assert.That(exception.Message.StartsWith(expectedMessage, StringComparison.Ordinal)).IsTrue();
        await Assert.That(contextException.Message).IsEqualTo(exception.Message);
    }

    [Test]
    [Arguments("42a")]
    [Arguments("42 43")]
    [Arguments("42 'trailing'")]
    [Arguments("Abs(-1) ]")]
    [Arguments("(1 + 2))")]
    [Arguments("[value] trailing")]
    [Arguments("true false")]
    [Arguments("42 +")]
    public void ShouldRejectUnconsumedInputFromBothPublicParseOverloads(string text)
    {
        Assert.Throws<NCalcParserException>(() => LogicalExpressionParser.Parse(text));
        Assert.Throws<NCalcParserException>(() => LogicalExpressionParser.Parse(new LogicalExpressionParseContext(text)));
    }

    [Test]
    public void ShouldHonorCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        Assert.Throws<OperationCanceledException>(() =>
            LogicalExpressionParser.Parse("1 + 2", cancellationToken: cancellation.Token));
        Assert.Throws<OperationCanceledException>(() => LogicalExpressionParser.Parse(
            new LogicalExpressionParseContext("1 + 2", cancellationToken: cancellation.Token)));
    }

    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task ShouldHonorCancellationDuringParsing(bool useContext)
    {
        using var cancellation = new CancellationTokenSource();
        var culture = new CancellingCulture(cancellation);
        var text = "#01/02/2025# + " + string.Join(" + ", Enumerable.Repeat("1", 128));

        await Assert.That(cancellation.IsCancellationRequested).IsFalse();

        var exception = Assert.Throws<OperationCanceledException>(() =>
        {
            if (useContext)
                LogicalExpressionParser.Parse(new LogicalExpressionParseContext(text, cancellationToken: cancellation.Token), culture);
            else
                LogicalExpressionParser.Parse(text, culture: culture, cancellationToken: cancellation.Token);
        });

        await Assert.That(exception.CancellationToken).IsEqualTo(cancellation.Token);
    }

    private sealed class CancellingCulture(CancellationTokenSource cancellation) : CultureInfo("en-US")
    {
        public override DateTimeFormatInfo DateTimeFormat
        {
            get
            {
                // Cancel inside a grammar callback, after the generated entrypoint's initial check.
                cancellation.Cancel();
                return base.DateTimeFormat;
            }
            set => base.DateTimeFormat = value;
        }
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
