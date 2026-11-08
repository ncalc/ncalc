using System.Numerics;
using System.Text.Json;
using NCalc.Helpers;
using NCalc.LambdaCompilation;

namespace NCalc.Tests;

[Property("Category", "BigInteger")]
public class BigIntegerTests
{
    private static ExpressionConfiguration Configuration(bool cacheEnabled = false) => new()
    {
        Parsing = new LogicalExpressionParserOptions { IntegerNumberType = IntegerNumberType.BigInteger },
        Evaluation = new ExpressionEvaluationOptions
        {
            Math = new MathOptions { IntegerNumberType = IntegerNumberType.BigInteger }
        },
        CacheEnabled = cacheEnabled
    };

    private static Expression Create(string text) => new(text, Configuration(), cultureInfo: CultureInfo.InvariantCulture);

    [Test]
    [Arguments("0")]
    [Arguments("-0")]
    [Arguments("+1")]
    [Arguments("+999999999999999999999999999999999999")]
    [Arguments("2147483647")]
    [Arguments("2147483648")]
    [Arguments("-2147483649")]
    [Arguments("9223372036854775807")]
    [Arguments("9223372036854775808")]
    [Arguments("-9223372036854775809")]
    [Arguments("18446744073709551615")]
    [Arguments("12345678901234567890123456789012")]
    [Arguments("999999999999999999999999999999999999999999999999999999999999999999")]
    public async Task ShouldParseWholeLiteralsExactly(string text)
    {
        var result = Create(text).Evaluate();
        await Assert.That(result).IsTypeOf<BigInteger>();
        await Assert.That((BigInteger)result).IsEqualTo(BigInteger.Parse(text, CultureInfo.InvariantCulture));
    }

    [Test]
    public async Task ShouldMultiplyReportedValuesAndSuppliedParametersExactly()
    {
        const string first = "99999999999999999999999999";
        const string second = "888888888888888888888888888888888888888";
        var a = BigInteger.Parse(first, CultureInfo.InvariantCulture);
        var b = BigInteger.Parse(second, CultureInfo.InvariantCulture);
        await Assert.That(Create(first + "*" + second).Evaluate()).IsEqualTo(a * b);
        var supplied = new Expression("a*b", new ExpressionConfiguration { CacheEnabled = false })
        {
            Parameters = { ["a"] = a, ["b"] = b }
        };
        await Assert.That(supplied.Evaluate()).IsEqualTo(a * b);
        await Assert.That(await supplied.EvaluateAsync()).IsEqualTo(a * b);
    }

    [Test]
    public async Task ShouldPreserveIntegralFunctionsAndParameterText()
    {
        var large = (BigInteger.One << 160) + 3;
        var expression = Create("Abs(a)+Max(a,1)-Min(a,1)");
        expression.Parameters["a"] = -large;
        await Assert.That(expression.Evaluate()).IsEqualTo(large + 1 + large);
        var sign = Create("Sign(a)");
        sign.Parameters["a"] = -large;
        await Assert.That(sign.Evaluate()).IsEqualTo(-1);
        var parameter = Create("a+1");
        parameter.Parameters["a"] = large;
        var text = parameter.ToExpressionString(evaluateParameters: true);
        await Assert.That(Create(text).Evaluate()).IsEqualTo(large + 1);
        await Assert.That(await parameter.EvaluateAsync<BigInteger>()).IsEqualTo(large + 1);
    }

    [Test]
    public async Task ShouldKeepBasedIntegerLiteralsExact()
    {
        var expected = (BigInteger.One << 160) + 15;
        await Assert.That(Create("0x" + expected.ToString("x", CultureInfo.InvariantCulture)).Evaluate()).IsEqualTo(expected);
        await Assert.That(Create("0b1" + new string('0', 160)).Evaluate()).IsEqualTo(BigInteger.One << 160);
        await Assert.That(Create("0o1" + new string('0', 60)).Evaluate()).IsEqualTo(BigInteger.One << 180);
        await Assert.That(Create("-0x10000000000000000").Evaluate()).IsEqualTo(-(BigInteger.One << 64));
    }

    [Test]
    public async Task ShouldPromoteAllFixedWidthIntegerParametersWithoutNarrowing()
    {
        object[] smallValues = [byte.MaxValue, sbyte.MinValue, short.MinValue, ushort.MaxValue,
            int.MinValue, uint.MaxValue, long.MinValue, ulong.MaxValue];
        var large = (BigInteger.One << 160) + 19;
        foreach (var small in smallValues)
        {
            var integral = BigInteger.Parse(Convert.ToString(small, CultureInfo.InvariantCulture)!, CultureInfo.InvariantCulture);
            foreach (var reverse in new[] { false, true })
            {
                var a = reverse ? integral : large;
                var b = reverse ? large : integral;
                foreach (var op in new[] { "+", "-", "*", "/", "%" })
                {
                    var expression = Create("a" + op + "b");
                    expression.Parameters["a"] = reverse ? small : large;
                    expression.Parameters["b"] = reverse ? large : small;
                    var expected = op switch
                    {
                        "+" => a + b,
                        "-" => a - b,
                        "*" => a * b,
                        "/" => a / b,
                        _ => a % b
                    };
                    await Assert.That(expression.Evaluate()).IsEqualTo(expected);
                }
                var comparison = Create("a < b");
                comparison.Parameters["a"] = reverse ? small : large;
                comparison.Parameters["b"] = reverse ? large : small;
                await Assert.That(comparison.Evaluate()).IsEqualTo(a < b);
            }
        }
    }

    [Test]
    public async Task ShouldAgreeWithBoundedDeterministicBigIntegerOracle()
    {
        var random = new Random(264);
        for (var i = 0; i < 24; i++)
        {
            var bytes = new byte[24];
            random.NextBytes(bytes);
            var a = new BigInteger(bytes);
            var b = new BigInteger(random.Next(1, 1000)) * (i % 2 == 0 ? 1 : -1);
            var expression = Create("(a+b)*(a-b)");
            expression.Parameters["a"] = a;
            expression.Parameters["b"] = b;
            await Assert.That(expression.Evaluate()).IsEqualTo((a + b) * (a - b));
            await Assert.That(await expression.EvaluateAsync()).IsEqualTo((a + b) * (a - b));
        }
    }

    [Test]
    public async Task ShouldUseIntegerDivisionRemainderAndUnaryNegation()
    {
        await Assert.That(Create("-7/3").Evaluate()).IsEqualTo(new BigInteger(-2));
        await Assert.That(Create("-7%3").Evaluate()).IsEqualTo(new BigInteger(-1));
        await Assert.That(Create("-(-9223372036854775808)").Evaluate()).IsEqualTo(BigInteger.One << 63);
        Assert.Throws<DivideByZeroException>(() => Create("1/0").Evaluate());
        Assert.Throws<DivideByZeroException>(() => Create("1%0").Evaluate());
    }

    [Test]
    public async Task ShouldCompareBeyondFloatingPointPrecision()
    {
        await Assert.That((bool)Create("9007199254740993 = 9007199254740992").Evaluate()).IsFalse();
        await Assert.That((bool)Create("18446744073709551616 > 18446744073709551615").Evaluate()).IsTrue();
        await Assert.That((bool)Create("-18446744073709551616 < -18446744073709551615").Evaluate()).IsTrue();
        await Assert.That((bool)Create("9007199254740993 != 9007199254740992").Evaluate()).IsTrue();
    }

    [Test]
    public async Task ShouldRoundTripSmallAndLargeBigIntegerAstValues()
    {
        foreach (var value in new[] { BigInteger.Zero, BigInteger.MinusOne, (BigInteger.One << 256) + 1 })
        {
            LogicalExpression original = new ValueExpression(value);
            var json = JsonSerializer.Serialize(original, NCalcTestJsonContext.Default.LogicalExpression);
            var restored = (ValueExpression)JsonSerializer.Deserialize(json, NCalcTestJsonContext.Default.LogicalExpression)!;
            await Assert.That(restored.Type).IsEqualTo(ValueType.BigInteger);
            await Assert.That(restored.Value).IsTypeOf<BigInteger>();
            await Assert.That(restored.Value).IsEqualTo(value);
            var text = original.ToExpressionString();
            await Assert.That(Create(text).Evaluate()).IsEqualTo(value);
        }
        var binary = Create("99999999999999999999999999999999+1").GetLogicalExpression();
        var serialized = JsonSerializer.Serialize(binary, NCalcTestJsonContext.Default.LogicalExpression);
        var roundTrip = JsonSerializer.Deserialize(serialized, NCalcTestJsonContext.Default.LogicalExpression);
        await Assert.That(new Expression(roundTrip!, Configuration()).Evaluate())
            .IsEqualTo(BigInteger.Parse("100000000000000000000000000000000", CultureInfo.InvariantCulture));
    }

    [Test]
    public async Task ShouldSeparateDefaultAndBigIntegerCachedTrees()
    {
        foreach (var bigFirst in new[] { false, true })
        {
            var text = bigFirst ? "999999999999999999999999999999917" : "999999999999999999999999999999927";
            var big = new Expression(text, Configuration(cacheEnabled: true));
            var normal = new Expression(text);
            if (bigFirst)
            {
                await Assert.That(big.Evaluate()).IsTypeOf<BigInteger>();
                await Assert.That(normal.Evaluate()).IsTypeOf<double>();
            }
            else
            {
                await Assert.That(normal.Evaluate()).IsTypeOf<double>();
                await Assert.That(big.Evaluate()).IsTypeOf<BigInteger>();
            }
        }
    }

    [Test]
    public async Task ShouldNotShareRawCacheKeySpaceWithOptInParsing()
    {
        const string text = "998877665544332211009988776655443399";
        await Assert.That(new Expression(text, Configuration(cacheEnabled: true)).Evaluate()).IsTypeOf<BigInteger>();
        var formerKey = string.Join("\0", "", nameof(IntegerNumberType.BigInteger),
            FloatingPointNumberType.Double, false, false, ArgumentSeparator.Comma, CultureInfo.CurrentCulture.Name, text);
        // Raw user text must be parsed as user text, never resolved as another mode's cache key.
        await Assert.That(new Expression(formerKey).HasErrors()).IsTrue();
        var first = new Expression("273819+2");
        var second = new Expression("273819+2");
        await Assert.That(ReferenceEquals(first.GetLogicalExpression(), second.GetLogicalExpression())).IsTrue();
    }

    [Test]
    public async Task ShouldKeepCustomDateCultureCacheOnAndOffEquivalent()
    {
        foreach (var cacheEnabled in new[] { false, true })
        {
            foreach (var dayFirst in new[] { false, true })
            {
                var culture = (CultureInfo)CultureInfo.GetCultureInfo("en-US").Clone();
                culture.DateTimeFormat.ShortDatePattern = dayFirst ? "dd/MM/yyyy" : "MM/dd/yyyy";
                var expression = new Expression("#01/02/2031#", Configuration(cacheEnabled), cultureInfo: culture);
                var expected = dayFirst ? new DateTime(2031, 2, 1) : new DateTime(2031, 1, 2);
                await Assert.That(expression.Evaluate()).IsEqualTo(expected);
            }
        }
    }

    [Test]
    public async Task ShouldKeepParserOptionsCacheOnAndOffEquivalent()
    {
        foreach (var cacheEnabled in new[] { false, true })
        {
            foreach (var decimalNumbers in new[] { false, true })
            {
                var configuration = new ExpressionConfiguration
                {
                    Parsing = new LogicalExpressionParserOptions
                    {
                        IntegerNumberType = IntegerNumberType.BigInteger,
                        FloatingPointNumberType = decimalNumbers
                            ? FloatingPointNumberType.Decimal : FloatingPointNumberType.Double
                    },
                    CacheEnabled = cacheEnabled
                };
                var expression = new Expression("1.25", configuration, cultureInfo: CultureInfo.InvariantCulture);
                if (decimalNumbers)
                    await Assert.That(expression.Evaluate()).IsEqualTo(1.25m);
                else
                    await Assert.That(expression.Evaluate()).IsEqualTo(1.25d);
            }
        }
    }

    [Test]
    public async Task ShouldPreserveDefaultAndExplicitRealNumberBehavior()
    {
        await Assert.That(new Expression("1+2").Evaluate()).IsTypeOf<int>();
        await Assert.That(new Expression("1/2").Evaluate()).IsEqualTo(0.5);
        await Assert.That(new Expression("2147483647+1").Evaluate()).IsEqualTo(int.MinValue);
        await Assert.That(Create("1.5+2.5").Evaluate()).IsEqualTo(4d);
        await Assert.That(Create("1e30").Evaluate()).IsTypeOf<double>();
        var decimalOptions = new ExpressionConfiguration
        {
            Parsing = new LogicalExpressionParserOptions
            {
                IntegerNumberType = IntegerNumberType.BigInteger,
                FloatingPointNumberType = FloatingPointNumberType.Decimal
            },
            Evaluation = new ExpressionEvaluationOptions
            {
                Math = new MathOptions
                {
                    IntegerNumberType = IntegerNumberType.BigInteger,
                    FloatingPointNumberType = FloatingPointNumberType.Decimal
                }
            },
            CacheEnabled = false
        };
        await Assert.That(new Expression("1.5+2.5", decimalOptions).Evaluate()).IsEqualTo(4m);
    }

    [Test]
    public async Task ShouldKeepBooleanAndStringOptionsExplicit()
    {
        Assert.Throws<InvalidOperationException>(() => Create("true+1").Evaluate());
        var configuration = new ExpressionConfiguration
        {
            Parsing = Configuration().Parsing,
            Evaluation = new ExpressionEvaluationOptions
            {
                Math = new MathOptions { IntegerNumberType = IntegerNumberType.BigInteger, AllowBooleanCalculation = true }
            },
            CacheEnabled = false
        };
        await Assert.That(new Expression("true+1", configuration).Evaluate()).IsEqualTo(new BigInteger(2));
        await Assert.That(Create("'9007199254740993'+1").Evaluate()).IsEqualTo(BigInteger.Parse("9007199254740994", CultureInfo.InvariantCulture));
        var concatenate = new ExpressionConfiguration
        {
            Parsing = Configuration().Parsing,
            Evaluation = new ExpressionEvaluationOptions { StringConcat = true },
            CacheEnabled = false
        };
        await Assert.That(new Expression("9007199254740993+'x'", concatenate).Evaluate()).IsEqualTo("9007199254740993x");
    }

    [Test]
    public async Task ShouldKeepOverflowProtectionAndMathOnlyOptInExplicit()
    {
        var normalChecked = new ExpressionConfiguration
        {
            Evaluation = new ExpressionEvaluationOptions { Math = new MathOptions { OverflowProtection = true } },
            CacheEnabled = false
        };
        Assert.Throws<OverflowException>(() => new Expression("2147483647+1", normalChecked).Evaluate());
        var bigChecked = new ExpressionConfiguration
        {
            Parsing = Configuration().Parsing,
            Evaluation = new ExpressionEvaluationOptions
            {
                Math = new MathOptions { IntegerNumberType = IntegerNumberType.BigInteger, OverflowProtection = true }
            },
            CacheEnabled = false
        };
        await Assert.That(new Expression("2147483647+1", bigChecked).Evaluate()).IsEqualTo(new BigInteger(2147483648L));
        var mathOnly = new ExpressionConfiguration
        {
            Evaluation = bigChecked.Evaluation,
            CacheEnabled = false
        };
        var parameters = new Expression("a+b", mathOnly) { Parameters = { ["a"] = 1, ["b"] = 2 } };
        await Assert.That(parameters.Evaluate()).IsEqualTo(new BigInteger(3));
    }

    [Test]
    public async Task ShouldRespectNoStringCoercionAndStrictTypeMatching()
    {
        var configuration = new ExpressionConfiguration
        {
            Parsing = Configuration().Parsing,
            Evaluation = new ExpressionEvaluationOptions { NoStringTypeCoercion = true, StrictTypeMatching = true },
            CacheEnabled = false
        };
        await Assert.That(new Expression("'1'+2", configuration).Evaluate()).IsEqualTo("12");
        var comparison = new Expression("1 = a", configuration) { Parameters = { ["a"] = 1 } };
        await Assert.That((bool)comparison.Evaluate()).IsFalse();
        var ordinary = Create("1 = a");
        ordinary.Parameters["a"] = 1;
        await Assert.That((bool)ordinary.Evaluate()).IsTrue();
    }

    [Test]
    public async Task ShouldRecognizeOnlySupportedIntegralTypePromotion()
    {
        await Assert.That(TypeHelper.GetMostPreciseNumberType(typeof(BigInteger), typeof(ulong)))
            .IsEqualTo(typeof(BigInteger));
        await Assert.That(TypeHelper.GetMostPreciseNumberType(typeof(int), typeof(BigInteger)))
            .IsEqualTo(typeof(BigInteger));
        await Assert.That(TypeHelper.GetMostPreciseNumberType(typeof(object), typeof(BigInteger))).IsNull();
        await Assert.That(TypeHelper.GetMostPreciseNumberType(typeof(double), typeof(BigInteger))).IsNull();
        var expression = Create("a+1");
        expression.Parameters["a"] = new object();
        Assert.Throws<InvalidOperationException>(() => expression.Evaluate());
    }

    [Test]
    public void ShouldRejectImplicitRealConversionsAndLambdaCompilation()
    {
        foreach (var real in new object[] { 1.5f, 1.5d, 1.5m })
        {
            var expression = Create("a+1");
            expression.Parameters["a"] = real;
            Assert.Throws<InvalidOperationException>(() => expression.Evaluate());
        }
        Assert.Throws<NotSupportedException>(() => Create("Sin(1)").Evaluate());
        Assert.Throws<NotSupportedException>(() => Create("Pow(2,3)").Evaluate());
        Assert.Throws<NotSupportedException>(() => Create("1 << 2").Evaluate());
        Assert.Throws<NotSupportedException>(() => Create("~1").Evaluate());
        Assert.Throws<NotSupportedException>(() => Create("1+2").ToLambda<BigInteger>());
        var mathOnly = new Expression("a", new ExpressionConfiguration
        {
            Evaluation = new ExpressionEvaluationOptions
            {
                Math = new MathOptions { IntegerNumberType = IntegerNumberType.BigInteger }
            }
        }) { Parameters = { ["a"] = BigInteger.One } };
        Assert.Throws<NotSupportedException>(() => mathOnly.ToLambda<BigInteger>());
    }
}
