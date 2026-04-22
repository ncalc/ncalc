using NCalc.LambdaCompilation;
using System.Threading.Tasks;

namespace NCalc.Tests;

[Property("Category", "Math")]
public class MathsTests
{
    [Test]
    [MethodDataSource(typeof(BuiltInFunctionsTestData), "GetEnumerator")]
    public async Task BuiltInFunctions_Test(string expression, object expected, double? tolerance)
    {
        var result = new Expression(expression).Evaluate(CancellationToken.None);

        if (tolerance.HasValue)
        {
            // TODO: TUnit migration - xUnit Assert.Equal had additional argument(s) (precision: 15) that could not be converted.
            await Assert.That((double)result).IsEqualTo((double)expected);
        }
        else
        {
            await Assert.That(result).IsEqualTo(expected);
        }
    }

    [Test]
    public async Task Should_Modulo_All_Numeric_Types_Issue_58()
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
                        .Evaluate(CancellationToken.None);
                    await Assert.That(Convert.ToInt64(result) == expectedResult).IsTrue().Because($"{expr}: {typecodeA} = {lhsValue}, {typecodeB} = {rhsValue} should return {expectedResult}");
                }
                catch (Exception ex)
                {
                    Assert.Fail("Assertion failure");
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
                        .Evaluate(CancellationToken.None));
            }
        }
    }

    [Test]
    public async Task Should_Add_All_Numeric_Types_Issue_58()
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
                        .Evaluate(CancellationToken.None);
                    await Assert.That(Convert.ToInt64(result) == expectedResult).IsTrue().Because($"{expr}: {typecodeA} = {lhsValue}, {typecodeB} = {rhsValue} should return {expectedResult}");
                }
                catch (Exception ex)
                {
                    Assert.Fail("Assertion failure");
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
                        .Evaluate(CancellationToken.None));
            }
        }
    }

    [Test]
    public async Task Should_Subtract_All_Numeric_Types_Issue_58()
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
                        .Evaluate(CancellationToken.None);
                    await Assert.That(Convert.ToInt64(result) == expectedResult).IsTrue().Because($"{expr}: {typecodeA} = {lhsValue}, {typecodeB} = {rhsValue} should return {expectedResult}");
                }
                catch (Exception ex)
                {
                    Assert.Fail("Assertion failure");
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
                }.Evaluate(CancellationToken.None));
            }
        }
    }

    [Test]
    public async Task IncorrectCalculation_NCalcAsync_Issue_4()
    {
        Expression e = new Expression("(1604326026000-1604325747000)/60000");
        var evalutedResult = e.Evaluate(CancellationToken.None);

        await Assert.That(evalutedResult).IsTypeOf<double>();
        // TODO: TUnit migration - xUnit Assert.Equal had additional argument(s) (precision: 3) that could not be converted.
        await Assert.That((double)evalutedResult).IsEqualTo(4.65);
    }

    [Test]
    [Arguments("1.22e1", 12.2d)]
    [Arguments("1e2", 100d)]
    [Arguments("1e+2", 100d)]
    [Arguments("1e-2", 0.01d)]
    [Arguments(".1e-2", 0.001d)]
    [Arguments("1e10", 10000000000d)]
    public async Task ShouldParseScientificNotation(string expression, double expected)
    {
        await Assert.That(new Expression(expression).Evaluate(CancellationToken.None)).IsEqualTo(expected);
    }

    [Test]
    public async Task ShouldHandleLongValues()
    {
        await Assert.That(new Expression("40000000000+1")
            .Evaluate(CancellationToken.None)).IsEqualTo(40_000_000_000 + 1);
    }

    [Test]
    public async Task ShouldCompareLongValues()
    {
        await Assert.That(new Expression("(0=1500000)||(((0+2200000000)-1500000)<0)")
            .Evaluate(CancellationToken.None)).IsEqualTo(false);
    }

    [Test]
    public async Task ShouldNotConvertRealTypes()
    {
        var e = new Expression("x/2");
        e.Parameters["x"] = 2F;
        await Assert.That(e.Evaluate(CancellationToken.None)).IsTypeOf<float>();

        e = new Expression("x/2");
        e.Parameters["x"] = 2D;
        await Assert.That(e.Evaluate(CancellationToken.None)).IsTypeOf<double>();

        e = new Expression("x/2");
        e.Parameters["x"] = 2m;
        await Assert.That(e.Evaluate(CancellationToken.None)).IsTypeOf<decimal>();

        e = new Expression("a / b * 100");
        e.Parameters["a"] = 20M;
        e.Parameters["b"] = 20M;
        await Assert.That(e.Evaluate(CancellationToken.None)).IsEqualTo(100M);
    }

    [Test]
    public async Task Overflow_Issue_190()
    {
        const decimal minValue = decimal.MinValue;
        var expr = new Expression(minValue.ToString(CultureInfo.InvariantCulture), ExpressionOptions.DecimalAsDefault, CultureInfo.InvariantCulture);
        await Assert.That(expr.Evaluate(CancellationToken.None)).IsEqualTo(minValue);
    }

    [Test]
    [Arguments("(X1 = 1)/2", 0.5)]
    [Arguments("(X1 = 1)*2", 2)]
    [Arguments("(X1 = 1)+1", 2)]
    [Arguments("(X1 = 1)-1", 0)]
    [Arguments("2*(X1 = 1)", 2)]
    [Arguments("2/(X1 = 1)", 2.0)]
    [Arguments("1+(X1 = 1)", 2)]
    [Arguments("true-(X1 = 1)", 0)]
    [Arguments("true-(X1 = true - false)", 0)]
    public async Task ShouldOptionallyCalculateWithBoolean(string formula, object expectedValue)
    {
        var expression = new Expression(formula, ExpressionOptions.AllowBooleanCalculation);
        expression.Parameters["X1"] = 1;

        await Assert.That(expression.Evaluate(CancellationToken.None)).IsEqualTo(expectedValue);

        var lambda = expression.ToLambda<double>(CancellationToken.None);

        await Assert.That(lambda()).IsEqualTo(Convert.ToDouble(expectedValue));
    }

    [Test]
    public async Task Should_Evaluate_Floor_Of_Double_Max_Value()
    {
        var expr = new Expression($"Floor({double.MaxValue.ToString(CultureInfo.InvariantCulture)})");
        var res = expr.Evaluate(CancellationToken.None);

#if NET8_0_OR_GREATER
        await Assert.That(res).IsEqualTo(Math.Floor(double.MaxValue));
#else
        Assert.Equal(double.PositiveInfinity, res);
#endif
    }

    [Test]
    public async Task Should_Not_Change_Double_Precision()
    {
        var expr = new Expression("Floor(12e+100)");
        var res = expr.Evaluate(CancellationToken.None);

        await Assert.That(res).IsEqualTo(Math.Floor(12e+100));
    }

    [Test]
    [Arguments(".05", 0.05)]
    [Arguments("0.05", 0.05)]
    [Arguments("0.005", 0.005)]
    [Arguments(".0", 0d)]
    public async Task Should_Correctly_Parse_Floating_Point_Numbers(string formula, object expectedValue)
    {
        var expr = new Expression(formula, CultureInfo.InvariantCulture);
        var res = expr.Evaluate(CancellationToken.None);

        await Assert.That(res).IsEqualTo(expectedValue);
    }

    [Test]
    [Arguments("131055 ^ 8", 131047ul)]
    [Arguments("524288 | 128", 524416ul)]
    [Arguments("262143 & 131055", 131055ul)]
    [Arguments("262143 << 2", 1048572ul)]
    [Arguments("262143 >> 2", 65535ul)]
    public async Task Should_Not_Overflow_Bitwise(string formula, object expectedValue)
    {
        var e = new Expression(formula, CultureInfo.InvariantCulture);
        var res = e.Evaluate(CancellationToken.None);

        await Assert.That(res).IsEqualTo(expectedValue);
    }

    [Test]
    [Arguments(int.MaxValue, '+', int.MaxValue)]
    [Arguments(int.MinValue, '-', int.MaxValue)]
    [Arguments(int.MaxValue, '*', int.MaxValue)]
    public void Should_Handle_Overflow_Int(int a, char op, int b)
    {
        var e = new Expression($"[a] {op} [b]", ExpressionOptions.OverflowProtection, CultureInfo.InvariantCulture);
        e.Parameters["a"] = a;
        e.Parameters["b"] = b;

        Assert.Throws<OverflowException>(() => e.Evaluate(CancellationToken.None));
    }

    [Test]
    [Arguments(double.MaxValue, '+', double.MaxValue)]
    [Arguments(double.MinValue, '-', double.MaxValue)]
    [Arguments(double.MaxValue, '*', double.MaxValue)]
    [Arguments(double.MinValue, '/', 0.001d)]
    public void Should_Handle_Overflow_Double(double a, char op, double b)
    {
        var e = new Expression($"[a] {op} [b]", ExpressionOptions.OverflowProtection, CultureInfo.InvariantCulture);
        e.Parameters["a"] = a;
        e.Parameters["b"] = b;

        Assert.Throws<OverflowException>(() => e.Evaluate(CancellationToken.None));
    }

    [Test]
    [Arguments(float.MaxValue, '+', float.MaxValue)]
    [Arguments(float.MinValue, '-', float.MaxValue)]
    [Arguments(float.MaxValue, '*', float.MaxValue)]
    [Arguments(float.MinValue, '/', 0.001f)]
    public void Should_Handle_Overflow_Float(float a, char op, float b)
    {
        var e = new Expression($"[a] {op} [b]", ExpressionOptions.OverflowProtection, CultureInfo.InvariantCulture);
        e.Parameters["a"] = a;
        e.Parameters["b"] = b;

        Assert.Throws<OverflowException>(() => e.Evaluate(CancellationToken.None));
    }

    [Test]
    [Arguments("3 + '3'", ExpressionOptions.AllowCharValues, 54)]
    [Arguments("3 + '3'", ExpressionOptions.None, 6d)]
    [Arguments("'4' + '2'", ExpressionOptions.AllowCharValues, 102)]
    [Arguments("'4' + '2'", ExpressionOptions.StringConcat, "42")]
    [Arguments("'4' + '2'", ExpressionOptions.None, 6d)]
    public async Task ShouldHandleCharAddition(string expression, ExpressionOptions options, object expected)
    {
        await Assert.That(new Expression(expression, options | ExpressionOptions.NoCache)
            .Evaluate(CancellationToken.None)).IsEqualTo(expected);
    }

    [Test]
    [Arguments("1 + ''", 1)]
    [Arguments("'' + 1", 1)]
    [Arguments("1 + null", 1)]
    [Arguments("null + 1", 1)]
    [Arguments("1 - ''", 1)]
    [Arguments("'' - 1", -1)]
    [Arguments("1 - null", 1)]
    [Arguments("null - 1", -1)]
    [Arguments("2 * ''", 0)]
    [Arguments("'' * 2", 0)]
    [Arguments("2 * null", 0)]
    [Arguments("null * 2", 0)]
    [Arguments("'' / 2", 0d)]
    [Arguments("null / 2", 0d)]
    [Arguments("'' % 2", 0)]
    [Arguments("null % 2", 0)]
    [Arguments("'' + ''", 0)]
    public void ShouldUseArithmeticNullOrEmptyStringAsZero(string expressionString, object expected)
    {
        const ExpressionOptions options = ExpressionOptions.ArithmeticNullOrEmptyStringAsZero | ExpressionOptions.AllowNullParameter | ExpressionOptions.NoCache;
        var expression = new Expression(expressionString, options);

        Assert.Expression(expected, expression);
    }

    [Test]
    public async Task DivideNullShouldBeNull()
    {
        var e = new Expression("a / b", ExpressionOptions.AllowNullParameter);
        e.Parameters["a"] = null;
        e.Parameters["b"] = 2;

        await Assert.That(e.Evaluate(CancellationToken.None)).IsNull();
    }

    [Test]
    [Arguments((sbyte)10)]
    [Arguments((short)10)]
    [Arguments((int)10)]
    [Arguments((long)10)]
    public async Task ShouldAllowIntegerWithUlongMath(object val)
    {
        var e = new Expression("a + b");
        e.Parameters["a"] = val;
        e.Parameters["b"] = 1ul;

        var expected = Convert.ToUInt64(val) + 1ul;
        await Assert.That(e.Evaluate(CancellationToken.None)).IsEqualTo(expected);
    }

    [Test]
    [Arguments(-32767, 65535, 32768)]
    [Arguments(-32768, 65535, 32767)]
    [Arguments(-1, 65535, 65534)]
    [Arguments(2, 65535, 65537)]
    public async Task ShouldAddSignedAndUnsignedShorts(short a, ushort b, int expected)
    {
        var exp = new Expression("a + b");
        exp.Parameters["a"] = a;
        exp.Parameters["b"] = b;
        var result = exp.Evaluate(CancellationToken.None);

        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    [Arguments(1, ushort.MaxValue, -65534)]
    public async Task ShouldSubtractSignedAndUnsignedShorts(short a, ushort b, int expected)
    {
        var exp = new Expression("a - b");
        exp.Parameters["a"] = a;
        exp.Parameters["b"] = b;
        var result = exp.Evaluate(CancellationToken.None);
        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    [Arguments(1, uint.MaxValue, -4294967294)]
    public async Task ShouldSubtractSignedAndUnsignedInts(int a, uint b, long expected)
    {
        var exp = new Expression("a - b");
        exp.Parameters["a"] = a;
        exp.Parameters["b"] = b;
        var result = exp.Evaluate(CancellationToken.None);
        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    [Arguments(short.MaxValue, short.MaxValue, 65534)]
    public async Task ShouldAddToOutOfBoundsShorts(short a, short b, int expected)
    {
        var exp = new Expression("a + b");
        exp.Parameters["a"] = a;
        exp.Parameters["b"] = b;
        var result = exp.Evaluate(CancellationToken.None);
        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    [Arguments(short.MinValue, short.MaxValue, -65535)]
    public async Task ShouldSubtractToOutOfBoundsShorts(short a, short b, int expected)
    {
        var exp = new Expression("a - b");
        exp.Parameters["a"] = a;
        exp.Parameters["b"] = b;
        var result = exp.Evaluate(CancellationToken.None);

		await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    public async Task ShouldAllowLongAsDefault()
    {
        var exp = new Expression("10000000*1000", ExpressionOptions.LongAsDefault);
        var result = exp.Evaluate(CancellationToken.None);

        var expected = 10000000L * 1000;
        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    public async Task ShouldBeDoubleWithLongAsDefault()
    {
        var exp = new Expression("10000000.1*1000", ExpressionOptions.LongAsDefault, CultureInfo.InvariantCulture);
        var result = exp.Evaluate(CancellationToken.None);

        const double expected = 10000000.1 * 1000;
        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    public async Task ShouldAddWithParenthesis()
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

        await Assert.That(Convert.ToDouble(result)).IsEqualTo(expected);
    }
}