using System.Threading.Tasks;
using NCalc.Helpers;

namespace NCalc.Tests;

[Property("Category", "Decimals")]
public class DecimalsTests
{
    [Test]
    // It's not possible to specify decimal literals in annotations, so here we use
    // a string to specify the expected results, and parse it to a decimal in the test.
    [Arguments("12.34", "12.34")]
    [Arguments("12345678901234567890123456789",  "12345678901234567890123456789")]
    [Arguments("0.1234567890123456789",          "0.1234567890123456789")]
    [Arguments("10000000000000000.12",           "10000000000000000.12")]
    [Arguments("100000000000000000000000000.12", "100000000000000000000000000.12")]
    [Arguments("123456789012345678901234567.89", "123456789012345678901234567.89")]
    [Arguments("1234567890.1234567890123456789", "1234567890.1234567890123456789")]
    public async Task Should_Correctly_Parse_Large_Decimal_Literals_Issue_335(String expressionStr, String expectedDecimalResultStr)
    {
        // https://github.com/ncalc/ncalc/issues/335
        var expectedDecimalResult = decimal.Parse(expectedDecimalResultStr, CultureInfo.InvariantCulture);
        // Ensure that decimal.Parse is working as presumed
        await Assert.That(expectedDecimalResultStr).IsEqualTo(expectedDecimalResult.ToString(CultureInfo.InvariantCulture));

        var expression = new Expression(expressionStr, ExpressionOptions.DecimalAsDefault);
        var result = expression.Evaluate(CancellationToken.None);
        await Assert.That(result).IsTypeOf<decimal>();
        await Assert.That(result).IsEqualTo(expectedDecimalResult);
    }

    [Test]
    public async Task Should_Return_PositiveInfinity_When_Overflow_Issue_335()
    {
        // https://github.com/ncalc/ncalc/issues/335
        var expression = new Expression("8E28", ExpressionOptions.DecimalAsDefault);
        var result = expression.Evaluate(CancellationToken.None);
        await Assert.That(result).IsTypeOf<double>();
        await Assert.That(result).IsEqualTo(double.PositiveInfinity);
    }

    [Test]
    public async Task Should_Return_NegativeInfinity_When_Overflow_Issue_335()
    {
        // https://github.com/ncalc/ncalc/issues/335
        var expression = new Expression("-8E28", ExpressionOptions.DecimalAsDefault);
        var result = expression.Evaluate(CancellationToken.None);
        await Assert.That(result).IsTypeOf<double>();
        await Assert.That(result).IsEqualTo(double.NegativeInfinity);
    }

    [Test]
    public async Task Decimals_Should_Not_Loose_Precision()
    {
        var expression = new Expression("0.3 - 0.2 - 0.1", ExpressionOptions.DecimalAsDefault);

        var result = expression.Evaluate(CancellationToken.None);
        await Assert.That(result).IsEqualTo(0M);
    }

    [Test]
    public async Task ShouldAddDoubleAndDecimal()
    {
        var e = new Expression("1.8 + Abs([var1])")
        {
            Parameters =
            {
                ["var1"] = 9.2
            }
        };

        await Assert.That(e.Evaluate(CancellationToken.None)).IsEqualTo(11d);
    }

    [Test]
    public async Task ShouldSubtractDoubleAndDecimal()
    {
        var e = new Expression("1.8 - Abs([var1])")
        {
            Parameters =
            {
                ["var1"] = 0.8
            }
        };

        await Assert.That(e.Evaluate(CancellationToken.None)).IsEqualTo(1d);
    }

    [Test]
    public async Task ShouldMultiplyDoubleAndDecimal()
    {
        var e = new Expression("1.8 * Abs([var1])")
        {
            Parameters =
            {
                ["var1"] = 9.2
            }
        };

        await Assert.That(e.Evaluate(CancellationToken.None)).IsEqualTo(16.56);
    }

    [Test]
    public async Task ShouldDivideDoubleAndDecimal()
    {
        var e = new Expression("1.8 / Abs([var1])")
        {
            Parameters =
            {
                ["var1"] = 0.5
            }
        };

        await Assert.That(e.Evaluate(CancellationToken.None)).IsEqualTo(3.6d);
    }

    [Test]
    public async Task Should_Divide_Decimal_By_Double_Issue_16()
    {
        // https://github.com/ncalc/ncalc/issues/16

        var e = new Expression("x / 1.0")
        {
            Parameters =
            {
                ["x"] = 1m
            }
        };

        await Assert.That(e.Evaluate(CancellationToken.None)).IsEqualTo(1m);
    }

    [Test]
    public async Task Should_Divide_Decimal_By_Single()
    {
        var e = new Expression("x / y")
        {
            Parameters =
            {
                ["x"] = 1m,
                ["y"] = 1f
            }
        };

        await Assert.That(e.Evaluate(CancellationToken.None)).IsEqualTo(1m);
    }

    [Test]
    public async Task ShouldHandleTrailingDecimalPoint()
    {
        await Assert.That(new Expression("1. + 2.").Evaluate(CancellationToken.None)).IsEqualTo(3.0);
    }

    [Test]
    public async Task ShouldNotLoosePrecision()
    {
        await Assert.That(new Expression("3/6").Evaluate(CancellationToken.None)).IsEqualTo(0.5);
    }

    [Test]
    public async Task ShouldNotRoundDecimalValues()
    {
        await Assert.That(new Expression("0 <= -0.6").Evaluate<bool>(CancellationToken.None)).IsFalse();
    }

    [Test]
    public async Task ShouldAllowPowWithDecimals()
    {
        var e = new Expression("3.1 ** 2", ExpressionOptions.DecimalAsDefault);
        await Assert.That(e.Evaluate(CancellationToken.None)).IsEqualTo(9.61m);

        var e2 = new Expression("Pow(3.1, 2)", ExpressionOptions.DecimalAsDefault);
        await Assert.That(e2.Evaluate(CancellationToken.None)).IsEqualTo(9.61m);
    }

    [Test]
    public async Task ShouldUseMathNumberTypesWhenCoercingStringNumbers()
    {
        var cultureInfo = CultureInfo.InvariantCulture;

        await Assert.That(MathHelper.Add("2", "3", new MathOptions(), cultureInfo)).IsEqualTo(5d);
        await Assert.That(MathHelper.Add("2", "3", new MathOptions
        {
            FloatingPointNumberType = FloatingPointNumberType.Decimal
        }, cultureInfo)).IsEqualTo(5m);
        await Assert.That(MathHelper.Add("2", "3", new MathOptions
        {
            IntegerNumberType = IntegerNumberType.Int64
        }, cultureInfo)).IsEqualTo(5L);
    }

    [Test]
    public async Task ShouldUseFloatingPointNumberTypeWhenChoosingMathFunctionPrecision()
    {
        var doubleResult = MathHelper.Abs("-1", new MathOptions(), CultureInfo.InvariantCulture);
        var decimalResult = MathHelper.Abs("-1", new MathOptions
        {
            FloatingPointNumberType = FloatingPointNumberType.Decimal
        }, CultureInfo.InvariantCulture);

        await Assert.That(doubleResult).IsEqualTo(1d);
        await Assert.That(decimalResult).IsEqualTo(1m);
    }

    [Test]
    public async Task ShouldResolveHexadecimal()
    {
        await Assert.That(new Expression("0x17 + 0x18").Evaluate(CancellationToken.None)).IsEqualTo(0x2f);
    }

    [Test]
    public async Task ShouldResolveOctal()
    {
        await Assert.That(new Expression("0o16 + 0o17").Evaluate(CancellationToken.None)).IsEqualTo(29);
    }

    [Test]
    public async Task ShouldResolveBinary()
    {
        await Assert.That(new Expression("0b00001111 + 0b11110000").Evaluate(CancellationToken.None)).IsEqualTo(255);
        await Assert.That(new Expression("0b00001111 & 0b11110000").Evaluate(CancellationToken.None)).IsEqualTo(0UL);
        await Assert.That(new Expression("0b00001111 | 0b11110000").Evaluate(CancellationToken.None)).IsEqualTo(255UL);
    }

    [Test]
    [Arguments("1")]
    [Arguments("1+2")]
    [Arguments("4-3")]
    [Arguments("1*2")]
    public async Task ShouldNotTreatIntegersAsDecimals(string expr)
    {
        var expression = new Expression(expr, ExpressionOptions.DecimalAsDefault);
        var res = expression.Evaluate(CancellationToken.None);
        await Assert.That(res.GetType()).IsEqualTo(typeof(int));
    }

    [Test]
    public async Task ShouldParseBigNumbersAsDecimals()
    {
        var expr = new Expression("25343463636363454545454544563464.12", ExpressionOptions.DecimalAsDefault, null,CultureInfo.InvariantCulture);
        var res = expr.Evaluate(CancellationToken.None);
        await Assert.That(res).IsEqualTo(double.PositiveInfinity);
    }

    [Test]
    [Arguments("Pow(2, 10)", "1024")]
    [Arguments("Pow(1.5, 2)", "2.25")]
    [Arguments("Pow(-2, 3)", "-8")]
    [Arguments("Pow(3, 0)", "1")]
    [Arguments("Pow(2, -2)", "0.25")]
    [Arguments("Pow(1.5, -1)", "0.6666666666666666666666666667")]
    [Arguments("Pow(0.5, -95)", "39614081257132168796771975168")]
    [Arguments("Pow(1.1, 30)", "17.449402268886407318558803754")]
    [Arguments("Pow(0.5, 100)", "0")]
    [Arguments("1.1 ** 2", "1.21")]
    public async Task ShouldPowDecimalsWithIntegralExponents(string expression, string expected)
    {
        var e = new Expression(expression, ExpressionOptions.DecimalAsDefault, null, CultureInfo.InvariantCulture);
        await Assert.That(e.Evaluate(CancellationToken.None)).IsEqualTo(decimal.Parse(expected, CultureInfo.InvariantCulture));
    }

    [Test]
    [Timeout(5_000)]
    [Arguments("Pow(0.5, 2147483647)")]
    [Arguments("Pow(0.5, 100000000000000000000)")]
    [Arguments("Pow(-1, 100000000000000000001)")]
    public async Task ShouldPowDecimalsWithHugeExponentsQuickly(string expression, CancellationToken cancellationToken)
    {
        // The exact power has billions of digits; computing it before rounding to a decimal held a thread for hours.
        var e = new Expression(expression, ExpressionOptions.DecimalAsDefault, null, CultureInfo.InvariantCulture);
        var expected = expression.StartsWith("Pow(-1") ? -1m : 0m;
        await Assert.That(e.Evaluate(cancellationToken)).IsEqualTo(expected);
    }

    [Test]
    [Arguments("Pow(2, 0.5)", 2d, 0.5d)]
    [Arguments("Pow(1.05, 2.5)", 1.05d, 2.5d)]
    [Arguments("Pow(9, -0.5)", 9d, -0.5d)]
    public async Task ShouldPowDecimalsWithFractionalExponents(string expression, double @base, double exponent)
    {
        // The fraction used to be truncated away, so Pow(1.05, 2.5) was 1.05 squared.
        var e = new Expression(expression, ExpressionOptions.DecimalAsDefault, null, CultureInfo.InvariantCulture);
        await Assert.That(e.Evaluate(CancellationToken.None)).IsEqualTo((decimal)Math.Pow(@base, exponent));
    }

    [Test]
    public async Task ShouldThrowWhenADecimalPowLeavesTheDecimalRange()
    {
        var e = new Expression("Pow(10, 30)", ExpressionOptions.DecimalAsDefault, null, CultureInfo.InvariantCulture);
        await Assert.That(() => e.Evaluate(CancellationToken.None)).Throws<OverflowException>();
    }

    [Test]
    public async Task ShouldThrowWhenRaisingDecimalZeroToANegativePower()
    {
        var e = new Expression("Pow(0, -2)", ExpressionOptions.DecimalAsDefault, null, CultureInfo.InvariantCulture);
        await Assert.That(() => e.Evaluate(CancellationToken.None)).Throws<ArgumentException>();
    }
}
