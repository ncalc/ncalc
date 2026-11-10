using NCalc.LambdaCompilation;
using NCalc.Tests.Attributes;
using System.Threading.Tasks;

namespace NCalc.Tests;

[Property("Category", "Math")]
public class MathsTests
{
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
                    await Assert.That(Convert.ToInt64(result) == expectedResult).IsTrue().Because(
                        $"{expr}: {typecodeA} = {lhsValue}, {typecodeB} = {rhsValue} should return {expectedResult}");
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
                    await Assert.That(Convert.ToInt64(result) == expectedResult).IsTrue().Because(
                        $"{expr}: {typecodeA} = {lhsValue}, {typecodeB} = {rhsValue} should return {expectedResult}");
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
                    await Assert.That(Convert.ToInt64(result) == expectedResult).IsTrue().Because(
                        $"{expr}: {typecodeA} = {lhsValue}, {typecodeB} = {rhsValue} should return {expectedResult}");
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
}
