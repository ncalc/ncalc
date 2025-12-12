#if NET8_0_OR_GREATER
#nullable enable
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using NCalc.LambdaCompilation;
// ReSharper disable MemberCanBeProtected.Local

namespace NCalc.Tests;

[SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Local", Justification = "Reflection")]
[SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Reflection")]
[SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Reflection")]
[SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Style")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local", Justification = "Reflection")]

public class LambdaTests
{
    private class Context
    {
        public int FieldA { get; set; }
        public string? FieldB { get; set; }
        public decimal FieldC { get; set; }
        public decimal? FieldD { get; set; }
        public int? FieldE { get; set; }

        // ReSharper disable once MemberCanBeMadeStatic.Local
        public int Test(int a, int b)
        {
            return a + b;
        }

        public string Test(string a, string b)
        {
            return a + b;
        }

        public int Test(int a, int b, int c)
        {
            return a + b + c;
        }

        public double Test(double a, double b, double c)
        {
            return a + b + c;
        }

        public string Sum(string msg, params int[] numbers)
        {
            var total = 0;
            foreach (var num in numbers)
            {
                total += num;
            }
            return msg + total;
        }

        public int Sum(params int[] numbers)
        {
            var total = 0;
            foreach (var num in numbers)
            {
                total += num;
            }
            return total;
        }

        public int Sum(TestObject1 obj1, TestObject2 obj2)
        {
            return obj1.Count1 + obj2.Count2;
        }

        public int Sum(TestObject2 obj1, TestObject1 obj2)
        {
            return obj1.Count2 + obj2.Count1;
        }

        public int Sum(TestObject1 obj1, TestObject1 obj2)
        {
            return obj1.Count1 + obj2.Count1;
        }

        public int Sum(TestObject2 obj1, TestObject2 obj2)
        {
            return obj1.Count2 + obj2.Count2;
        }

        public double Max(TestObject1 obj1, TestObject1 obj2)
        {
            return Math.Max(obj1.Count1, obj2.Count1);
        }

        public double Min(TestObject1 obj1, TestObject1 obj2)
        {
            return Math.Min(obj1.Count1, obj2.Count1);
        }

        public class TestObject1
        {
            public int Count1 { get; set; }
        }

        public class TestObject2
        {
            public int Count2 { get; set; }
        }

        public TestObject1 CreateTestObject1(int count)
        {
            return new TestObject1() { Count1 = count };
        }

        public TestObject2 CreateTestObject2(int count)
        {
            return new TestObject2() { Count2 = count };
        }
    }

    private class SubContext : Context
    {
        public int Multiply(int a, int b)
        {
            return a * b;
        }

        public new int Test(int a, int b)
        {
            return base.Test(a, b) / 2;
        }

        public int Test(int a, int b, int c, int d)
        {
            return a + b + c + d;
        }

        public int Sum(TestObject1 obj1, TestObject2 obj2, TestObject2 obj3)
        {
            return obj1.Count1 + obj2.Count2 + obj3.Count2 + 100;
        }
    }

    [Test]
    [Arguments("1+2", 3)]
    [Arguments("1-2", -1)]
    [Arguments("2*2", 4)]
    [Arguments("10/2", 5)]
    [Arguments("7%2", 1)]
    public async Task ShouldHandleIntegers(string input, int expected, CancellationToken cancellationToken)
    {
        var expression = new Expression(input);
        var sut = expression.ToLambda<int>(cancellationToken);

        await Assert.That(sut()).IsEqualTo(expected);
    }

    [Test]
    public async Task ShouldHandleParameters(CancellationToken cancellationToken)
    {
        var expression = new Expression("[FieldA] > 5 && [FieldB] = 'test'");
        var sut = expression.ToLambda<Context, bool>(cancellationToken);
        var context = new Context { FieldA = 7, FieldB = "test" };

        await Assert.That(sut(context)).IsTrue();
    }

    [Test]
    public async Task ShouldHandleOverloadingSameParamCount(CancellationToken cancellationToken)
    {
        var expression = new Expression("Test('Hello', ' world!')");
        var sut = expression.ToLambda<Context, string>(cancellationToken);
        var context = new Context();

        await Assert.That(sut(context)).IsEqualTo("Hello world!");
    }

    [Test]
    public async Task ShouldHandleOverloadingDifferentParamCount(CancellationToken cancellationToken)
    {
        var expression = new Expression("Test(Test(1, 2), 3, 4)");
        var sut = expression.ToLambda<Context, int>(cancellationToken);
        var context = new Context();

        await Assert.That(sut(context)).IsEqualTo(10);
    }

    [Test]
    public async Task ShouldHandleOverloadingObjectParameters(CancellationToken cancellationToken)
    {
        var expression = new Expression("Sum(CreateTestObject1(2), CreateTestObject2(2)) + Sum(CreateTestObject2(1), CreateTestObject1(5))");
        var sut = expression.ToLambda<Context, int>(cancellationToken);
        var context = new Context();

        await Assert.That(sut(context)).IsEqualTo(10);
    }

    [Test]
    public async Task ShouldHandleParamsKeyword(CancellationToken cancellationToken)
    {
        var expression = new Expression("Sum(Test(1,1),2)");
        var sut = expression.ToLambda<Context, int>(cancellationToken);
        var context = new Context();

        await Assert.That(sut(context)).IsEqualTo(4);
    }

    [Test]
    public async Task ShouldHandleMixedParamsKeyword(CancellationToken cancellationToken)
    {
        var expression = new Expression("Sum('Your total is: ', Test(1,1), 2, 3)");
        var sut = expression.ToLambda<Context, string>(cancellationToken);
        var context = new Context();

        await Assert.That(sut(context)).IsEqualTo("Your total is: 7");
    }

    [Test]
    public async Task ShouldHandleCustomFunctions(CancellationToken cancellationToken)
    {
        var expression = new Expression("Test(Test(1, 2), 3)");
        var sut = expression.ToLambda<Context, int>(cancellationToken);
        var context = new Context();

        await Assert.That(sut(context)).IsEqualTo(6);
    }

    [Test]
    public async Task ShouldHandleContextInheritance(CancellationToken cancellationToken)
    {
        var lambda1 = new Expression("Multiply(5, 2)").ToLambda<SubContext, int>(cancellationToken);
        var lambda2 = new Expression("Test(5, 5)").ToLambda<SubContext, int>(cancellationToken);
        var lambda3 = new Expression("Test(1,2,3,4)").ToLambda<SubContext, int>(cancellationToken);
        var lambda4 = new Expression("Sum(CreateTestObject1(100), CreateTestObject2(100), CreateTestObject2(100))")
            .ToLambda<SubContext, int>(cancellationToken);

        var context = new SubContext();
        await Assert.That(lambda1(context)).IsEqualTo(10);
        await Assert.That(lambda2(context)).IsEqualTo(5);
        await Assert.That(lambda3(context)).IsEqualTo(10);
        await Assert.That(lambda4(context)).IsEqualTo(400);
    }

    [Test]
    [Arguments("Test(1, 1, 1)")]
    [Arguments("Test(1.0, 1.0, 1.0)")]
    [Arguments("Test(1.0, 1, 1.0)")]
    public async Task ShouldHandleImplicitConversion(string input, CancellationToken cancellationToken)
    {
        var lambda = new Expression(input).ToLambda<Context, int>(cancellationToken);

        var context = new Context();
        await Assert.That(lambda(context)).IsEqualTo(3);
    }

    [Test]
    public void MissingMethod(CancellationToken cancellationToken)
    {
        var expression = new Expression("MissingMethod(1)");

        Assert.ThrowsExactly<MissingMethodException>(() =>
            expression.ToLambda<Context, int>(cancellationToken));
    }

    [Test]
    public async Task ShouldHandleTernaryOperator(CancellationToken cancellationToken)
    {
        var expression = new Expression("Test(1, 2) = 3 ? 1 : 2");
        var sut = expression.ToLambda<Context, int>(cancellationToken);
        var context = new Context();

        await Assert.That(sut(context)).IsEqualTo(1);
    }

    [Test]
    public async Task Issue1(CancellationToken cancellationToken)
    {
        var expr = new Expression("2 + 2 - a - b - x");

        const decimal x = 5m;
        const decimal a = 6m;
        const decimal b = 7m;

        expr.Parameters["x"] = x;
        expr.Parameters["a"] = a;
        expr.Parameters["b"] = b;

        var f = expr.ToLambda<float>(cancellationToken); // Here it throws System.ArgumentNullException. Parameter name: expression
        await Assert.That(f()).IsEqualTo(-14);
    }

    [Test]
    [Arguments("if(true, true, false)")]
    [Arguments("in(3, 1, 2, 3, 4)")]
    public async Task ShouldHandleBuiltInFunctions(string input, CancellationToken cancellationToken)
    {
        var expression = new Expression(input);
        var sut = expression.ToLambda<bool>(cancellationToken);
        await Assert.That(sut()).IsTrue();
    }

    [Test]
    [Arguments("Min(CreateTestObject1(1), CreateTestObject1(2))", 1)]
    [Arguments("Max(CreateTestObject1(1), CreateTestObject1(2))", 2)]
    [Arguments("Min(1, 2)", 1)]
    [Arguments("Max(1, 2)", 2)]
    public async Task ShouldProritiseContextFunctions(string input, double expected, CancellationToken cancellationToken)
    {
        var expression = new Expression(input);
        var lambda = expression.ToLambda<Context, double>(cancellationToken);
        var context = new Context();
        var actual = lambda(context);
        await Assert.That(actual).IsEqualTo(expected);
    }

    [Test]
    [Arguments("[FieldA] > [FieldC]", true)]
    [Arguments("[FieldC] > 1.34", true)]
    [Arguments("[FieldC] > (1.34 * 2) % 3", false)]
    [Arguments("[FieldE] = 2", true)]
    [Arguments("[FieldD] > 0", false)]
    public async Task ShouldHandleDataConversions(string input, bool expected, CancellationToken cancellationToken)
    {
        var expression = new Expression(input);
        var sut = expression.ToLambda<Context, bool>(cancellationToken);
        var context = new Context { FieldA = 7, FieldB = "test", FieldC = 2.4m, FieldE = 2 };

        await Assert.That(sut(context)).IsEqualTo(expected);
    }

    [Test]
    [Arguments("Min(3,2)", 2)]
    [Arguments("Min(3.2,6.3)", 3.2)]
    [Arguments("Max(2.6,9.6)", 9.6)]
    [Arguments("Max(9,6)", 9.0)]
    [Arguments("Pow(5,2)", 25)]
    public async Task ShouldHandleNumericBuiltInFunctions(string input, double expected, CancellationToken cancellationToken)
    {
        var expression = new Expression(input);
        var sut = expression.ToLambda<object>(cancellationToken);
        await Assert.That(sut()).IsEqualTo(expected);
    }

    [Test]
    [Arguments("if(true, 1, 0.0)", 1.0)]
    [Arguments("if(true, 1.0, 0)", 1.0)]
    [Arguments("if(true, 1.0, 0.0)", 1.0)]
    public async Task ShouldHandleFloatIfFunction(string input, double expected, CancellationToken cancellationToken)
    {
        var expression = new Expression(input);
        var sut = expression.ToLambda<object>(cancellationToken);
        await Assert.That(sut()).EqualTo(expected);
    }

    [Test]
    [Arguments("if(true, 1, 0)", 1)]
    public async Task ShouldHandleIntIfFunction(string input, int expected, CancellationToken cancellationToken)
    {
        var expression = new Expression(input);
        var sut = expression.ToLambda<object>(cancellationToken);
        await Assert.That(sut()).IsEqualTo(expected);
    }

    [Test]
    [Arguments("if(true, 'a', 'b')", "a")]
    public async Task ShouldHandleStringIfFunctionAsync(string input, string expected, CancellationToken cancellationToken)
    {
        var expression = new Expression(input);
        var sut = expression.ToLambda<object>(cancellationToken);
        await Assert.That(sut()).IsEqualTo(expected);
    }

    [Test]
    public async Task ShouldAllowValueTypeContexts(CancellationToken cancellationToken)
    {
        // Arrange
        const decimal expected = 6.908m;
        var expression = new Expression("Foo * 3.14");
        var sut = expression.ToLambda<FooStruct, decimal>(cancellationToken);
        var context = new FooStruct();

        // Act
        var actual = sut(context);

        // Assert
        await Assert.That(actual).IsEqualTo(expected);
    }

    // https://github.com/sklose/NCalc2/issues/54
    [Test]
    public async Task Issue54(CancellationToken cancellationToken)
    {
        // Arrange
        const long expected = 9999999999L;
        var expression = $"if(true, {expected}, 0)";
        var e = new Expression(expression);
        var context = new object();

        var lambda = e.ToLambda<object, long>(cancellationToken);

        // Act
        var actual = lambda(context);

        // Assert
        await Assert.That(actual).IsEqualTo(expected);
    }

    internal struct FooStruct
    {
        public double Foo => 2.2;
    }

    struct ContextAndResult
    {
        public double x;
        public double y;
        public string Func;
        public double ExpressionResult;
        public double LambdaResult;
    }

    [Test]
    public async Task ExpressionAndLambdaFuncBehaviorMatch(CancellationToken cancellationToken)
    {
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

        // Arrange
        double[] testValues = [double.MinValue, -Math.PI, -1, Math.BitDecrement(0), 0, Math.BitIncrement(0), 0.001, 1, 2, 3.14, Math.PI, 10, 100, double.MaxValue
        ];

        string[] functionsImplemented =
        [
            "Abs", "Acos", "Asin", "Atan", "Ceiling", "Cos", "Exp", "Floor", "IEEERemainder",
            "Log", "Log10", "Max", "Min", "Pow", "Round", "Sin", "Sqrt", "Tan", "Truncate"
        ];
        var functionsWithTwoArguments = new HashSet<string>
        {
            "Round", "IEEERemainder", "Log", "Max", "Min", "Pow"
        };

        var doubleToStringFormats = new[] { "F17", "0" };

        List<ContextAndResult> testResults = [];
        ContextAndResult currentContext = new();

        // Act
        try
        {
            // Iterate each function name with context
            foreach (var funcName in functionsImplemented)
            {
                var isTwoArgumentFunc = functionsWithTwoArguments.Contains(funcName);
                var expressionString = isTwoArgumentFunc ? funcName + "(x, y)" : funcName + "(x)";

                currentContext.Func = expressionString;

                var expression = new Expression(expressionString, CultureInfo.InvariantCulture);
                var lambda = expression.ToLambda<ContextAndResult, double>(cancellationToken);

                for (var i = 0; i < testValues.Length; ++i)
                {
                    currentContext.x = testValues[i];
                    expression.Parameters["x"] = currentContext.x;

                    if (isTwoArgumentFunc)
                    {
                        currentContext.y = testValues[(i + 1) % testValues.Length];
                        // Edge case (Round second argument is int decimal places to round from 0 to 15)
                        if (funcName == "Round")
                            currentContext.y = Math.Clamp(currentContext.y, 0, 15);
                        expression.Parameters["y"] = currentContext.y;
                    }

                    currentContext.ExpressionResult = (double)expression.Evaluate(cancellationToken)!;
                    currentContext.LambdaResult = lambda(currentContext);

                    testResults.Add(currentContext);
                }
            }

            // Iterate each function name without context with numbers of format 123.456 (doubles) or 123 (integers)
            foreach (var doubleToStringFormat in doubleToStringFormats)
            {
                for (var i = 0; i < testValues.Length; ++i)
                {
                    currentContext.x = testValues[i];
                    // Edge case (Exception when too big doubles not fit into Int64)
                    // We are multiplying by 0.99 because after clamping exception is still thrown
                    // Int64.MinValue = -9223372036854775808, (double)Int64.MinValue = -9223372036854780000 which is lesser)
                    if (doubleToStringFormat == "0" && Math.Abs(currentContext.x) > long.MaxValue)
                    {
                        currentContext.x = Math.Clamp(currentContext.x, long.MinValue, long.MaxValue) * 0.99;
                    }

                    var doubleParam1 = currentContext.x.ToString(doubleToStringFormat);

                    foreach (var funcName in functionsImplemented)
                    {
                        var isTwoArgumentFunc = functionsWithTwoArguments.Contains(funcName);

                        string expressionString;

                        if (isTwoArgumentFunc)
                        {
                            currentContext.y = testValues[(i + 1) % testValues.Length];
                            // Edge case (see previous)
                            if (doubleToStringFormat == "0" && Math.Abs(currentContext.y) > long.MaxValue)
                            {
                                currentContext.y = Math.Clamp(currentContext.y, long.MinValue, long.MaxValue) * 0.99;
                            }
                            // Edge case (Round second argument is int decimal places to round from 0 to 15)
                            if (funcName == "Round")
                                currentContext.y = Math.Clamp(currentContext.y, 0, 15);

                            var doubleParam2 = currentContext.y.ToString(doubleToStringFormat);
                            expressionString = funcName + $"({doubleParam1}, {doubleParam2})";
                        }
                        else
                        {
                            expressionString = funcName + $"({doubleParam1})";
                        }
                        currentContext.Func = expressionString;

                        var expression = new Expression(expressionString);
                        var lambda = expression.ToLambda<double>(cancellationToken);

                        currentContext.ExpressionResult = Convert.ToDouble(expression.Evaluate(cancellationToken));
                        currentContext.LambdaResult = lambda();
                        testResults.Add(currentContext);
                    }
                }
            }
        }
        // Serves to find an exact spot of error. Change Exception to non-related (for ex. OutOfMemoryException) to navigate via links in Test Explorer
        catch (Exception ex)
        {
            Assert.Fail($"""
                         {ex.GetType()}, context x: {currentContext.x},
                                             func: {currentContext.Func},
                                             Expression result: {currentContext.ExpressionResult},
                                             Lambda result: {currentContext.LambdaResult}
                                             exception message: {ex.Message}
                                             exception stack: {ex.StackTrace}
                         """);
        }

        // Assert
        foreach (var testContext in testResults)
        {
            await Assert.That(IsEqual(testContext.ExpressionResult, testContext.LambdaResult)).IsTrue();
        }

        return;

        static bool IsEqual(double a, double b)
        {
            if (double.IsNaN(a) && double.IsNaN(b))
            {
                return true;
            }

            return a == b;
            //double tol = Math.Clamp(Math.Max(a, b) * 1e-12, 1e-12, 1);
            //return Math.Abs(a - b) < tol;
        }
    }

    struct ContextWithOverridenMethods
    {
        public double x;
        public double y;

        public double Cos(double val) => Math.Sin(val) + 1;

        public double Log(double val) => Math.Sin(val) + 2;

        public double Log(double val1, double val2) => Math.Sin(val1) + 3;
    }

    [Test]
    public async Task LambdaOverrideMathFunction(CancellationToken cancellationToken)
    {
        // Arrange
        ContextWithOverridenMethods context = new() { x = 3.5, y = 2.5 };

        // Not overriden function
        var expressionAbs = new Expression("Abs(x)");
        var lambdaAbs = expressionAbs.ToLambda<ContextWithOverridenMethods, double>(cancellationToken);

        // Overriden functions
        var expressionCos = new Expression("Cos(x)");
        var lambdaCos = expressionCos
            .ToLambda<ContextWithOverridenMethods, double>(cancellationToken);

        var expressionLog1 = new Expression("Log(x)");
        var lambdaLog1 = expressionLog1
            .ToLambda<ContextWithOverridenMethods, double>(cancellationToken);

        var expressionLog2 = new Expression("Log(x, y)");
        var lambdaLog2 = expressionLog2
            .ToLambda<ContextWithOverridenMethods, double>(cancellationToken);

        // Act
        var actualAbs = lambdaAbs(context);
        var expectedAbs = Math.Abs(context.x);

        var actualCos = lambdaCos(context);
        var expectedCos = context.Cos(context.x);

        var actualLog1 = lambdaLog1(context);
        var expectedLog1 = context.Log(context.x);

        var actualLog2 = lambdaLog2(context);
        var expectedLog2 = context.Log(context.x, context.y);

        // Assert
        await Assert.That(actualAbs).IsEqualTo(expectedAbs);
        await Assert.That(actualCos).IsEqualTo(expectedCos);
        await Assert.That(actualLog1).IsEqualTo(expectedLog1);
        await Assert.That(actualLog2).IsEqualTo(expectedLog2);
    }

    [Test]
    [Arguments(int.MaxValue, '+', int.MaxValue)]
    [Arguments(int.MinValue, '-', int.MaxValue)]
    [Arguments(int.MaxValue, '*', int.MaxValue)]
    public void ShouldHandleOverflowInt(int a, char op, int b, CancellationToken cancellationToken)
    {
        var e = new Expression($"[a] {op} [b]", ExpressionOptions.OverflowProtection, CultureInfo.InvariantCulture);
        e.Parameters["a"] = a;
        e.Parameters["b"] = b;

        var lambda = e.ToLambda<int>(cancellationToken);

        Assert.Throws<OverflowException>(() => lambda());
    }

    [Test]
    public async Task ShouldAllowPowWithDecimals(CancellationToken cancellationToken)
    {
        var e = new Expression("Pow(3.1, 2)", ExpressionOptions.DecimalAsDefault, CultureInfo.InvariantCulture);
        var lambda = e.ToLambda<decimal>(cancellationToken);
        await Assert.That(lambda()).IsEqualTo(9.61m);
    }

    [Test]
    public async Task ShouldUseDecimalsWithDecimalAsDefault(CancellationToken cancellationToken)
    {
        const decimal val = 3.1m;

        // Arrange
        var expressionAbs = new Expression("Abs(x)", ExpressionOptions.DecimalAsDefault);
        expressionAbs.Parameters["x"] = val;
        var lambdaAbs = expressionAbs.ToLambda<decimal>(cancellationToken);

        var expressionCeiling = new Expression("Ceiling(x)", ExpressionOptions.DecimalAsDefault);
        expressionCeiling.Parameters["x"] = val;
        var lambdaCeiling = expressionCeiling.ToLambda<decimal>(cancellationToken);

        var expressionFloor = new Expression("Floor(x)", ExpressionOptions.DecimalAsDefault);
        expressionFloor.Parameters["x"] = val;
        var lambdaFloor = expressionFloor.ToLambda<decimal>(cancellationToken);

        var expressionSign = new Expression("Sign(x)", ExpressionOptions.DecimalAsDefault);
        expressionSign.Parameters["x"] = val;
        var lambdaSign = expressionSign.ToLambda<decimal>(cancellationToken);

        var expressionTruncate = new Expression("Truncate(x)", ExpressionOptions.DecimalAsDefault);
        expressionTruncate.Parameters["x"] = val;
        var lambdaTruncate = expressionTruncate.ToLambda<decimal>(cancellationToken);

        // Act
        var actualAbs = lambdaAbs();
        var expectedAbs = Math.Abs(val);

        var actualCeiling = lambdaCeiling();
        var expectedCeiling = Math.Ceiling(val);

        var actualFloor = lambdaFloor();
        var expectedFloor = Math.Floor(val);

        var actualSign = lambdaSign();
        var expectedSign = Math.Sign(val);

        var actualTruncate = lambdaTruncate();
        var expectedTruncate = Math.Truncate(val);

        // Assert
        await Assert.That(actualAbs).IsEqualTo(expectedAbs);
        await Assert.That(actualCeiling).IsEqualTo(expectedCeiling);
        await Assert.That(actualFloor).IsEqualTo(expectedFloor);
        await Assert.That(actualSign).IsEqualTo(expectedSign);
        await Assert.That(actualTruncate).IsEqualTo(expectedTruncate);
    }

    [Test]
    [Arguments("test", "%est", true, ExpressionOptions.None)]
    [Arguments("TEST", "%est", false, ExpressionOptions.None)]
    [Arguments("TEST", "%est", true, ExpressionOptions.CaseInsensitiveStringComparer)]
    [Arguments("this is a test", "%is a%", true, ExpressionOptions.None)]
    [Arguments("this is a test", "%ITS A%", false, ExpressionOptions.None)]
    public async Task ShouldHandleLikeOperator(string val, string right, bool expected, ExpressionOptions opts, CancellationToken cancellationToken)
    {
        string[] ops = ["like", "not like"];
        // Arrange
        foreach (var op in ops)
        {
            if (op == "not like")
                expected = !expected;

            var expressionLike = new Expression($"x {op} '{right}'", opts);
            expressionLike.Parameters["x"] = val;

            var lambdaAbs = expressionLike.ToLambda<bool>(cancellationToken);

            // Act
            var actualAbs = lambdaAbs();
            var expectedAbs = expressionLike.Evaluate(cancellationToken);
            Console.WriteLine($"Value: {val}, Pattern: {right}, Result: {actualAbs}");

            // Assert
            await Assert.That(actualAbs).IsEqualTo(expected);
            await Assert.That(actualAbs).IsEqualTo((bool)expectedAbs);
        }
    }

    [Test]
    [Arguments("test", "'a','test','z'", true, ExpressionOptions.None)]
    [Arguments("TEST", "'a','test','z'", false, ExpressionOptions.None)]
    [Arguments("TEST", "'a','test','z'", true, ExpressionOptions.CaseInsensitiveStringComparer)]
    [Arguments("this", "'this','is','a','test'", true, ExpressionOptions.None)]
    [Arguments("THIS", "'this','is','a','test'", false, ExpressionOptions.None)]
    [Arguments("THIS", "'this','is','a','test'", true, ExpressionOptions.CaseInsensitiveStringComparer)]
    public async Task ShouldHandleInOperator_String(string val, string list, bool expected, ExpressionOptions opts, CancellationToken cancellationToken)
    {
        string[] ops = ["in", "not in"];

        foreach (var op in ops)
        {
            var exp = expected;
            if (op == "not in") exp = !exp;

            var expression = new Expression($"x {op} ({list})", opts);
            expression.Parameters["x"] = val;

            var lambda = expression.ToLambda<bool>(cancellationToken);

            var actual = lambda();
            var expectedEval = (bool)expression.Evaluate(cancellationToken);

            await Assert.That(actual).IsEqualTo(exp);
            await Assert.That(actual).IsEqualTo(expectedEval);
        }
    }

    [Test]
    [Arguments("test", new[] { "a", "test", "z" }, true)]
    [Arguments("nope", new[] { "a", "test", "z" }, false)]
    [Arguments("x", new string[] { }, false)] // empty list
    [Arguments("dup", new[] { "dup", "dup" }, true)]  // duplicates
    [Arguments("Test", new[] { "a", "test", "z" }, false)] // case sensitivity
    public async Task ShouldHandleInOperatorWithVariables_String(string x, string[] y, bool expected, CancellationToken cancellationToken)
    {
        var expression = new Expression("x in y", ExpressionOptions.None);
        expression.Parameters["x"] = x;
        expression.Parameters["y"] = y; // string[] is fine

        var lambda = expression.ToLambda<bool>(cancellationToken);

        var actual = lambda();
        var expectedEval = (bool)expression.Evaluate(cancellationToken);

        await Assert.That(actual).IsEqualTo(expected);
        await Assert.That(actual).IsEqualTo(expectedEval);
    }

    [Test]
    [Arguments(3, new[] { 1, 2, 3 }, true)]
    [Arguments(4, new[] { 1, 2, 3 }, false)]
    [Arguments(7, new int[] { }, false)] // empty list
    public async Task ShouldHandleInOperatorWithVariables_Int(int x, int[] y, bool expected, CancellationToken cancellationToken)
    {
        var expression = new Expression("x in y", ExpressionOptions.None);
        expression.Parameters["x"] = x;
        // NCalc is happy with arrays; if needed you can box: y.Cast<object>().ToArray()
        expression.Parameters["y"] = y;

        var lambda = expression.ToLambda<bool>(cancellationToken);

        var actual = lambda();

        await Assert.That(expected).IsEqualTo(actual);
    }

    [Test]
    public async Task ShouldHandleInOperatorWithVariables_ListInt(CancellationToken cancellationToken)
    {
        var expression = new Expression("x in y", ExpressionOptions.None);
        expression.Parameters["x"] = 3;
        // NCalc is happy with arrays; if needed you can box: y.Cast<object>().ToArray()
        expression.Parameters["y"] = new List<int> { 1, 2, 3 };

        var lambda = expression.ToLambda<bool>(cancellationToken);

        var actual = lambda();

        await Assert.That(actual).IsTrue();
    }

    [Test]
    [Arguments(3.0, new[] { 1.0, 2.0, 3.0 }, true)]
    [Arguments(3.0, new[] { 1.1, 2.2, 3.3 }, false)]
    public async Task ShouldHandleInOperatorWithVariables_Double(double x, double[] y, bool expected, CancellationToken cancellationToken)
    {
        var expression = new Expression("x in y", ExpressionOptions.None);
        expression.Parameters["x"] = x;
        expression.Parameters["y"] = y;

        var lambda = expression.ToLambda<bool>(cancellationToken);

        var actual = lambda();

        await Assert.That(actual).IsEqualTo(expected);
    }
}
#endif