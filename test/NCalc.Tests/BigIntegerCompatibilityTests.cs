using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using NCalc.Helpers;

namespace NCalc.Tests;

[Property("Category", "BigInteger")]
public class BigIntegerCompatibilityTests
{
    private static ExpressionConfiguration MathConfiguration(bool strictTypes = false) => new()
    {
        CacheEnabled = false,
        Evaluation = new ExpressionEvaluationOptions
        {
            StrictTypeMatching = strictTypes,
            Math = new MathOptions { IntegerNumberType = IntegerNumberType.BigInteger }
        }
    };

    [Test]
    public async Task ShouldNegateDecimalParametersWithoutPromotingAnInternalZero()
    {
        foreach (var value in new[] { 1.5m, -1.5m, 0m, decimal.MinValue, decimal.MaxValue })
        {
            var expression = new Expression("-x", MathConfiguration()) { Parameters = { ["x"] = value } };
            var sync = expression.Evaluate();
            var asyncResult = await expression.EvaluateAsync();
            await Assert.That(sync).IsTypeOf<decimal>();
            await Assert.That(sync).IsEqualTo(-value);
            await Assert.That(asyncResult).IsTypeOf<decimal>();
            await Assert.That(asyncResult).IsEqualTo(-value);
        }
    }

    [Test]
    public async Task ShouldNegateDecimalLiteralsWithBothBigIntegerOptions()
    {
        var configuration = new ExpressionConfiguration
        {
            Parsing = new LogicalExpressionParserOptions
            {
                IntegerNumberType = IntegerNumberType.BigInteger,
                FloatingPointNumberType = FloatingPointNumberType.Decimal
            },
            Evaluation = MathConfiguration().Evaluation,
            CacheEnabled = false
        };
        var expression = new Expression("-1.5", configuration, cultureInfo: CultureInfo.InvariantCulture);
        await Assert.That(expression.Evaluate()).IsEqualTo(-1.5m);
        await Assert.That(await expression.EvaluateAsync()).IsEqualTo(-1.5m);
    }

    [Test]
    public async Task ShouldPromoteFixedWidthAbsParametersExactlyInMathMode()
    {
        object[] values = [byte.MaxValue, sbyte.MinValue, short.MinValue, ushort.MaxValue,
            int.MinValue, uint.MaxValue, long.MinValue, ulong.MaxValue, -9007199254740993L];
        foreach (var value in values)
        {
            var expected = BigInteger.Abs(BigInteger.Parse(Convert.ToString(value, CultureInfo.InvariantCulture)!,
                CultureInfo.InvariantCulture));
            var expression = new Expression("Abs(x)", MathConfiguration()) { Parameters = { ["x"] = value } };
            var sync = expression.Evaluate();
            var asyncResult = await expression.EvaluateAsync();
            await Assert.That(sync).IsTypeOf<BigInteger>();
            await Assert.That(sync).IsEqualTo(expected);
            await Assert.That(asyncResult).IsTypeOf<BigInteger>();
            await Assert.That(asyncResult).IsEqualTo(expected);
        }

        var ordinary = new Expression("Abs(x)") { Parameters = { ["x"] = -9007199254740993L } };
        await Assert.That(ordinary.Evaluate()).IsTypeOf<double>();
        await Assert.That(ordinary.Evaluate()).IsEqualTo(Math.Abs(Convert.ToDouble(-9007199254740993L)));
    }

    [Test]
    public async Task ShouldCompareFixedWidthParametersExactlyInMathMode()
    {
        (object Left, object Right)[] pairs = [(-1L, ulong.MaxValue),
            (9007199254740993L, 9007199254740992UL), (long.MaxValue, ulong.MaxValue)];
        foreach (var pair in pairs)
        {
            foreach (var reverse in new[] { false, true })
            {
                var left = reverse ? pair.Right : pair.Left;
                var right = reverse ? pair.Left : pair.Right;
                var a = BigInteger.Parse(Convert.ToString(left, CultureInfo.InvariantCulture)!, CultureInfo.InvariantCulture);
                var b = BigInteger.Parse(Convert.ToString(right, CultureInfo.InvariantCulture)!, CultureInfo.InvariantCulture);
                foreach (var operation in new[] { "=", "!=", "<", "<=", ">", ">=" })
                {
                    var expected = operation switch
                    {
                        "=" => a == b,
                        "!=" => a != b,
                        "<" => a < b,
                        "<=" => a <= b,
                        ">" => a > b,
                        _ => a >= b
                    };
                    var expression = new Expression("a" + operation + "b", MathConfiguration())
                    {
                        Parameters = { ["a"] = left, ["b"] = right }
                    };
                    await Assert.That(expression.Evaluate()).IsEqualTo(expected);
                    await Assert.That(await expression.EvaluateAsync()).IsEqualTo(expected);
                }
            }
        }
    }

    [Test]
    public async Task ShouldCheckStrictParameterTypesBeforeMathPromotion()
    {
        foreach (var operation in new[] { "=", "!=", "<", "<=", ">", ">=" })
        {
            var expression = new Expression("a" + operation + "b", MathConfiguration(strictTypes: true))
            {
                Parameters = { ["a"] = -1L, ["b"] = ulong.MaxValue }
            };
            await Assert.That(expression.Evaluate()).IsEqualTo(operation == "!=");
            await Assert.That(await expression.EvaluateAsync()).IsEqualTo(operation == "!=");
        }
    }

    [Test]
    public async Task ShouldPromoteIntegralMembershipComparisonsInMathMode()
    {
        var expression = new Expression("a in (b,c)", MathConfiguration())
        {
            Parameters = { ["a"] = -1L, ["b"] = ulong.MaxValue, ["c"] = -1L }
        };
        await Assert.That(expression.Evaluate()).IsEqualTo(true);
        await Assert.That(await expression.EvaluateAsync()).IsEqualTo(true);
        var strict = new Expression("a in (b,c)", new ExpressionConfiguration
        {
            CacheEnabled = false,
            Evaluation = new ExpressionEvaluationOptions
            {
                StrictTypeMatching = true,
                NoStringTypeCoercion = true,
                Math = new MathOptions { IntegerNumberType = IntegerNumberType.BigInteger }
            }
        }) { Parameters = { ["a"] = 1L, ["b"] = 1UL, ["c"] = (byte)1 } };
        await Assert.That(strict.Evaluate()).IsEqualTo(false);
        await Assert.That(await strict.EvaluateAsync()).IsEqualTo(false);
    }

    [Test]
    public async Task ShouldPromoteFunctionFormMembershipInBothEvaluators()
    {
        foreach (var (left, right, expected) in new (object, object, bool)[]
                 { (-1L, ulong.MaxValue, false), (9007199254740993L, 9007199254740992UL, false),
                     (ulong.MaxValue, ulong.MaxValue, true), (-1L, -1L, true) })
        {
            var expression = new Expression("in(a,b,c)", MathConfiguration())
            {
                Parameters = { ["a"] = left, ["b"] = right, ["c"] = right }
            };
            await Assert.That(expression.Evaluate()).IsEqualTo(expected);
            await Assert.That(await expression.EvaluateAsync()).IsEqualTo(expected);
        }

        var strict = new Expression("in(a,b,c)", MathConfiguration(strictTypes: true))
        {
            Parameters = { ["a"] = 1L, ["b"] = 1UL, ["c"] = (byte)1 }
        };
        await Assert.That(strict.Evaluate()).IsEqualTo(false);
        await Assert.That(await strict.EvaluateAsync()).IsEqualTo(false);

        // The existing function-form comparison ignores strict matching outside
        // the opted-in mode; do not change that legacy route as a side effect.
        var ordinary = new Expression("in(a,b,c)", new ExpressionConfiguration
        {
            Evaluation = new ExpressionEvaluationOptions { StrictTypeMatching = true }
        }) { Parameters = { ["a"] = 1L, ["b"] = 1UL, ["c"] = (byte)1 } };
        await Assert.That(ordinary.Evaluate()).IsEqualTo(true);
        await Assert.That(await ordinary.EvaluateAsync()).IsEqualTo(true);
    }

    [Test]
    public async Task ShouldPreserveRegisteredLegacyNullCallbacks()
    {
        var converter = new LegacyNullConverter();
        var options = new JsonSerializerOptions();
        options.Converters.Add(converter);
        var context = new NCalcTestJsonContext(options);
        LogicalExpression original = new ValueExpression((string?)null);
        var json = JsonSerializer.Serialize(original, context.LogicalExpression);
        using var document = JsonDocument.Parse(json);
        await Assert.That(document.RootElement.GetProperty("Value").GetString()).IsEqualTo("written-null");
        await Assert.That(converter.NullWrites).IsEqualTo(1);
        var restored = (ValueExpression)JsonSerializer.Deserialize(
            "{\"$type\":\"value\",\"Value\":null,\"Type\":" + (int)ValueType.String + "}", context.LogicalExpression)!;
        await Assert.That(restored.Value).IsEqualTo("read-null");
        await Assert.That(converter.NullReads).IsEqualTo(1);
        var expression = new Expression(restored);
        await Assert.That(expression.Evaluate()).IsEqualTo("read-null");
        await Assert.That(await expression.EvaluateAsync()).IsEqualTo("read-null");

        var ordinaryContext = new NCalcTestJsonContext();
        var ordinaryJson = JsonSerializer.Serialize(original, ordinaryContext.LogicalExpression);
        var ordinaryRestored = (ValueExpression)JsonSerializer.Deserialize(ordinaryJson, ordinaryContext.LogicalExpression)!;
        await Assert.That(ordinaryRestored.Value).IsNull();
    }

    [Test]
    public async Task ShouldKeepMixedRealComparisonConversionsExplicitInMathMode()
    {
        foreach (var value in new object[] { 1.5f, 1.5d, 1.5m })
        {
            var expression = new Expression("a < b", MathConfiguration())
            {
                Parameters = { ["a"] = 1L, ["b"] = value }
            };
            Assert.Throws<InvalidOperationException>(() => expression.Evaluate());
            await Assert.ThrowsAsync<InvalidOperationException>(async () => { await expression.EvaluateAsync(); });
        }
    }

    [Test]
    public async Task ShouldPreserveRegisteredLegacyObjectJsonConversions()
    {
        var converter = new LegacyNumericStringConverter();
        var options = new JsonSerializerOptions();
        options.Converters.Add(converter);
        var context = new NCalcTestJsonContext(options);
        LogicalExpression original = new ValueExpression(9007199254740993L);
        var json = JsonSerializer.Serialize(original, context.LogicalExpression);
        using var document = JsonDocument.Parse(json);
        await Assert.That(document.RootElement.GetProperty("Value").GetString()).IsEqualTo("9007199254740993");
        var restored = (ValueExpression)JsonSerializer.Deserialize(json, context.LogicalExpression)!;
        await Assert.That(restored.Value).IsTypeOf<long>();
        await Assert.That(restored.Value).IsEqualTo(9007199254740993L);
        await Assert.That(converter.ReadCalls).IsEqualTo(1);
        await Assert.That(converter.WriteCalls).IsEqualTo(1);
        var expression = new Expression(restored);
        await Assert.That(expression.Evaluate()).IsEqualTo(9007199254740993L);
        await Assert.That(await expression.EvaluateAsync()).IsEqualTo(9007199254740993L);
    }

    [Test]
    public async Task ShouldUseTheRegisteredLegacyConverterRegardlessOfPropertyOrder()
    {
        var converter = new LegacyNumericStringConverter();
        var options = new JsonSerializerOptions();
        options.Converters.Add(converter);
        var context = new NCalcTestJsonContext(options);
        foreach (var typeFirst in new[] { false, true })
        {
            var fields = typeFirst
                ? $"\"Type\":{(int)ValueType.Integer},\"Value\":\"17\""
                : $"\"Value\":\"17\",\"Type\":{(int)ValueType.Integer}";
            var restored = (ValueExpression)JsonSerializer.Deserialize("{\"$type\":\"value\"," + fields + "}",
                context.LogicalExpression)!;
            await Assert.That(restored.Value).IsTypeOf<long>();
            await Assert.That(restored.Value).IsEqualTo(17L);
        }
        await Assert.That(converter.ReadCalls).IsEqualTo(2);
    }

    [Test]
    public async Task ShouldKeepBigIntegerJsonExactWithoutApplyingLegacyObjectConversions()
    {
        var converter = new LegacyNumericStringConverter();
        var options = new JsonSerializerOptions();
        options.Converters.Add(converter);
        var context = new NCalcTestJsonContext(options);
        foreach (var value in new[] { BigInteger.Zero, BigInteger.MinusOne, (BigInteger.One << 256) + 1 })
        {
            LogicalExpression original = new ValueExpression(value);
            var json = JsonSerializer.Serialize(original, context.LogicalExpression);
            using var document = JsonDocument.Parse(json);
            await Assert.That(document.RootElement.GetProperty("Value").ValueKind).IsEqualTo(JsonValueKind.Number);
            await Assert.That(document.RootElement.GetProperty("Value").GetRawText())
                .IsEqualTo(value.ToString(CultureInfo.InvariantCulture));
            var restored = (ValueExpression)JsonSerializer.Deserialize(json, context.LogicalExpression)!;
            await Assert.That(restored.Value).IsTypeOf<BigInteger>();
            await Assert.That(restored.Value).IsEqualTo(value);
            await Assert.That(restored.Type).IsEqualTo(ValueType.BigInteger);
            var expression = new Expression(restored);
            await Assert.That(expression.Evaluate()).IsEqualTo(value);
            await Assert.That(await expression.EvaluateAsync()).IsEqualTo(value);
        }
        await Assert.That(converter.ReadCalls).IsEqualTo(0);
        await Assert.That(converter.WriteCalls).IsEqualTo(0);
    }

    private sealed class LegacyNullConverter : JsonConverter<object>
    {
        public override bool HandleNull => true;
        public int NullReads { get; private set; }
        public int NullWrites { get; private set; }

        public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.Null)
                throw new JsonException("The null callback control requires an actual null token.");
            NullReads++;
            return "read-null";
        }

        public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
        {
            if (value is not null)
                throw new JsonException("The null callback control requires an actual null value.");
            NullWrites++;
            writer.WriteStringValue("written-null");
        }
    }

    private sealed class LegacyNumericStringConverter : JsonConverter<object>
    {
        public int ReadCalls { get; private set; }
        public int WriteCalls { get; private set; }

        public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            ReadCalls++;
            // This legacy format deliberately rejects numeric JSON tokens.
            // BigInteger deserialization must not route through this converter.
            return long.Parse(reader.GetString()!, CultureInfo.InvariantCulture);
        }

        public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
        {
            WriteCalls++;
            writer.WriteStringValue(Convert.ToString(value, CultureInfo.InvariantCulture));
        }
    }
}
