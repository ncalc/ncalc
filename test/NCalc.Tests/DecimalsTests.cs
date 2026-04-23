using System.Threading.Tasks;

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
        var e = new Expression("1.8 + Abs([var1])");
        e.Parameters["var1"] = 9.2;

        await Assert.That(e.Evaluate(CancellationToken.None)).IsEqualTo(11d);
    }

    [Test]
    public async Task ShouldSubtractDoubleAndDecimal()
    {
        var e = new Expression("1.8 - Abs([var1])");
        e.Parameters["var1"] = 0.8;

        await Assert.That(e.Evaluate(CancellationToken.None)).IsEqualTo(1d);
    }

    [Test]
    public async Task ShouldMultiplyDoubleAndDecimal()
    {
        var e = new Expression("1.8 * Abs([var1])");
        e.Parameters["var1"] = 9.2;

        await Assert.That(e.Evaluate(CancellationToken.None)).IsEqualTo(16.56);
    }

    [Test]
    public async Task ShouldDivideDoubleAndDecimal()
    {
        var e = new Expression("1.8 / Abs([var1])");
        e.Parameters["var1"] = 0.5;

        await Assert.That(e.Evaluate(CancellationToken.None)).IsEqualTo(3.6d);
    }

    [Test]
    public async Task Should_Divide_Decimal_By_Double_Issue_16()
    {
        // https://github.com/ncalc/ncalc/issues/16

        var e = new Expression("x / 1.0");
        e.Parameters["x"] = 1m;

        await Assert.That(e.Evaluate(CancellationToken.None)).IsEqualTo(1m);
    }

    [Test]
    public async Task Should_Divide_Decimal_By_Single()
    {
        var e = new Expression("x / y");
        e.Parameters["x"] = 1m;
        e.Parameters["y"] = 1f;

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
        await Assert.That(new Expression("0 <= -0.6").Evaluate(CancellationToken.None)).IsEqualTo(false);
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
        var expr = new Expression("25343463636363454545454544563464.12", ExpressionOptions.DecimalAsDefault, CultureInfo.InvariantCulture);
        var res = expr.Evaluate(CancellationToken.None);
        await Assert.That(res).IsEqualTo(double.PositiveInfinity);
    }
}