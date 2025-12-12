using NCalc.LambdaCompilation;
using NCalc.Tests.TestData;

namespace NCalc.Tests;

[Property("Category", "Math")]
public class MathsTests
{
    [Test]
    [MethodDataSource<BuiltInFunctionsTestData>(nameof(BuiltInFunctionsTestData.GetTestData))]
    public async Task BuiltInFunctions_Test(string expression, object expected, double? tolerance, CancellationToken cancellationToken)
    {
        var result = new Expression(expression).Evaluate(cancellationToken);

        if (tolerance.HasValue)
        {
            await Assert.That((double)result).IsEqualTo((double)expected).Within(15);
        }
        else
        {
            await Assert.That(result).IsEqualTo(expected);
        }
    }

    [Test]
    public async Task Should_Modulo_All_Numeric_Types_Issue_58(CancellationToken cancellationToken)
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
                        .Evaluate(cancellationToken);
                    await Assert.That(Convert.ToInt64(result)).IsEqualTo(expectedResult);
                }
                catch (Exception ex)
                {
                    Assert.Fail(ex.Message);
                }
            }

            // These should throw exceptions
            foreach (var typecodeB in shouldNotWork[typecodeA])
            {
                const string expr = $"x {operand} y";
                await Assert.That(() => new Expression(expr, CultureInfo.InvariantCulture)
                {
                    Parameters =
                            {
                                ["x"] = Convert.ChangeType(lhsValue, typecodeA),
                                ["y"] = Convert.ChangeType(rhsValue, typecodeB)
                            }
                }
                        .Evaluate(cancellationToken)).ThrowsExactly<InvalidOperationException>();
            }
        }
    }

    [Test]
    public async Task Should_Add_All_Numeric_Types_Issue_58(CancellationToken cancellationToken)
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
                        .Evaluate(cancellationToken);

                    await Assert.That(Convert.ToInt64(result)).IsEqualTo(expectedResult);
                }
                catch (Exception ex)
                {
                    Assert.Fail(ex.Message);
                }
            }

            // These should throw exceptions

            foreach (var typecodeB in shouldNotWork[typecodeA])
            {
                const string expr = $"x {operand} y";
                await Assert.That(() => new Expression(expr, CultureInfo.InvariantCulture)
                {
                    Parameters =
                            {
                                ["x"] = Convert.ChangeType(1, typecodeA),
                                ["y"] = Convert.ChangeType(1, typecodeB)
                            }
                }
                        .Evaluate(cancellationToken)).ThrowsExactly<InvalidOperationException>();
            }
        }
    }

    [Test]
    public async Task Should_Subtract_All_Numeric_Types_Issue_58(CancellationToken cancellationToken)
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
                        .Evaluate(cancellationToken);

                    await Assert.That(Convert.ToInt64(result)).IsEqualTo(expectedResult);
                }
                catch (Exception ex)
                {
                    Assert.Fail(ex.Message);
                }
            }

            // These should throw exceptions

            foreach (var typecodeB in shouldNotWork[typecodeA])
            {
                const string expr = $"x {operand} y";
                await Assert.That(() => new Expression(expr, CultureInfo.InvariantCulture)
                {
                    Parameters =
                        {
                                ["x"] = Convert.ChangeType(lhsValue, typecodeA),
                                ["y"] = Convert.ChangeType(rhsValue, typecodeB)
                        }
                }.Evaluate(cancellationToken)).ThrowsExactly<InvalidOperationException>();
            }
        }
    }

    [Test]
    public async Task IncorrectCalculation_NCalcAsync_Issue_4(CancellationToken cancellationToken)
    {
        Expression e = new Expression("(1604326026000-1604325747000)/60000");
        var evalutedResult = e.Evaluate(cancellationToken);

        await Assert.That(evalutedResult).IsTypeOf<double>();
        await Assert.That((double)evalutedResult).IsEqualTo(4.65).Within(3);
    }

    [Test]
    [Arguments("1.22e1", 12.2d)]
    [Arguments("1e2", 100d)]
    [Arguments("1e+2", 100d)]
    [Arguments("1e-2", 0.01d)]
    [Arguments(".1e-2", 0.001d)]
    [Arguments("1e10", 10000000000d)]
    public async Task ShouldParseScientificNotation(string expression, double expected, CancellationToken cancellationToken)
    {
        await Assert.That(new Expression(expression).Evaluate(cancellationToken)).IsEqualTo(expected);
    }

    [Test]
    public async Task ShouldHandleLongValues(CancellationToken cancellationToken)
    {
        await Assert.That(new Expression("40000000000+1")
            .Evaluate(cancellationToken)).IsEqualTo(40_000_000_000 + 1);
    }

    [Test]
    public async Task ShouldCompareLongValues(CancellationToken cancellationToken)
    {
        await Assert.That(new Expression("(0=1500000)||(((0+2200000000)-1500000)<0)")
            .Evaluate(cancellationToken)).IsEqualTo(false);
    }

    [Test]
    public async Task ShouldNotConvertRealTypes(CancellationToken cancellationToken)
    {
        var e = new Expression("x/2");
        e.Parameters["x"] = 2F;
        await Assert.That(e.Evaluate(cancellationToken)).IsTypeOf<float>();

        e = new Expression("x/2");
        e.Parameters["x"] = 2D;
        await Assert.That(e.Evaluate(cancellationToken)).IsTypeOf<double>();

        e = new Expression("x/2");
        e.Parameters["x"] = 2m;
        await Assert.That(e.Evaluate(cancellationToken)).IsTypeOf<decimal>();

        e = new Expression("a / b * 100");
        e.Parameters["a"] = 20M;
        e.Parameters["b"] = 20M;
        await Assert.That(e.Evaluate(cancellationToken)).IsEqualTo(100M);
    }

    [Test]
    public async Task Overflow_Issue_190(CancellationToken cancellationToken)
    {
        const decimal minValue = decimal.MinValue;
        var expr = new Expression(minValue.ToString(CultureInfo.InvariantCulture), ExpressionOptions.DecimalAsDefault, CultureInfo.InvariantCulture);
        await Assert.That(expr.Evaluate(cancellationToken)).IsEqualTo(minValue);
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
    public async Task ShouldOptionallyCalculateWithBoolean(string formula, object expectedValue, CancellationToken cancellationToken)
    {
        var expression = new Expression(formula, ExpressionOptions.AllowBooleanCalculation);
        expression.Parameters["X1"] = 1;

        await Assert.That(expression.Evaluate(cancellationToken)).IsEqualTo(expectedValue);

        var lambda = expression.ToLambda<double>(cancellationToken);

        await Assert.That(lambda()).IsEqualTo(Convert.ToDouble(expectedValue));
    }

    [Test]
    public async Task Should_Evaluate_Floor_Of_Double_Max_Value(CancellationToken cancellationToken)
    {
        var expr = new Expression($"Floor({double.MaxValue.ToString(CultureInfo.InvariantCulture)})");
        var res = expr.Evaluate(cancellationToken);

#if NET8_0_OR_GREATER
        await Assert.That(res).IsEqualTo(Math.Floor(double.MaxValue));
#else
        await Assert.That(res).IsEqualTo(double.PositiveInfinity);
#endif
    }

    [Test]
    public async Task Should_Not_Change_Double_Precision(CancellationToken cancellationToken)
    {
        var expr = new Expression("Floor(12e+100)");
        var res = expr.Evaluate(cancellationToken);

        await Assert.That(res).IsEqualTo(Math.Floor(12e+100));
    }

    [Test]
    [Arguments(".05", 0.05)]
    [Arguments("0.05", 0.05)]
    [Arguments("0.005", 0.005)]
    [Arguments(".0", 0d)]
    public async Task Should_Correctly_Parse_Floating_Point_Numbers(string formula, object expectedValue, CancellationToken cancellationToken)
    {
        var expr = new Expression(formula, CultureInfo.InvariantCulture);
        var res = expr.Evaluate(cancellationToken);

        await Assert.That(res).IsEqualTo(expectedValue);
    }

    [Test]
    [Arguments("131055 ^ 8", 131047ul)]
    [Arguments("524288 | 128", 524416ul)]
    [Arguments("262143 & 131055", 131055ul)]
    [Arguments("262143 << 2", 1048572ul)]
    [Arguments("262143 >> 2", 65535ul)]
    public async Task Should_Not_Overflow_Bitwise(string formula, object expectedValue, CancellationToken cancellationToken)
    {
        var e = new Expression(formula, CultureInfo.InvariantCulture);
        var res = e.Evaluate(cancellationToken);

        await Assert.That(res).IsEqualTo(expectedValue);
    }

    [Test]
    [Arguments(int.MaxValue, '+', int.MaxValue)]
    [Arguments(int.MinValue, '-', int.MaxValue)]
    [Arguments(int.MaxValue, '*', int.MaxValue)]
    public async Task Should_Handle_Overflow_Int(int a, char op, int b, CancellationToken cancellationToken)
    {
        var e = new Expression($"[a] {op} [b]", ExpressionOptions.OverflowProtection, CultureInfo.InvariantCulture);
        e.Parameters["a"] = a;
        e.Parameters["b"] = b;

        await Assert.That(() => e.Evaluate(cancellationToken)).ThrowsExactly<OverflowException>();
    }

    [Test]
    [Arguments(double.MaxValue, '+', double.MaxValue)]
    [Arguments(double.MinValue, '-', double.MaxValue)]
    [Arguments(double.MaxValue, '*', double.MaxValue)]
    [Arguments(double.MinValue, '/', 0.001d)]
    public async Task Should_Handle_Overflow_Double(double a, char op, double b, CancellationToken cancellationToken)
    {
        var e = new Expression($"[a] {op} [b]", ExpressionOptions.OverflowProtection, CultureInfo.InvariantCulture);
        e.Parameters["a"] = a;
        e.Parameters["b"] = b;

        await Assert.That(() => e.Evaluate(cancellationToken)).ThrowsExactly<OverflowException>();
    }

    [Test]
    [Arguments(float.MaxValue, '+', float.MaxValue)]
    [Arguments(float.MinValue, '-', float.MaxValue)]
    [Arguments(float.MaxValue, '*', float.MaxValue)]
    [Arguments(float.MinValue, '/', 0.001f)]
    public async Task Should_Handle_Overflow_Float(float a, char op, float b, CancellationToken cancellationToken)
    {
        var e = new Expression($"[a] {op} [b]", ExpressionOptions.OverflowProtection, CultureInfo.InvariantCulture);
        e.Parameters["a"] = a;
        e.Parameters["b"] = b;

        await Assert.That(() => e.Evaluate(cancellationToken)).ThrowsExactly<OverflowException>();
    }

    [Test]
    [Arguments("3 + '3'", ExpressionOptions.AllowCharValues, 54)]
    [Arguments("3 + '3'", ExpressionOptions.None, 6d)]
    [Arguments("'4' + '2'", ExpressionOptions.AllowCharValues, 102)]
    [Arguments("'4' + '2'", ExpressionOptions.StringConcat, "42")]
    [Arguments("'4' + '2'", ExpressionOptions.None, 6d)]
    public async Task ShouldHandleCharAddition(string expression, ExpressionOptions options, object expected, CancellationToken cancellationToken)
    {
        await Assert.That(new Expression(expression, options | ExpressionOptions.NoCache)
            .Evaluate(cancellationToken)).IsEqualTo(expected);
    }

    [Test]
    public async Task DivideNullShouldBeNull(CancellationToken cancellationToken)
    {
        var e = new Expression("a / b", ExpressionOptions.AllowNullParameter);
        e.Parameters["a"] = null;
        e.Parameters["b"] = 2;

        await Assert.That(e.Evaluate(cancellationToken)).IsNull();
    }

    [Test]
    [Arguments((sbyte)10)]
    [Arguments((short)10)]
    [Arguments((int)10)]
    [Arguments((long)10)]
    public async Task ShouldAllowIntegerWithUlongMath(object val, CancellationToken cancellationToken)
    {
        var e = new Expression("a + b");
        e.Parameters["a"] = val;
        e.Parameters["b"] = 1ul;

        var expected = Convert.ToUInt64(val) + 1ul;
        await Assert.That(e.Evaluate(cancellationToken)).IsEqualTo(expected);
    }

    [Test]
    [Arguments(-32767, 65535, 32768)]
    [Arguments(-32768, 65535, 32767)]
    [Arguments(-1, 65535, 65534)]
    [Arguments(2, 65535, 65537)]
    public async Task ShouldAddSignedAndUnsignedShorts(short a, ushort b, int expected, CancellationToken cancellationToken)
    {
        var exp = new Expression("a + b");
        exp.Parameters["a"] = a;
        exp.Parameters["b"] = b;
        var result = exp.Evaluate(cancellationToken);

        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    [Arguments(1, ushort.MaxValue, -65534)]
    public async Task ShouldSubtractSignedAndUnsignedShorts(short a, ushort b, int expected, CancellationToken cancellationToken)
    {
        var exp = new Expression("a - b");
        exp.Parameters["a"] = a;
        exp.Parameters["b"] = b;
        var result = exp.Evaluate(cancellationToken);
        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    [Arguments(1, uint.MaxValue, -4294967294)]
    public async Task ShouldSubtractSignedAndUnsignedInts(int a, uint b, long expected, CancellationToken cancellationToken)
    {
        var exp = new Expression("a - b");
        exp.Parameters["a"] = a;
        exp.Parameters["b"] = b;
        var result = exp.Evaluate(cancellationToken);
        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    [Arguments(short.MaxValue, short.MaxValue, 65534)]
    public async Task ShouldAddToOutOfBoundsShorts(short a, short b, int expected, CancellationToken cancellationToken)
    {
        var exp = new Expression("a + b");
        exp.Parameters["a"] = a;
        exp.Parameters["b"] = b;
        var result = exp.Evaluate(cancellationToken);
        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    [Arguments(short.MinValue, short.MaxValue, -65535)]
    public async Task ShouldSubtractToOutOfBoundsShorts(short a, short b, int expected, CancellationToken cancellationToken)
    {
        var exp = new Expression("a - b");
        exp.Parameters["a"] = a;
        exp.Parameters["b"] = b;
        var result = exp.Evaluate(cancellationToken);

        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    public async Task ShouldAllowLongAsDefault(CancellationToken cancellationToken)
    {
        var exp = new Expression("10000000*1000", ExpressionOptions.LongAsDefault);
        var result = exp.Evaluate(cancellationToken);

        const long expected = 10000000L * 1000;
        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    public async Task ShouldBeDoubleWithLongAsDefault(CancellationToken cancellationToken)
    {
        var exp = new Expression("10000000.1*1000", ExpressionOptions.LongAsDefault, CultureInfo.InvariantCulture);
        var result = exp.Evaluate(cancellationToken);

        const double expected = 10000000.1 * 1000;
        await Assert.That(result).IsEqualTo(expected);
    }
}