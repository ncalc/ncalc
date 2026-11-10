#if NET8_0_OR_GREATER
#nullable enable
using NCalc.LambdaCompilation;
using NCalc.Tests.Attributes;
using System.Threading.Tasks;

// ReSharper disable MemberCanBeProtected.Local

namespace NCalc.Tests;

[SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Local", Justification = "Reflection")]
[SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Reflection")]
[SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Reflection")]
[SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Style")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local", Justification = "Reflection")]
[Property("Category", "Lambdas")]
[SkipInNativeAot]
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

        public double If(bool condition, double trueRes, double falseRes)
        {
            return condition ? trueRes : falseRes;
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
    public async Task ShouldHandleParameters()
    {
        var expression = new Expression("[FieldA] > 5 && [FieldB] = 'test'");
        var sut = expression.ToLambda<Context, bool>(CancellationToken.None);
        var context = new Context { FieldA = 7, FieldB = "test" };

        await Assert.That(sut(context)).IsTrue();
    }

    [Test]
    public async Task ShouldHandleCoalesceOperator()
    {
        var referenceExpression = new Expression("[FieldB] ?? 'fallback'");
        var nullableExpression = new Expression("[FieldE] ?? 42");
        var referenceLambda = referenceExpression.ToLambda<Context, string>(CancellationToken.None);
        var nullableLambda = nullableExpression.ToLambda<Context, int>(CancellationToken.None);

        await Assert.That(referenceLambda(new Context())).IsEqualTo("fallback");
        await Assert.That(referenceLambda(new Context { FieldB = "value" })).IsEqualTo("value");
        await Assert.That(nullableLambda(new Context())).IsEqualTo(42);
        await Assert.That(nullableLambda(new Context { FieldE = 7 })).IsEqualTo(7);
    }

    [Test]
    public async Task ShouldHandleOverloadingSameParamCount()
    {
        var expression = new Expression("Test('Hello', ' world!')");
        var sut = expression.ToLambda<Context, string>(CancellationToken.None);
        var context = new Context();

        await Assert.That(sut(context)).IsEqualTo("Hello world!");
    }

    [Test]
    public async Task ShouldHandleOverloadingDifferentParamCount()
    {
        var expression = new Expression("Test(Test(1, 2), 3, 4)");
        var sut = expression.ToLambda<Context, int>(CancellationToken.None);
        var context = new Context();

        await Assert.That(sut(context)).IsEqualTo(10);
    }

    [Test]
    public async Task ShouldHandleOverloadingObjectParameters()
    {
        var expression = new Expression("Sum(CreateTestObject1(2), CreateTestObject2(2)) + Sum(CreateTestObject2(1), CreateTestObject1(5))");
        var sut = expression.ToLambda<Context, int>(CancellationToken.None);
        var context = new Context();

        await Assert.That(sut(context)).IsEqualTo(10);
    }

    [Test]
    public async Task ShouldHandleParamsKeyword()
    {
        var expression = new Expression("Sum(Test(1,1),2)");
        var sut = expression.ToLambda<Context, int>(CancellationToken.None);
        var context = new Context();

        await Assert.That(sut(context)).IsEqualTo(4);
    }

    [Test]
    public async Task ShouldHandleMixedParamsKeyword()
    {
        var expression = new Expression("Sum('Your total is: ', Test(1,1), 2, 3)");
        var sut = expression.ToLambda<Context, string>(CancellationToken.None);
        var context = new Context();

        await Assert.That(sut(context)).IsEqualTo("Your total is: 7");
    }

    [Test]
    public async Task ShouldHandleCustomFunctions()
    {
        var expression = new Expression("Test(Test(1, 2), 3)");
        var sut = expression.ToLambda<Context, int>(CancellationToken.None);
        var context = new Context();

        await Assert.That(sut(context)).IsEqualTo(6);
    }

    [Test]
    public async Task ShouldHandleContextInheritance()
    {
        var lambda1 = new Expression("Multiply(5, 2)").ToLambda<SubContext, int>(CancellationToken.None);
        var lambda2 = new Expression("Test(5, 5)").ToLambda<SubContext, int>(CancellationToken.None);
        var lambda3 = new Expression("Test(1,2,3,4)").ToLambda<SubContext, int>(CancellationToken.None);
        var lambda4 = new Expression("Sum(CreateTestObject1(100), CreateTestObject2(100), CreateTestObject2(100))")
            .ToLambda<SubContext, int>(CancellationToken.None);

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
    public async Task ShouldHandleImplicitConversion(string input)
    {
        var lambda = new Expression(input).ToLambda<Context, int>(CancellationToken.None);

        var context = new Context();
        await Assert.That(lambda(context)).IsEqualTo(3);
    }

    [Test]
    public async Task MissingMethod()
    {
        var expression = new Expression("MissingMethod(1)");
        try
        {
            _ = expression.ToLambda<Context, int>(CancellationToken.None);
        }
        catch (MissingMethodException ex)
        {
            System.Diagnostics.Debug.Write(ex);
            await Assert.That(true).IsTrue();
            return;
        }
        await Assert.That(false).IsTrue();
    }

    [Test]
    public async Task ShouldHandleTernaryOperator()
    {
        var expression = new Expression("Test(1, 2) = 3 ? 1 : 2");
        var sut = expression.ToLambda<Context, int>(CancellationToken.None);
        var context = new Context();

        await Assert.That(sut(context)).IsEqualTo(1);
    }

    [Test]
    public async Task ShouldHandleNullCheckBuiltInFunctions()
    {
        var isNull = new Expression("isNull([FieldB])").ToLambda<Context, bool>(CancellationToken.None);
        var isNullOrEmpty = new Expression("isNullOrEmpty([FieldB])")
            .ToLambda<Context, bool>(CancellationToken.None);

        await Assert.That(isNull(new Context())).IsTrue();
        await Assert.That(isNullOrEmpty(new Context())).IsTrue();
        await Assert.That(isNullOrEmpty(new Context { FieldB = string.Empty })).IsTrue();
        await Assert.That(isNullOrEmpty(new Context { FieldB = "NCalc" })).IsFalse();
    }

    [Test]
    [Arguments("Min(CreateTestObject1(1), CreateTestObject1(2))", 1)]
    [Arguments("Max(CreateTestObject1(1), CreateTestObject1(2))", 2)]
    [Arguments("Min(1, 2)", 1)]
    [Arguments("Max(1, 2)", 2)]
    [Arguments("If(true, 3, 2)", 3)]
    public async Task ShouldProritiseContextFunctions(string input, double expected)
    {
        var expression = new Expression(input);
        var lambda = expression.ToLambda<Context, double>(CancellationToken.None);
        var context = new Context();
        var actual = lambda(context);
        await Assert.That(actual).IsEqualTo(expected);
    }

    [Test]
    [Arguments("ABS(-1)", 1)]
    [Arguments("ROUND(1.32, 1)", 1.3)]
    public async Task ShouldIgnoreCaseAtBuiltInFunctions(string input, double expected)
    {
        var expression = new Expression(input, ExpressionOptions.IgnoreCaseAtBuiltInFunctions);
        var lambda = expression.ToLambda<Context, double>(CancellationToken.None);
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
    public async Task ShouldHandleDataConversions(string input, bool expected)
    {
        var expression = new Expression(input);
        var sut = expression.ToLambda<Context, bool>(CancellationToken.None);
        var context = new Context { FieldA = 7, FieldB = "test", FieldC = 2.4m, FieldE = 2 };

        await Assert.That(sut(context)).IsEqualTo(expected);
    }

    [Test]
    public async Task ShouldAllowValueTypeContexts()
    {
        // Arrange
        const decimal expected = 6.908m;
        var expression = new Expression("Foo * 3.14");
        var sut = expression.ToLambda<FooStruct, decimal>(CancellationToken.None);
        var context = new FooStruct();

        // Act
        var actual = sut(context);

        // Assert
        await Assert.That(actual).IsEqualTo(expected);
    }

    // https://github.com/sklose/NCalc2/issues/54
    [Test]
    public async Task Issue54()
    {
        // Arrange
        const long expected = 9999999999L;
        var expression = $"if(true, {expected}, 0)";
        var e = new Expression(expression);
        var context = new object();

        var lambda = e.ToLambda<object, long>(CancellationToken.None);

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
    public async Task ExpressionAndLambdaFuncBehaviorMatch()
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
                var lambda = expression.ToLambda<ContextAndResult, double>(CancellationToken.None);

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

                    currentContext.ExpressionResult = (double)expression.Evaluate(CancellationToken.None)!;
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
                        var lambda = expression.ToLambda<double>(CancellationToken.None);

                        currentContext.ExpressionResult = Convert.ToDouble(expression.Evaluate(CancellationToken.None));
                        currentContext.LambdaResult = lambda();
                        testResults.Add(currentContext);
                    }
                }
            }
        }
        // Serves to find an exact spot of error. Change Exception to non-related (for ex. OutOfMemoryException) to navigate via links in Test Explorer
        catch (Exception ex)
        {
            Assert.Fail("Assertion failure");
        }

        // Assert
        foreach (var testContext in testResults)
        {
            await Assert.That(IsEqual(testContext.ExpressionResult, testContext.LambdaResult)).IsTrue().Because($"""
                 context x: {testContext.x},
                                     func: {testContext.Func},
                                     Expression result: {testContext.ExpressionResult},
                                     Lambda result: {testContext.LambdaResult}
                 """);
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
    public async Task LambdaOverrideMathFunction()
    {
        // Arrange
        ContextWithOverridenMethods context = new() { x = 3.5, y = 2.5 };

        // Not overriden function
        var expressionAbs = new Expression("Abs(x)");
        var lambdaAbs = expressionAbs.ToLambda<ContextWithOverridenMethods, double>(CancellationToken.None);

        // Overriden functions
        var expressionCos = new Expression("Cos(x)");
        var lambdaCos = expressionCos
            .ToLambda<ContextWithOverridenMethods, double>(CancellationToken.None);

        var expressionLog1 = new Expression("Log(x)");
        var lambdaLog1 = expressionLog1
            .ToLambda<ContextWithOverridenMethods, double>(CancellationToken.None);

        var expressionLog2 = new Expression("Log(x, y)");
        var lambdaLog2 = expressionLog2
            .ToLambda<ContextWithOverridenMethods, double>(CancellationToken.None);

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
}
#endif
