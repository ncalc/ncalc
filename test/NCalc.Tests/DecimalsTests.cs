namespace NCalc.Tests;

[Property("Category", "Decimals")]
public class DecimalsTests
{
    [Test]
    // It's not possible to specify decimal literals in annotations, so here we use
    // a string to specify the expected results, and parse it to a decimal in the test.
    [Arguments("12.34", "12.34")]
    [Arguments("12345678901234567890123456789", "12345678901234567890123456789")]
    [Arguments("0.1234567890123456789", "0.1234567890123456789")]
    [Arguments("10000000000000000.12", "10000000000000000.12")]
    [Arguments("100000000000000000000000000.12", "100000000000000000000000000.12")]
    [Arguments("123456789012345678901234567.89", "123456789012345678901234567.89")]
    [Arguments("1234567890.1234567890123456789", "1234567890.1234567890123456789")]
    public async Task Should_Correctly_Parse_Large_Decimal_Literals_Issue_335(string expressionStr, string expectedDecimalResultStr, CancellationToken cancellationToken)
    {
        // https://github.com/ncalc/ncalc/issues/335
        var expectedDecimalResult = decimal.Parse(expectedDecimalResultStr, CultureInfo.InvariantCulture);
        // Ensure that decimal.Parse is working as presumed
        await Assert.That(expectedDecimalResultStr).IsEqualTo(expectedDecimalResult.ToString(CultureInfo.InvariantCulture));

        var expression = new Expression(expressionStr, ExpressionOptions.DecimalAsDefault);
        var result = expression.Evaluate(cancellationToken);
        await Assert.That(result).IsTypeOf<decimal>();
        await Assert.That(result).IsEqualTo(expectedDecimalResult);
    }

    [Test]
    public async Task Should_Return_PositiveInfinity_When_Overflow_Issue_335(CancellationToken cancellationToken)
    {
        // https://github.com/ncalc/ncalc/issues/335
        var expression = new Expression("8E28", ExpressionOptions.DecimalAsDefault);
        var result = expression.Evaluate(cancellationToken);
        await Assert.That(result).IsTypeOf<double>();
        await Assert.That(result).IsEqualTo(double.PositiveInfinity);
    }

    [Test]
    public async Task Should_Return_NegativeInfinity_When_Overflow_Issue_335(CancellationToken cancellationToken)
    {
        // https://github.com/ncalc/ncalc/issues/335
        var expression = new Expression("-8E28", ExpressionOptions.DecimalAsDefault);
        var result = expression.Evaluate(cancellationToken);
        await Assert.That(result).IsTypeOf<double>();
        await Assert.That(result).IsEqualTo(double.NegativeInfinity);
    }

    [Test]
    public async Task Decimals_Should_Not_Loose_Precision(CancellationToken cancellationToken)
    {
        var expression = new Expression("0.3 - 0.2 - 0.1", ExpressionOptions.DecimalAsDefault);

        var result = expression.Evaluate(cancellationToken);
        await Assert.That(result).IsEqualTo(0M);
    }

    [Test]
    public async Task ShouldAddDoubleAndDecimal(CancellationToken cancellationToken)
    {
        var e = new Expression("1.8 + Abs([var1])");
        e.Parameters["var1"] = 9.2;

        await Assert.That(e.Evaluate(cancellationToken)).IsEqualTo(11d);
    }

    [Test]
    public async Task ShouldSubtractDoubleAndDecimal(CancellationToken cancellationToken)
    {
        var e = new Expression("1.8 - Abs([var1])");
        e.Parameters["var1"] = 0.8;

        await Assert.That(e.Evaluate(cancellationToken)).IsEqualTo(1d);
    }

    [Test]
    public async Task ShouldMultiplyDoubleAndDecimal(CancellationToken cancellationToken)
    {
        var e = new Expression("1.8 * Abs([var1])");
        e.Parameters["var1"] = 9.2;

        await Assert.That(e.Evaluate(cancellationToken)).IsEqualTo(16.56);
    }

    [Test]
    public async Task ShouldDivideDoubleAndDecimal(CancellationToken cancellationToken)
    {
        var e = new Expression("1.8 / Abs([var1])");
        e.Parameters["var1"] = 0.5;

        await Assert.That(e.Evaluate(cancellationToken)).IsEqualTo(3.6d);
    }

    [Test]
    public async Task Should_Divide_Decimal_By_Double_Issue_16(CancellationToken cancellationToken)
    {
        // https://github.com/ncalc/ncalc/issues/16

        var e = new Expression("x / 1.0");
        e.Parameters["x"] = 1m;

        await Assert.That(e.Evaluate(cancellationToken)).IsEqualTo(1m);
    }

    [Test]
    public async Task Should_Divide_Decimal_By_Single(CancellationToken cancellationToken)
    {
        var e = new Expression("x / y");
        e.Parameters["x"] = 1m;
        e.Parameters["y"] = 1f;

        await Assert.That(e.Evaluate(cancellationToken)).IsEqualTo(1m);
    }

    [Test]
    public async Task ShouldHandleTrailingDecimalPoint(CancellationToken cancellationToken)
    {
        await Assert.That(new Expression("1. + 2.").Evaluate(cancellationToken)).IsEqualTo(3.0);
    }

    [Test]
    public async Task ShouldNotLoosePrecision(CancellationToken cancellationToken)
    {
        await Assert.That(new Expression("3/6").Evaluate(cancellationToken)).IsEqualTo(0.5);
    }

    [Test]
    public async Task ShouldNotRoundDecimalValues(CancellationToken cancellationToken)
    {
        await Assert.That(new Expression("0 <= -0.6").Evaluate(cancellationToken)).IsEqualTo(false);
    }

    [Test]
    public async Task ShouldAllowPowWithDecimals(CancellationToken cancellationToken)
    {
        var e = new Expression("3.1 ** 2", ExpressionOptions.DecimalAsDefault);
        await Assert.That(e.Evaluate(cancellationToken)).IsEqualTo(9.61m);

        var e2 = new Expression("Pow(3.1, 2)", ExpressionOptions.DecimalAsDefault);
        await Assert.That(e2.Evaluate(cancellationToken)).IsEqualTo(9.61m);
    }

    [Test]
    public async Task ShouldResolveHexadecimal(CancellationToken cancellationToken)
    {
        await Assert.That(new Expression("0x17 + 0x18").Evaluate(cancellationToken)).IsEqualTo(0x2f);
    }

    [Test]
    public async Task ShouldResolveOctal(CancellationToken cancellationToken)
    {
        await Assert.That(new Expression("0o16 + 0o17").Evaluate(cancellationToken)).IsEqualTo(29);
    }

    [Test]
    public async Task ShouldResolveBinary(CancellationToken cancellationToken)
    {
        await Assert.That(new Expression("0b00001111 + 0b11110000").Evaluate(cancellationToken)).IsEqualTo(255);
        await Assert.That(new Expression("0b00001111 & 0b11110000").Evaluate(cancellationToken)).IsEqualTo(0UL);
        await Assert.That(new Expression("0b00001111 | 0b11110000").Evaluate(cancellationToken)).IsEqualTo(255UL);
    }

    [Test]
    [Arguments("1")]
    [Arguments("1+2")]
    [Arguments("4-3")]
    [Arguments("1*2")]
    public async Task ShouldNotTreatIntegersAsDecimals(string expr, CancellationToken cancellationToken)
    {
        var expression = new Expression(expr, ExpressionOptions.DecimalAsDefault);
        var res = expression.Evaluate(cancellationToken);
        await Assert.That(res.GetType()).IsEqualTo(typeof(int));
    }

    [Test]
    public async Task ShouldParseBigNumbersAsDecimals(CancellationToken cancellationToken)
    {
        var expr = new Expression("25343463636363454545454544563464.12", ExpressionOptions.DecimalAsDefault, CultureInfo.InvariantCulture);
        var res = expr.Evaluate(cancellationToken);
        await Assert.That(res).IsEqualTo(double.PositiveInfinity);
    }
}