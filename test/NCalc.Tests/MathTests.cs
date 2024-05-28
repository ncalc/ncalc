using System.Globalization;
using Assert = Xunit.Assert;

namespace NCalc.Tests;

[Trait("Category","Math")]
public class MathsTests
{
    public static TheoryData<string, object, double?> GetBuiltInFunctionTestData()
    {
        var data = new TheoryData<string, object, double?>
        {
            { "Abs(-1)", 1M, null },
            { "Acos(1)", 0d, null },
            { "Asin(0)", 0d, null },
            { "Atan(0)", 0d, null },
            { "Ceiling(1.5)", 2d, null },
            { "Cos(0)", 1d, null },
            { "Exp(0)", 1d, null },
            { "Floor(1.5)", 1d, null },
            { "IEEERemainder(3,2)", -1d, null },
            { "Log(1,10)", 0d, null },
            { "Ln(1)", 0d, null },
            { "Log10(1)", 0d, null },
            { "Pow(3,2)", 9d, null },
            { "Round(3.222,2)", 3.22d, null },
            { "Sign(-10)", -1, null },
            { "Sin(0)", 0d, null },
            { "Sqrt(4)", 2d, null },
            { "Tan(0)", 0d, null },
            { "Truncate(1.7)", 1d, null },
            { "Atan2(-1,0)", -Math.PI/2, 1e-16 },
            { "Atan2(1,0)", Math.PI/2, 1e-16 },
            { "Atan2(0,-1)", Math.PI, 1e-16 },
            { "Atan2(0,1)", 0d, 1e-16 },
            { "Max(1,10)", 10, null },
            { "Min(1,10)", 1, null }
        };

        return data;
    }

    [Theory]
    [MemberData(nameof(GetBuiltInFunctionTestData))]
    public void BuiltInFunctions_Test(string expression, object expected, double? tolerance)
    {
        var result = new Expression(expression).Evaluate();

        if (tolerance.HasValue)
        {
            Assert.Equal((double)expected, (double)result, precision: 15);
        }
        else
        {
            Assert.Equal(expected, result);
        }
    }
    
    [Fact]
    public void Should_Modulo_All_Numeric_Types_Issue_58()
    {
        // https://github.com/ncalc/ncalc/issues/58
        var expectedResult = 0;
        var operand = "%";
        var lhsValue = 50;
        var rhsValue = 50;

        var allTypes = new List<TypeCode>()
        {
            TypeCode.Boolean, TypeCode.Byte, TypeCode.SByte, TypeCode.Int16, TypeCode.UInt16, TypeCode.Int32,
            TypeCode.UInt32, TypeCode.Int64, TypeCode.UInt64, TypeCode.Single, TypeCode.Double, TypeCode.Decimal
        };

        var shouldNotWork = new Dictionary<TypeCode, List<TypeCode>>
        {
            // We want to test all of the cases in numbers.cs which means we need to test both LHS/RHS
            [TypeCode.Boolean] = allTypes,
            [TypeCode.Byte] = [TypeCode.Boolean],
            [TypeCode.SByte] = [TypeCode.Boolean, TypeCode.UInt64],
            [TypeCode.Int16] = [TypeCode.Boolean, TypeCode.UInt64],
            [TypeCode.UInt16] = [TypeCode.Boolean],
            [TypeCode.Int32] = [TypeCode.Boolean, TypeCode.UInt64],
            [TypeCode.UInt32] = [TypeCode.Boolean],
            [TypeCode.Int64] = [TypeCode.Boolean, TypeCode.UInt64],
            [TypeCode.UInt64] = [TypeCode.Boolean, TypeCode.SByte, TypeCode.Int16, TypeCode.Int32, TypeCode.Int64],
            [TypeCode.Single] = [TypeCode.Boolean, TypeCode.Decimal],
            [TypeCode.Double] = [TypeCode.Boolean, TypeCode.Decimal],
            [TypeCode.Decimal] = [TypeCode.Boolean, TypeCode.Single, TypeCode.Double]
        };

        // These should all work and return a value
        foreach (var typecodeA in allTypes)
        {
            var toTest = allTypes.Except(shouldNotWork[typecodeA]);
            foreach (var typecodeB in toTest)
            {
                var expr = $"x {operand} y";
                try
                {
                    var result = new Expression(expr, CultureInfo.InvariantCulture)
                        {
                            Parameters =
                            {
                                ["x"] = Convert.ChangeType(lhsValue, typecodeA),
                                ["y"] = Convert.ChangeType(rhsValue, typecodeB)
                            }
                        }
                        .Evaluate();
                    Assert.True(Convert.ToInt64(result) == expectedResult,
                        $"{expr}: {typecodeA} = {lhsValue}, {typecodeB} = {rhsValue} should return {expectedResult}");
                }
                catch (Exception ex)
                {
                    Assert.Fail($"{expr}: {typecodeA}, {typecodeB} should not throw an exception but {ex} was thrown");
                }
            }

            // These should throw exceptions
            foreach (var typecodeB in shouldNotWork[typecodeA])
            {
                var expr = $"x {operand} y";
                Assert.Throws<InvalidOperationException>(() => new Expression(expr, CultureInfo.InvariantCulture)
                        {
                            Parameters =
                            {
                                ["x"] = Convert.ChangeType(lhsValue, typecodeA),
                                ["y"] = Convert.ChangeType(rhsValue, typecodeB)
                            }
                        }
                        .Evaluate());
            }
        }
    }

    
    [Fact]
    public void Should_Add_All_Numeric_Types_Issue_58()
    {
        // https://github.com/ncalc/ncalc/issues/58
        const int expectedResult = 100;
        const string operand = "+";
        const string lhsValue = "50";
        const string rhsValue = "50";

        var allTypes = new List<TypeCode>()
        {
            TypeCode.Boolean, TypeCode.Byte, TypeCode.SByte, TypeCode.Int16, TypeCode.UInt16, TypeCode.Int32,
            TypeCode.UInt32, TypeCode.Int64, TypeCode.UInt64, TypeCode.Single, TypeCode.Double, TypeCode.Decimal
        };

        var shouldNotWork = new Dictionary<TypeCode, List<TypeCode>>
        {
            // We want to test all of the cases in numbers.cs which means we need to test both LHS/RHS
            [TypeCode.Boolean] = allTypes,
            [TypeCode.Byte] = [TypeCode.Boolean],
            [TypeCode.SByte] = [TypeCode.Boolean, TypeCode.UInt64],
            [TypeCode.Int16] = [TypeCode.Boolean, TypeCode.UInt64],
            [TypeCode.UInt16] = [TypeCode.Boolean],
            [TypeCode.Int32] = [TypeCode.Boolean, TypeCode.UInt64],
            [TypeCode.UInt32] = [TypeCode.Boolean],
            [TypeCode.Int64] = [TypeCode.Boolean, TypeCode.UInt64],
            [TypeCode.UInt64] = [TypeCode.Boolean, TypeCode.SByte, TypeCode.Int16, TypeCode.Int32, TypeCode.Int64],
            [TypeCode.Single] = [TypeCode.Boolean],
            [TypeCode.Double] = [TypeCode.Boolean],
            [TypeCode.Decimal] = [TypeCode.Boolean]
        };

        // These should all work and return a value
        foreach (var typecodeA in allTypes)
        {
            var toTest = allTypes.Except(shouldNotWork[typecodeA]);
            foreach (var typecodeB in toTest)
            {
                var expr = $"x {operand} y";
                try
                {
                    var result = new Expression(expr, CultureInfo.InvariantCulture)
                        {
                            Parameters =
                            {
                                ["x"] = Convert.ChangeType(lhsValue, typecodeA),
                                ["y"] = Convert.ChangeType(rhsValue, typecodeB)
                            } 
                        }
                        .Evaluate();
                    Assert.True(Convert.ToInt64(result) == expectedResult,
                        $"{expr}: {typecodeA} = {lhsValue}, {typecodeB} = {rhsValue} should return {expectedResult}");
                }
                catch (Exception ex)
                {
                    Assert.Fail($"{expr}: {typecodeA}, {typecodeB} should not throw an exception but {ex} was thrown");
                }
            }

            // These should throw exceptions

            foreach (var typecodeB in shouldNotWork[typecodeA])
            {
                const string expr = $"x {operand} y";
                Assert.Throws<InvalidOperationException>(() => new Expression(expr, CultureInfo.InvariantCulture)
                        {
                            Parameters =
                            {
                                ["x"] = Convert.ChangeType(1, typecodeA),
                                ["y"] = Convert.ChangeType(1, typecodeB)
                            }
                        }
                        .Evaluate());
            }
        }
    }
    
    [Fact]
    public void Should_Subtract_All_Numeric_Types_Issue_58()
    {
        // https://github.com/ncalc/ncalc/issues/58
        const int expectedResult = 0;
        const string operand = "-";
        const int lhsValue = 50;
        const int rhsValue = 50;

        var allTypes = new List<TypeCode>()
        {
            TypeCode.Boolean, TypeCode.Byte, TypeCode.SByte, TypeCode.Int16, TypeCode.UInt16, TypeCode.Int32,
            TypeCode.UInt32, TypeCode.Int64, TypeCode.UInt64, TypeCode.Single, TypeCode.Double, TypeCode.Decimal
        };

        var shouldNotWork = new Dictionary<TypeCode, List<TypeCode>>
        {
            // We want to test all of the cases in numbers.cs which means we need to test both LHS/RHS
            [TypeCode.Boolean] = allTypes,
            [TypeCode.Byte] = [TypeCode.Boolean],
            [TypeCode.SByte] = [TypeCode.Boolean, TypeCode.UInt64],
            [TypeCode.Int16] = [TypeCode.Boolean, TypeCode.UInt64],
            [TypeCode.UInt16] = [TypeCode.Boolean],
            [TypeCode.Int32] = [TypeCode.Boolean, TypeCode.UInt64],
            [TypeCode.UInt32] = [TypeCode.Boolean],
            [TypeCode.Int64] = [TypeCode.Boolean, TypeCode.UInt64],
            [TypeCode.UInt64] = [TypeCode.Boolean, TypeCode.SByte, TypeCode.Int16, TypeCode.Int32, TypeCode.Int64],
            [TypeCode.Single] = [TypeCode.Boolean, TypeCode.Decimal],
            [TypeCode.Double] = [TypeCode.Boolean],
            [TypeCode.Decimal] = [TypeCode.Boolean]
        };

        // These should all work and return a value
        foreach (var typecodeA in allTypes)
        {
            var toTest = allTypes.Except(shouldNotWork[typecodeA]);
            foreach (var typecodeB in toTest)
            {
                var expr = $"x {operand} y";
                try
                {
                    var result = new Expression(expr, CultureInfo.InvariantCulture)
                        {
                            Parameters =
                            {
                                ["x"] = Convert.ChangeType(lhsValue, typecodeA),
                                ["y"] = Convert.ChangeType(rhsValue, typecodeB)
                            }
                        }
                        .Evaluate();
                    Assert.True(Convert.ToInt64(result) == expectedResult,
                        $"{expr}: {typecodeA} = {lhsValue}, {typecodeB} = {rhsValue} should return {expectedResult}");
                }
                catch (Exception ex)
                {
                    Assert.Fail($"{expr}: {typecodeA}, {typecodeB} should not throw an exception but {ex} was thrown");
                }
            }

            // These should throw exceptions

            foreach (var typecodeB in shouldNotWork[typecodeA])
            {
                var expr = $"x {operand} y";
                Assert.Throws<InvalidOperationException>(() => new Expression(expr, CultureInfo.InvariantCulture)
                        {
                            Parameters =
                            {
                                ["x"] = Convert.ChangeType(lhsValue, typecodeA),
                                ["y"] = Convert.ChangeType(rhsValue, typecodeB)
                            }
                        }
                        .Evaluate());
            }
        }
    }
    
    [Fact]
    public void IncorrectCalculation_NCalcAsync_Issue_4()
    {
        Expression e = new Expression("(1604326026000-1604325747000)/60000");
        var evalutedResult = e.Evaluate();

        Assert.IsType<double>(evalutedResult);
        Assert.Equal(4.65, (double)evalutedResult, 3);
    }
    
    [Theory]
    [InlineData("1.22e1", 12.2d)]
    [InlineData("1e2", 100d)]
    [InlineData("1e+2", 100d)]
    [InlineData("1e-2", 0.01d)]
    [InlineData(".1e-2", 0.001d)]
    [InlineData("1e10", 10000000000d)]
    public void ShouldParseScientificNotation(string expression, double expected)
    {
        Assert.Equal(expected, new Expression(expression).Evaluate());
    }
    
    [Fact]
    public void ShouldHandleLongValues()
    {
        Assert.Equal(40_000_000_000 + 1, new Expression("40000000000+1").Evaluate());
    }

    [Fact]
    public void ShouldCompareLongValues()
    {
        Assert.Equal(false, new Expression("(0=1500000)||(((0+2200000000)-1500000)<0)").Evaluate());
    }
    
    [Fact]
    public void ShouldNotConvertRealTypes()
    {
        var e = new Expression("x/2");
        e.Parameters["x"] = 2F;
        Assert.IsType<float>(e.Evaluate());

        e = new Expression("x/2");
        e.Parameters["x"] = 2D;
        Assert.IsType<double>(e.Evaluate());

        e = new Expression("x/2");
        e.Parameters["x"] = 2m;
        Assert.IsType<decimal>(e.Evaluate());

        e = new Expression("a / b * 100");
        e.Parameters["a"] = 20M;
        e.Parameters["b"] = 20M;
        Assert.Equal(100M, e.Evaluate());
    }
    
    [Fact]
    public void Overflow_Issue_190()
    {
        const decimal minValue = decimal.MinValue;
        var expr = new Expression(minValue.ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
        Assert.Equal(minValue,expr.Evaluate());
    }
    
    [Theory]
    [InlineData("(X1 = 1)/2", 0.5)]
    [InlineData("(X1 = 1)*2", 2)]
    [InlineData("(X1 = 1)+1", 2)]
    [InlineData("(X1 = 1)-1", 0)]
    [InlineData("2*(X1 = 1)", 2)]
    [InlineData("2/(X1 = 1)", 2.0)]
    [InlineData("1+(X1 = 1)", 2)]
    [InlineData("true-(X1 = 1)", 0)]
    [InlineData("true-(X1 = true - false)", 0)]
    public void ShouldOptionallyCalculateWithBoolean(string formula, object expectedValue)
    {
        var expression = new Expression(formula, ExpressionOptions.AllowBooleanCalculation);
        expression.Parameters["X1"] = 1;

        Assert.Equal(expectedValue, expression.Evaluate());


        var lambda = expression.ToLambda<double>();
        
        Assert.Equal(Convert.ToDouble(expectedValue), lambda());
    }
    
    [Fact]
    public void Should_Evaluate_Floor_Of_Double_Max_Value()
    {
        var expr = new Expression($"Floor({double.MaxValue.ToString(CultureInfo.InvariantCulture)})");
        var res = expr.Evaluate();

#if NET8_0
        Assert.Equal(Math.Floor(double.MaxValue), res);
#else
        Assert.Equal(double.PositiveInfinity, res);
#endif
    }
}