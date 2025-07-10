namespace NCalc.Tests;

[Trait("Category", "Decimals")]
public class DecimalsTests
{
    [Theory]
    // It's not possible to specify decimal literals in annotations, so here we use
    // a string to specify the expected results, and parse it to a decimal in the test.
    [InlineData("12.34", "12.34")]
    [InlineData("12345678901234567890123456789",  "12345678901234567890123456789")]
    [InlineData("0.1234567890123456789",          "0.1234567890123456789")]
    [InlineData("10000000000000000.12",           "10000000000000000.12")]
    [InlineData("100000000000000000000000000.12", "100000000000000000000000000.12")]
    [InlineData("123456789012345678901234567.89", "123456789012345678901234567.89")]
    [InlineData("1234567890.1234567890123456789", "1234567890.1234567890123456789")]
    public void Should_Correctly_Parse_Large_Decimal_Literals_Issue_335(String expressionStr, String expectedDecimalResultStr)
    {
        // https://github.com/ncalc/ncalc/issues/335
        var expectedDecimalResult = decimal.Parse(expectedDecimalResultStr, CultureInfo.InvariantCulture);
        // Ensure that decimal.Parse is working as presumed
        Assert.Equal(expectedDecimalResult.ToString(CultureInfo.InvariantCulture), expectedDecimalResultStr);

        var expression = new Expression(expressionStr, ExpressionOptions.DecimalAsDefault);
        var result = expression.Evaluate();
        Assert.IsType<decimal>(result);
        Assert.Equal(expectedDecimalResult, result);
    }

    [Fact]
    public void Should_Return_PositiveInfinity_When_Overflow_Issue_335()
    {
        // https://github.com/ncalc/ncalc/issues/335
        var expression = new Expression("8E28", ExpressionOptions.DecimalAsDefault);
        var result = expression.Evaluate();
        Assert.IsType<double>(result);
        Assert.Equal(double.PositiveInfinity, result);
    }

    [Fact]
    public void Should_Return_NegativeInfinity_When_Overflow_Issue_335()
    {
        // https://github.com/ncalc/ncalc/issues/335
        var expression = new Expression("-8E28", ExpressionOptions.DecimalAsDefault);
        var result = expression.Evaluate();
        Assert.IsType<double>(result);
        Assert.Equal(double.NegativeInfinity, result);
    }

    [Fact]
    public void Decimals_Should_Not_Loose_Precision()
    {
        var expression = new Expression("0.3 - 0.2 - 0.1", ExpressionOptions.DecimalAsDefault);

        var result = expression.Evaluate();
        Assert.Equal(0M, result);
    }

    [Fact]
    public void ShouldAddDoubleAndDecimal()
    {
        var e = new Expression("1.8 + Abs([var1])");
        e.Parameters["var1"] = 9.2;

        Assert.Equal(11d, e.Evaluate());
    }

    [Fact]
    public void ShouldSubtractDoubleAndDecimal()
    {
        var e = new Expression("1.8 - Abs([var1])");
        e.Parameters["var1"] = 0.8;

        Assert.Equal(1d, e.Evaluate());
    }

    [Fact]
    public void ShouldMultiplyDoubleAndDecimal()
    {
        var e = new Expression("1.8 * Abs([var1])");
        e.Parameters["var1"] = 9.2;

        Assert.Equal(16.56, e.Evaluate());
    }

    [Fact]
    public void ShouldDivideDoubleAndDecimal()
    {
        var e = new Expression("1.8 / Abs([var1])");
        e.Parameters["var1"] = 0.5;

        Assert.Equal(3.6d, e.Evaluate());
    }

    [Fact]
    public void Should_Divide_Decimal_By_Double_Issue_16()
    {
        // https://github.com/ncalc/ncalc/issues/16

        var e = new Expression("x / 1.0");
        e.Parameters["x"] = 1m;

        Assert.Equal(1m, e.Evaluate());
    }

    [Fact]
    public void Should_Divide_Decimal_By_Single()
    {
        var e = new Expression("x / y");
        e.Parameters["x"] = 1m;
        e.Parameters["y"] = 1f;

        Assert.Equal(1m, e.Evaluate());
    }

    [Fact]
    public void ShouldHandleTrailingDecimalPoint()
    {
        Assert.Equal(3.0, new Expression("1. + 2.").Evaluate());
    }

    [Fact]
    public void ShouldNotLoosePrecision()
    {
        Assert.Equal(0.5, new Expression("3/6").Evaluate());
    }

    [Fact]
    public void ShouldNotRoundDecimalValues()
    {
        Assert.Equal(false, new Expression("0 <= -0.6").Evaluate());
    }

    [Fact]
    public void ShouldAllowPowWithDecimals()
    {
        var e = new Expression("3.1 ** 2", ExpressionOptions.DecimalAsDefault);
        Assert.Equal(9.61m, e.Evaluate());

        var e2 = new Expression("Pow(3.1, 2)", ExpressionOptions.DecimalAsDefault);
        Assert.Equal(9.61m, e2.Evaluate());
    }

    [Fact]
    public void ShouldResolveHexadecimal()
    {
        Assert.Equal(0x2f, new Expression("0x17 + 0x18").Evaluate());
    }

    [Fact]
    public void ShouldResolveOctal()
    {
        Assert.Equal(29, new Expression("0o16 + 0o17").Evaluate());
    }

    [Fact]
    public void ShouldResolveBinary()
    {
        Assert.Equal(255, new Expression("0b00001111 + 0b11110000").Evaluate());
        Assert.Equal(0UL, new Expression("0b00001111 & 0b11110000").Evaluate());
        Assert.Equal(255UL, new Expression("0b00001111 | 0b11110000").Evaluate());
    }

    [Theory]
    [InlineData("1")]
    [InlineData("1+2")]
    [InlineData("4-3")]
    [InlineData("1*2")]
    public void ShouldNotTreatIntegersAsDecimals(string expr)
    {
        var expression = new Expression(expr, ExpressionOptions.DecimalAsDefault);
        var res = expression.Evaluate();
        Assert.Equal(typeof(int), res.GetType());
    }

    [Fact]
    public void ShouldParseBigNumbersAsDecimals()
    {
        var expr = new Expression("25343463636363454545454544563464.12", ExpressionOptions.DecimalAsDefault, CultureInfo.InvariantCulture);
        var res = expr.Evaluate();
        Assert.Equal(double.PositiveInfinity, res);
    }
}