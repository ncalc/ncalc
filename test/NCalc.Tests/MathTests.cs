using NCalc.LambdaCompilation;

namespace NCalc.Tests;

[Trait("Category", "Math")]
public class MathsTests
{
    [Theory]
    [ClassData(typeof(BuiltInFunctionsTestData))]
    public void BuiltInFunctions_Test(string expression, object expected, double? tolerance)
    {
        var result = new Expression(expression).Evaluate(TestContext.Current.CancellationToken);

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
        const int expectedResult = 0;
        const string operand = "%";
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
            [TypeCode.SByte] = [TypeCode.Boolean],
            [TypeCode.Int16] = [TypeCode.Boolean],
            [TypeCode.UInt16] = [TypeCode.Boolean],
            [TypeCode.Int32] = [TypeCode.Boolean],
            [TypeCode.UInt32] = [TypeCode.Boolean],
            [TypeCode.Int64] = [TypeCode.Boolean],
            [TypeCode.UInt64] = [TypeCode.Boolean],
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
                const string expr = $"x {operand} y";
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
                        .Evaluate(TestContext.Current.CancellationToken);
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
                                ["x"] = Convert.ChangeType(lhsValue, typecodeA),
                                ["y"] = Convert.ChangeType(rhsValue, typecodeB)
                            }
                }
                        .Evaluate(TestContext.Current.CancellationToken));
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
            [TypeCode.SByte] = [TypeCode.Boolean],
            [TypeCode.Int16] = [TypeCode.Boolean],
            [TypeCode.UInt16] = [TypeCode.Boolean],
            [TypeCode.Int32] = [TypeCode.Boolean],
            [TypeCode.UInt32] = [TypeCode.Boolean],
            [TypeCode.Int64] = [TypeCode.Boolean],
            [TypeCode.UInt64] = [TypeCode.Boolean],
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
                const string expr = $"x {operand} y";
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
                        .Evaluate(TestContext.Current.CancellationToken);
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
                        .Evaluate(TestContext.Current.CancellationToken));
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
            [TypeCode.SByte] = [TypeCode.Boolean],
            [TypeCode.Int16] = [TypeCode.Boolean],
            [TypeCode.UInt16] = [TypeCode.Boolean],
            [TypeCode.Int32] = [TypeCode.Boolean],
            [TypeCode.UInt32] = [TypeCode.Boolean],
            [TypeCode.Int64] = [TypeCode.Boolean],
            [TypeCode.UInt64] = [TypeCode.Boolean],
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
                const string expr = $"x {operand} y";
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
                        .Evaluate(TestContext.Current.CancellationToken);
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
                                ["x"] = Convert.ChangeType(lhsValue, typecodeA),
                                ["y"] = Convert.ChangeType(rhsValue, typecodeB)
                        }
                }.Evaluate(TestContext.Current.CancellationToken));
            }
        }
    }

    [Fact]
    public void IncorrectCalculation_NCalcAsync_Issue_4()
    {
        Expression e = new Expression("(1604326026000-1604325747000)/60000");
        var evalutedResult = e.Evaluate(TestContext.Current.CancellationToken);

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
        Assert.Equal(expected, new Expression(expression).Evaluate(TestContext.Current.CancellationToken));
    }

    [Fact]
    public void ShouldHandleLongValues()
    {
        Assert.Equal(40_000_000_000 + 1, new Expression("40000000000+1")
            .Evaluate(TestContext.Current.CancellationToken));
    }

    [Fact]
    public void ShouldCompareLongValues()
    {
        Assert.Equal(false, new Expression("(0=1500000)||(((0+2200000000)-1500000)<0)")
            .Evaluate(TestContext.Current.CancellationToken));
    }

    [Fact]
    public void ShouldNotConvertRealTypes()
    {
        var e = new Expression("x/2");
        e.Parameters["x"] = 2F;
        Assert.IsType<float>(e.Evaluate(TestContext.Current.CancellationToken));

        e = new Expression("x/2");
        e.Parameters["x"] = 2D;
        Assert.IsType<double>(e.Evaluate(TestContext.Current.CancellationToken));

        e = new Expression("x/2");
        e.Parameters["x"] = 2m;
        Assert.IsType<decimal>(e.Evaluate(TestContext.Current.CancellationToken));

        e = new Expression("a / b * 100");
        e.Parameters["a"] = 20M;
        e.Parameters["b"] = 20M;
        Assert.Equal(100M, e.Evaluate(TestContext.Current.CancellationToken));
    }

    [Fact]
    public void Overflow_Issue_190()
    {
        const decimal minValue = decimal.MinValue;
        var expr = new Expression(minValue.ToString(CultureInfo.InvariantCulture), ExpressionOptions.DecimalAsDefault, CultureInfo.InvariantCulture);
        Assert.Equal(minValue, expr.Evaluate(TestContext.Current.CancellationToken));
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

        Assert.Equal(expectedValue, expression.Evaluate(TestContext.Current.CancellationToken));

        var lambda = expression.ToLambda<double>(TestContext.Current.CancellationToken);

        Assert.Equal(Convert.ToDouble(expectedValue), lambda());
    }

    [Fact]
    public void Should_Evaluate_Floor_Of_Double_Max_Value()
    {
        var expr = new Expression($"Floor({double.MaxValue.ToString(CultureInfo.InvariantCulture)})");
        var res = expr.Evaluate(TestContext.Current.CancellationToken);

#if NET8_0_OR_GREATER
        Assert.Equal(Math.Floor(double.MaxValue), res);
#else
        Assert.Equal(double.PositiveInfinity, res);
#endif
    }

    [Fact]
    public void Should_Not_Change_Double_Precision()
    {
        var expr = new Expression("Floor(12e+100)");
        var res = expr.Evaluate(TestContext.Current.CancellationToken);

        Assert.Equal(Math.Floor(12e+100), res);
    }

    [Theory]
    [InlineData(".05", 0.05)]
    [InlineData("0.05", 0.05)]
    [InlineData("0.005", 0.005)]
    [InlineData(".0", 0d)]
    public void Should_Correctly_Parse_Floating_Point_Numbers(string formula, object expectedValue)
    {
        var expr = new Expression(formula, CultureInfo.InvariantCulture);
        var res = expr.Evaluate(TestContext.Current.CancellationToken);

        Assert.Equal(expectedValue, res);
    }

    [Theory]
    [InlineData("131055 ^ 8", 131047ul)]
    [InlineData("524288 | 128", 524416ul)]
    [InlineData("262143 & 131055", 131055ul)]
    [InlineData("262143 << 2", 1048572ul)]
    [InlineData("262143 >> 2", 65535ul)]
    public void Should_Not_Overflow_Bitwise(string formula, object expectedValue)
    {
        var e = new Expression(formula, CultureInfo.InvariantCulture);
        var res = e.Evaluate(TestContext.Current.CancellationToken);

        Assert.Equal(expectedValue, res);
    }

    [Theory]
    [InlineData(int.MaxValue, '+', int.MaxValue)]
    [InlineData(int.MinValue, '-', int.MaxValue)]
    [InlineData(int.MaxValue, '*', int.MaxValue)]
    public void Should_Handle_Overflow_Int(int a, char op, int b)
    {
        var e = new Expression($"[a] {op} [b]", ExpressionOptions.OverflowProtection, CultureInfo.InvariantCulture);
        e.Parameters["a"] = a;
        e.Parameters["b"] = b;

        Assert.Throws<OverflowException>(() => e.Evaluate(TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData(double.MaxValue, '+', double.MaxValue)]
    [InlineData(double.MinValue, '-', double.MaxValue)]
    [InlineData(double.MaxValue, '*', double.MaxValue)]
    [InlineData(double.MinValue, '/', 0.001d)]
    public void Should_Handle_Overflow_Double(double a, char op, double b)
    {
        var e = new Expression($"[a] {op} [b]", ExpressionOptions.OverflowProtection, CultureInfo.InvariantCulture);
        e.Parameters["a"] = a;
        e.Parameters["b"] = b;

        Assert.Throws<OverflowException>(() => e.Evaluate(TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData(float.MaxValue, '+', float.MaxValue)]
    [InlineData(float.MinValue, '-', float.MaxValue)]
    [InlineData(float.MaxValue, '*', float.MaxValue)]
    [InlineData(float.MinValue, '/', 0.001f)]
    public void Should_Handle_Overflow_Float(float a, char op, float b)
    {
        var e = new Expression($"[a] {op} [b]", ExpressionOptions.OverflowProtection, CultureInfo.InvariantCulture);
        e.Parameters["a"] = a;
        e.Parameters["b"] = b;

        Assert.Throws<OverflowException>(() => e.Evaluate(TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData("3 + '3'", ExpressionOptions.AllowCharValues, 54)]
    [InlineData("3 + '3'", ExpressionOptions.None, 6d)]
    [InlineData("'4' + '2'", ExpressionOptions.AllowCharValues, 102)]
    [InlineData("'4' + '2'", ExpressionOptions.StringConcat, "42")]
    [InlineData("'4' + '2'", ExpressionOptions.None, 6d)]
    public void ShouldHandleCharAddition(string expression, ExpressionOptions options, object expected)
    {
        Assert.Equal(expected, new Expression(expression, options | ExpressionOptions.NoCache)
            .Evaluate(TestContext.Current.CancellationToken));
    }

    [Fact]
    public void DivideNullShouldBeNull()
    {
        var e = new Expression("a / b", ExpressionOptions.AllowNullParameter);
        e.Parameters["a"] = null;
        e.Parameters["b"] = 2;

        Assert.Null(e.Evaluate(TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData((sbyte)10)]
    [InlineData((short)10)]
    [InlineData((int)10)]
    [InlineData((long)10)]
    public void ShouldAllowIntegerWithUlongMath(object val)
    {
        var e = new Expression("a + b");
        e.Parameters["a"] = val;
        e.Parameters["b"] = 1ul;

        var expected = Convert.ToUInt64(val) + 1ul;
        Assert.Equal(expected, e.Evaluate(TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData(-32767, 65535, 32768)]
    [InlineData(-32768, 65535, 32767)]
    [InlineData(-1, 65535, 65534)]
    [InlineData(2, 65535, 65537)]
    public void ShouldAddSignedAndUnsignedShorts(short a, ushort b, int expected)
    {
        var exp = new Expression("a + b");
        exp.Parameters["a"] = a;
        exp.Parameters["b"] = b;
        var result = exp.Evaluate(TestContext.Current.CancellationToken);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(1, ushort.MaxValue, -65534)]
    public void ShouldSubtractSignedAndUnsignedShorts(short a, ushort b, int expected)
    {
        var exp = new Expression("a - b");
        exp.Parameters["a"] = a;
        exp.Parameters["b"] = b;
        var result = exp.Evaluate(TestContext.Current.CancellationToken);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(1, uint.MaxValue, -4294967294)]
    public void ShouldSubtractSignedAndUnsignedInts(int a, uint b, long expected)
    {
        var exp = new Expression("a - b");
        exp.Parameters["a"] = a;
        exp.Parameters["b"] = b;
        var result = exp.Evaluate(TestContext.Current.CancellationToken);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(short.MaxValue, short.MaxValue, 65534)]
    public void ShouldAddToOutOfBoundsShorts(short a, short b, int expected)
    {
        var exp = new Expression("a + b");
        exp.Parameters["a"] = a;
        exp.Parameters["b"] = b;
        var result = exp.Evaluate(TestContext.Current.CancellationToken);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(short.MinValue, short.MaxValue, -65535)]
    public void ShouldSubtractToOutOfBoundsShorts(short a, short b, int expected)
    {
        var exp = new Expression("a - b");
        exp.Parameters["a"] = a;
        exp.Parameters["b"] = b;
        var result = exp.Evaluate(TestContext.Current.CancellationToken);

		Assert.Equal(expected, result);
	}

    [Fact]
    public void ShouldAllowLongAsDefault()
    {
        var exp = new Expression("10000000*1000", ExpressionOptions.LongAsDefault);
        var result = exp.Evaluate(TestContext.Current.CancellationToken);

        var expected = 10000000L * 1000;
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ShouldBeDoubleWithLongAsDefault()
    {
        var exp = new Expression("10000000.1*1000", ExpressionOptions.LongAsDefault, CultureInfo.InvariantCulture);
        var result = exp.Evaluate(TestContext.Current.CancellationToken);

        const double expected = 10000000.1 * 1000;
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ShouldAddWithParenthesis()
    {
        var expression = new Expression("(500 * {ml500}) + (250 * {ml250}) + (120 * {ml120}) + (60 * {ml60}) + (30 * {ml30}) + (10 * {ml10}) + (1 * {ml1}) + (0.5 * {ml05})");

        var parameters = new Dictionary<string, object>
        {
            ["ml500"] = 2,
            ["ml250"] = 0,
            ["ml120"] = 1,
            ["ml60"] = 0,
            ["ml30"] = 3,
            ["ml10"] = 0,
            ["ml1"] = 10,
            ["ml05"] = 4
        };
        expression.Parameters = parameters;

        var result = expression.Evaluate()!;

        const double expected = (2 * 500) + (1 * 120) + (3 * 30) + (10 * 1) + (4 * 0.5);

        Assert.Equal(expected, Convert.ToDouble(result));
    }
}