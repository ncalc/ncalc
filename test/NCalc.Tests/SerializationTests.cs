using NCalc.Factories;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace NCalc.Tests;

[Property("Category", "Serialization")]
public class SerializationTests
{
    [Test]
    [MethodDataSource(typeof(WaterLevelCheckTestData), "GetEnumerator")]
    public async Task SerializeAndDeserializeShouldWork(string expression, bool expected, double inputValue)
    {
        var compiled = LogicalExpressionFactory.Create(expression, cancellationToken: CancellationToken.None);
        var serialized = JsonSerializer.Serialize(compiled, NCalcTestJsonContext.Default.LogicalExpression);
        var deserialized = JsonSerializer.Deserialize(serialized, NCalcTestJsonContext.Default.LogicalExpression);

        var exp = new Expression(deserialized, ExpressionOptions.NoCache)
        {
            Parameters =
            {
                { "waterlevel", inputValue }
            }
        };

        object evaluated;
        try
        {
            evaluated = exp.Evaluate(CancellationToken.None);
        }
        catch
        {
            evaluated = false;
        }

        // Assert
        await Assert.That(evaluated).IsEqualTo(expected);
    }

#if NET
    [Test]
    public async Task SystemTextJsonPolymorphicSerializeAndDeserializeShouldWork()
    {
        var expression = LogicalExpressionFactory.Create("1 == 1", cancellationToken: CancellationToken.None);
        var expressionJson = JsonSerializer.Serialize(expression, NCalcTestJsonContext.Default.LogicalExpression);
        var deserialized = JsonSerializer.Deserialize(expressionJson, NCalcTestJsonContext.Default.LogicalExpression);
        await Assert.That(deserialized is BinaryExpression).IsTrue();
    }

    [Test]
    public async Task CoalesceExpressionShouldSerializeAndDeserialize()
    {
        var logicalExpression = LogicalExpressionFactory.Create("[value] ?? 'fallback'", cancellationToken: CancellationToken.None);
        var json = JsonSerializer.Serialize(logicalExpression, NCalcTestJsonContext.Default.LogicalExpression);
        var deserialized = JsonSerializer.Deserialize(json, NCalcTestJsonContext.Default.LogicalExpression);
        var expression = new Expression(deserialized!)
        {
            Parameters = { ["value"] = null }
        };

        await Assert.That(expression.Evaluate(CancellationToken.None)).IsEqualTo("fallback");
    }
#endif

    [Test]
    public async Task Binary_Expression_Serialization_Test()
    {
        await Assert.That(new BinaryExpression(BinaryExpressionType.And, new ValueExpression(true), new ValueExpression(false))
        .ToExpressionString()).IsEqualTo("True and False");
        await Assert.That(new BinaryExpression(BinaryExpressionType.Div, new ValueExpression(1), new ValueExpression(2)).ToExpressionString()).IsEqualTo("1 / 2");
        await Assert.That(new BinaryExpression(BinaryExpressionType.Equal, new ValueExpression(1), new ValueExpression(2))
                .ToExpressionString()).IsEqualTo("1 = 2");
        await Assert.That(new BinaryExpression(BinaryExpressionType.Greater, new ValueExpression(1), new ValueExpression(2))
                .ToExpressionString()).IsEqualTo("1 > 2");
        await Assert.That(new BinaryExpression(BinaryExpressionType.GreaterOrEqual, new ValueExpression(1), new ValueExpression(2))
                .ToExpressionString()).IsEqualTo("1 >= 2");
        await Assert.That(new BinaryExpression(BinaryExpressionType.Lesser, new ValueExpression(1), new ValueExpression(2))
                .ToExpressionString()).IsEqualTo("1 < 2");
        await Assert.That(new BinaryExpression(BinaryExpressionType.LesserOrEqual, new ValueExpression(1), new ValueExpression(2))
                .ToExpressionString()).IsEqualTo("1 <= 2");
        await Assert.That(new BinaryExpression(BinaryExpressionType.Minus, new ValueExpression(1), new ValueExpression(2))
                .ToExpressionString()).IsEqualTo("1 - 2");
        await Assert.That(new BinaryExpression(BinaryExpressionType.Modulo, new ValueExpression(1), new ValueExpression(2))
                .ToExpressionString()).IsEqualTo("1 % 2");
        await Assert.That(new BinaryExpression(BinaryExpressionType.NotEqual, new ValueExpression(1), new ValueExpression(2))
                .ToExpressionString()).IsEqualTo("1 != 2");
        await Assert.That(new BinaryExpression(BinaryExpressionType.Or, new ValueExpression(true), new ValueExpression(false))
                .ToExpressionString()).IsEqualTo("True or False");
        await Assert.That(new BinaryExpression(BinaryExpressionType.Plus, new ValueExpression(1), new ValueExpression(2)).ToExpressionString()).IsEqualTo("1 + 2");
        await Assert.That(new BinaryExpression(BinaryExpressionType.Times, new ValueExpression(1), new ValueExpression(2))
                .ToExpressionString()).IsEqualTo("1 * 2");
        await Assert.That(new BinaryExpression(BinaryExpressionType.Coalesce, new Identifier("value"), new ValueExpression(true))
                .ToExpressionString()).IsEqualTo("([value]) ?? True");
    }

    [Test]
    public async Task Unary_Expression_Serialization_Test()
    {
        await Assert.That(new UnaryExpression(UnaryExpressionType.Negate,
                new BinaryExpression(BinaryExpressionType.And, new ValueExpression(true),
                        new ValueExpression(false)))
                .ToExpressionString()).IsEqualTo("-(True and False)");
        await Assert.That(new UnaryExpression(UnaryExpressionType.Not,
                    new BinaryExpression(BinaryExpressionType.And, new ValueExpression(true),
                        new ValueExpression(false)))
                .ToExpressionString()).IsEqualTo("!(True and False)");
    }

    [Test]
    public async Task Function_Serialization_Test()
    {
        await Assert.That(new Function(new Identifier("test"), new LogicalExpressionList([
            new BinaryExpression(BinaryExpressionType.And, new ValueExpression(true), new ValueExpression(false)),
            new UnaryExpression(UnaryExpressionType.Negate,
                new BinaryExpression(BinaryExpressionType.And, new ValueExpression(true), new ValueExpression(false)))
        ])).ToExpressionString()).IsEqualTo("test(True and False, -(True and False))");

        await Assert.That(new Function(new Identifier("Sum"), new LogicalExpressionList([
            new BinaryExpression(BinaryExpressionType.Plus, new ValueExpression(1), new ValueExpression(2))
        ])).ToExpressionString()).IsEqualTo("Sum(1 + 2)");
    }

    [Test]
    public async Task Value_Serialization_Test()
    {
        await Assert.That(new ValueExpression(true).ToExpressionString()).IsEqualTo("True");
        await Assert.That(new ValueExpression(false).ToExpressionString()).IsEqualTo("False");
        await Assert.That(new ValueExpression(1).ToExpressionString()).IsEqualTo("1");
        await Assert.That(new ValueExpression(1.234).ToExpressionString()).IsEqualTo("1.234");
        await Assert.That(new ValueExpression("hello").ToExpressionString()).IsEqualTo("'hello'");
        await Assert.That(new ValueExpression('c').ToExpressionString()).IsEqualTo("'c'");
        await Assert.That(new ValueExpression(new Guid("8a7b6c5d-1234-4abc-9def-0123456789ab")).ToExpressionString())
            .IsEqualTo("8a7b6c5d-1234-4abc-9def-0123456789ab");
    }

    [Test]
    public async Task ArraySerializationTest()
    {
        var trueArrayExpression = new LogicalExpressionList([new ValueExpression(true)]);
        var helloWorldArrayExpression = new LogicalExpressionList([new ValueExpression("Hello"), new ValueExpression("World")]);
        await Assert.That(trueArrayExpression.ToExpressionString()).IsEqualTo("(True)");
        await Assert.That(helloWorldArrayExpression.ToExpressionString()).IsEqualTo("('Hello','World')");
        await Assert.That(new LogicalExpressionList([]).ToExpressionString()).IsEqualTo("()");
        await Assert.That(new LogicalExpressionList([trueArrayExpression, helloWorldArrayExpression]).ToExpressionString()).IsEqualTo("((True),('Hello','World'))");
    }

    [Test]
    public async Task FunctionWithParametersSerializationTest()
    {
        var expr = new Expression("Max([a], [b])");
        expr.Parameters["a"] = 5;
        expr.Parameters["b"] = 10;
        expr.Evaluate(CancellationToken.None);

        var exprString = expr.LogicalExpression.ToExpressionString();
        await Assert.That(exprString).IsEqualTo("Max([a], [b])");
    }

    [Test]
    public async Task ShouldSerializeExpressionWithParameterValues()
    {
        var expression = new Expression("[ValueA] + [ValueB]");
        expression.Parameters["ValueA"] = 10;
        expression.Parameters["ValueB"] = 15;

        await Assert.That(expression.ToExpressionString(evaluateParameters: true)).IsEqualTo("10 + 15");
    }

    [Test]
    public async Task ShouldSerializeGuidLiteralsSoTheyParseBack()
    {
        var logicalExpression = LogicalExpressionFactory.Create(
            "8a7b6c5d-1234-4abc-9def-0123456789ab = 8a7b6c5d12344abc9def0123456789ab",
            cancellationToken: CancellationToken.None);

        var serialized = logicalExpression.ToExpressionString();

        await Assert.That(serialized)
            .IsEqualTo("8a7b6c5d-1234-4abc-9def-0123456789ab = 8a7b6c5d-1234-4abc-9def-0123456789ab");
        await Assert.That(new Expression(serialized).Evaluate(CancellationToken.None)).IsEqualTo(true);
    }

    [Test]
    [Arguments("'it\\'s'")]
    [Arguments("'C:\\\\temp\\\\new'")]
    [Arguments("'line1\\r\\nline2'")]
    public async Task ShouldSerializeStringLiteralsSoTheyParseBack(string source)
    {
        var logicalExpression = LogicalExpressionFactory.Create(source, cancellationToken: CancellationToken.None);

        var serialized = logicalExpression.ToExpressionString();

        await Assert.That(new Expression(serialized).Evaluate(CancellationToken.None))
            .IsEqualTo(new Expression(source).Evaluate(CancellationToken.None));
    }

    [Test]
    [Arguments(@"\u0041")]
    [Arguments(@"C:\users\u0041")]
    public async Task ShouldSerializeBackslashFollowedByUnicodeLikeTextSoItParsesBack(string value)
    {
        var serialized = new ValueExpression(value).ToExpressionString();

        await Assert.That(new Expression(serialized).Evaluate(CancellationToken.None)).IsEqualTo(value);
    }

    [Test]
    [Arguments("", "#01/01/2009 00:00:00#")]
    [Arguments("en-US", "#1/1/2009 00:00:00#")]
    [Arguments("de-DE", "#01.01.2009 00:00:00#")]
    [Arguments("ja-JP", "#2009/01/01 00:00:00#")]
    public async Task ShouldSerializeDateTimeValuesInTheCurrentCulture(string culture, string expected)
    {
        var oldCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture);

        try
        {
            await Assert.That(new ValueExpression(new DateTime(2009, 1, 1)).ToExpressionString()).IsEqualTo(expected);
        }
        finally
        {
            CultureInfo.CurrentCulture = oldCulture;
        }
    }

    [Test]
    [Arguments("")]
    [Arguments("en-US")]
    [Arguments("de-DE")]
    [Arguments("zh-TW")]
    [Arguments("ko-KR")]
    public async Task ShouldSerializeDateTimeValuesSoTheyParseBack(string culture)
    {
        var oldCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture);

        try
        {
            foreach (var value in new[]
                     {
                         new DateTime(2009, 1, 1),
                         new DateTime(2026, 11, 22, 15, 4, 5),
                         new DateTime(2026, 11, 22, 15, 4, 5, 250)
                     })
            {
                var serialized = new ValueExpression(value).ToExpressionString();

                await Assert.That(new Expression(serialized).Evaluate(CancellationToken.None)).IsEqualTo(value);
            }
        }
        finally
        {
            CultureInfo.CurrentCulture = oldCulture;
        }
    }

    [Test]
    public async Task ShouldSerializeDateTimeParameterValuesSoTheyParseBack()
    {
        var oldCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");

        try
        {
            var expression = new Expression("[Due] > #11/1/2026#");
            expression.Parameters["Due"] = new DateTime(2026, 11, 22, 15, 4, 5);

            var serialized = expression.ToExpressionString(evaluateParameters: true);

            await Assert.That(serialized).IsEqualTo("#11/22/2026 15:04:05# > #11/1/2026 00:00:00#");
            await Assert.That((bool)new Expression(serialized).Evaluate(CancellationToken.None)!).IsTrue();
        }
        finally
        {
            CultureInfo.CurrentCulture = oldCulture;
        }
    }

    [Test]
    public async Task ShouldEscapeStringParameterValues()
    {
        var expression = new Expression("[Name] = 'x'");
        expression.Parameters["Name"] = "O'Brien";

        await Assert.That(expression.ToExpressionString(evaluateParameters: true))
            .IsEqualTo("'O\\'Brien' = 'x'");
    }

    [Test]
    public async Task ShouldSerializeExpressionWithGuidParameterValue()
    {
        var expression = new Expression("[Id] = [Other]");
        expression.Parameters["Id"] = new Guid("8a7b6c5d-1234-4abc-9def-0123456789ab");

        await Assert.That(expression.ToExpressionString(evaluateParameters: true))
            .IsEqualTo("8a7b6c5d-1234-4abc-9def-0123456789ab = [Other]");
    }

    [Test]
    public async Task ShouldNotSerializeExpressionWithParameterValuesByDefault()
    {
        var expression = new Expression("[ValueA] + [ValueB]");
        expression.Parameters["ValueA"] = 10;
        expression.Parameters["ValueB"] = 15;

        await Assert.That(expression.ToExpressionString()).IsEqualTo("([ValueA]) + ([ValueB])");
    }

    [Test]
    public async Task ShouldSerializeLogicalExpressionWithContextParameterValues()
    {
        var context = new ExpressionContext(new Dictionary<string, object>
        {
            ["ValueA"] = 10,
            ["ValueB"] = 15
        });

        var expression = new Expression("[ValueA] + [ValueB]", context);

        await Assert.That(expression.ToExpressionString(evaluateParameters: true, cancellationToken: CancellationToken.None)).IsEqualTo("10 + 15");
    }

    [Test]
    public async Task ShouldSerializeExpressionWithDynamicParameterValues()
    {
        var expression = new Expression("[ValueA] + [ValueB]")
        {
            Parameters =
            {
                ["ValueA"] = 10
            },
            DynamicParameters =
            {
                ["ValueB"] = _ => 15
            }
        };

        await Assert.That(expression.ToExpressionString(evaluateParameters: true)).IsEqualTo("10 + 15");
    }

    [Test]
    public async Task ShouldKeepUnknownParametersWhenSerializingWithParameterValues()
    {
        var expression = new Expression("[ValueA] + [ValueB]")
        {
            Parameters =
            {
                ["ValueA"] = 10
            }
        };

        await Assert.That(expression.ToExpressionString(evaluateParameters: true)).IsEqualTo("10 + [ValueB]");
    }

    [Test]
    public async Task ShouldSerializePreciseDecimalNumber()
    {
        await Assert.That(new ValueExpression(0.00001).ToExpressionString()).IsEqualTo("1E-05");
    }
}
