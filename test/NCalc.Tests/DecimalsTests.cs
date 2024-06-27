using System.Globalization;

namespace NCalc.Tests;

[Trait("Category","Decimals")]
public class DecimalsTests
{
    [Fact]
    public void Should_Use_Decimal_When_Configured()
    {
        var expression = new Expression("12.34", ExpressionOptions.DecimalAsDefault);

        var result = expression.Evaluate();
        Assert.IsType<decimal>(result);
        Assert.Equal(12.34m, result);
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
}