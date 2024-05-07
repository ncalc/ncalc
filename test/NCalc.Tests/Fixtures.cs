using System;
using NCalc.Domain;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Collections;
using System.Globalization;
using System.Linq;
using BinaryExpression = NCalc.Domain.BinaryExpression;
using UnaryExpression = NCalc.Domain.UnaryExpression;
using Newtonsoft.Json;

namespace NCalc.Tests;

[TestClass]
public class Fixtures
{
    /// <summary>
    ///Gets or sets the test context which provides
    ///information about and functionality for the current test run.
    ///</summary>
    public TestContext TestContext { get; set; }

    [TestMethod]
    public void ExpressionShouldEvaluate()
    {
        var expressions = new []
        {
            "2 + 3 + 5",
            "2 * 3 + 5",
            "2 * (3 + 5)",
            "2 * (2*(2*(2+1)))",
            "10 % 3",
            "true or false",
            "not true",
            "false || not (false and true)",
            "3 > 2 and 1 <= (3-2)",
            "3 % 2 != 10 % 3"
        };

        foreach (string expression in expressions)
            Console.WriteLine("{0} = {1}",
                expression,
                new Expression(expression).Evaluate());
    }

    [TestMethod]
    public void ShouldParseValues()
    {
        Assert.AreEqual(123456, new Expression("123456").Evaluate());
        Assert.AreEqual(new DateTime(2001, 01, 01), new Expression("#01/01/2001#").Evaluate());
        Assert.AreEqual(0.2d, new Expression(".2").Evaluate());
        Assert.AreEqual(123.456d, new Expression("123.456").Evaluate());
        Assert.AreEqual(123d, new Expression("123.").Evaluate());
        Assert.AreEqual(12300d, new Expression("123.E2").Evaluate());
        Assert.AreEqual(true, new Expression("true").Evaluate());
        Assert.AreEqual("true", new Expression("'true'").Evaluate());
        Assert.AreEqual("azerty", new Expression("'azerty'").Evaluate());
    }

    [TestMethod]
    public void ShouldHandleUnicode()
    {
        Assert.AreEqual("経済協力開発機構", new Expression("'経済協力開発機構'").Evaluate());
        Assert.AreEqual("Hello", new Expression(@"'\u0048\u0065\u006C\u006C\u006F'").Evaluate());
        Assert.AreEqual("だ", new Expression(@"'\u3060'").Evaluate());
        Assert.AreEqual("\u0100", new Expression(@"'\u0100'").Evaluate());
    }

    [TestMethod]
    public void ShouldEscapeCharacters()
    {
        Assert.AreEqual("'hello'", new Expression(@"'\'hello\''").Evaluate());
        Assert.AreEqual(" ' hel lo ' ", new Expression(@"' \' hel lo \' '").Evaluate());
        Assert.AreEqual("hel\nlo", new Expression(@"'hel\nlo'").Evaluate());
    }

    [TestMethod]
    public void ShouldDisplayErrorMessages()
    {
        try
        {
            new Expression("(3 + 2").Evaluate();
            Assert.Fail();
        }
        catch(EvaluationException e)
        {
            Console.WriteLine("Error catched: " + e.Message);
        }
    }

    [TestMethod]
    public void Maths()
    {
        Assert.AreEqual(1M, new Expression("Abs(-1)").Evaluate());
        Assert.AreEqual(0d, new Expression("Acos(1)").Evaluate());
        Assert.AreEqual(0d, new Expression("Asin(0)").Evaluate());
        Assert.AreEqual(0d, new Expression("Atan(0)").Evaluate());
        Assert.AreEqual(2d, new Expression("Ceiling(1.5)").Evaluate());
        Assert.AreEqual(1d, new Expression("Cos(0)").Evaluate());
        Assert.AreEqual(1d, new Expression("Exp(0)").Evaluate());
        Assert.AreEqual(1d, new Expression("Floor(1.5)").Evaluate());
        Assert.AreEqual(-1d, new Expression("IEEERemainder(3,2)").Evaluate());
        Assert.AreEqual(0d, new Expression("Log(1,10)").Evaluate());
        Assert.AreEqual(0d, new Expression("Ln(1)").Evaluate());
        Assert.AreEqual(0d, new Expression("Log10(1)").Evaluate());
        Assert.AreEqual(9d, new Expression("Pow(3,2)").Evaluate());
        Assert.AreEqual(3.22d, new Expression("Round(3.222,2)").Evaluate());
        Assert.AreEqual(-1, new Expression("Sign(-10)").Evaluate());
        Assert.AreEqual(0d, new Expression("Sin(0)").Evaluate());
        Assert.AreEqual(2d, new Expression("Sqrt(4)").Evaluate());
        Assert.AreEqual(0d, new Expression("Tan(0)").Evaluate());
        Assert.AreEqual(1d, new Expression("Truncate(1.7)").Evaluate());
        Assert.AreEqual(-Math.PI/2, (double) new Expression("Atan2(-1,0)").Evaluate(), 1e-16);
        Assert.AreEqual(Math.PI/2, (double) new Expression("Atan2(1,0)").Evaluate(), 1e-16);
        Assert.AreEqual(Math.PI, (double) new Expression("Atan2(0,-1)").Evaluate(), 1e-16);
        Assert.AreEqual(0, (double) new Expression("Atan2(0,1)").Evaluate(), 1e-16);
        Assert.AreEqual(10, new Expression("Max(1,10)").Evaluate());
        Assert.AreEqual(1, new Expression("Min(1,10)").Evaluate());
    }

    [TestMethod]
    public void ShouldHandleTrailingDecimalPoint()
    {
        Assert.AreEqual(3.0, new Expression("1. + 2.").Evaluate());
    }

    [TestMethod]
    public void ExpressionShouldEvaluateCustomFunctions()
    {
        var e = new Expression("SecretOperation(3, 6)");

        e.EvaluateFunction += delegate(string name, FunctionArgs args)
        {
            if (name == "SecretOperation")
                args.Result = (int)args.Parameters[0].Evaluate() + (int)args.Parameters[1].Evaluate();
        };

        Assert.AreEqual(9, e.Evaluate());
    }

    [TestMethod]
    public void ExpressionShouldEvaluateCustomFunctionsWithParameters()
    {
        var e = new Expression("SecretOperation([e], 6) + f");
        e.Parameters["e"] = 3;
        e.Parameters["f"] = 1;

        e.EvaluateFunction += delegate(string name, FunctionArgs args)
        {
            if (name == "SecretOperation")
                args.Result = (int)args.Parameters[0].Evaluate() + (int)args.Parameters[1].Evaluate();
        };

        Assert.AreEqual(10, e.Evaluate());
    }

    [TestMethod]
    public void ExpressionShouldEvaluateParameters()
    {
        var e = new Expression("Round(Pow(Pi, 2) + Pow([Pi Squared], 2) + [X], 2)");

        e.Parameters["Pi Squared"] = new Expression("Pi * [Pi]");
        e.Parameters["X"] = 10;

        e.EvaluateParameter += delegate(string name, ParameterArgs args)
        {
            if (name == "Pi")
                args.Result = 3.14;
        };

        Assert.AreEqual(117.07, e.Evaluate());
    }

    [TestMethod]
    public void ShouldEvaluateConditional()
    {
        var eif = new Expression("if([divider] <> 0, [divided] / [divider], 0)");
        eif.Parameters["divider"] = 5;
        eif.Parameters["divided"] = 5;

        Assert.AreEqual(1d, eif.Evaluate());

        eif = new Expression("if([divider] <> 0, [divided] / [divider], 0)");
        eif.Parameters["divider"] = 0;
        eif.Parameters["divided"] = 5;
        Assert.AreEqual(0, eif.Evaluate());
    }

    [TestMethod]
    public void ShouldOverrideExistingFunctions()
    {
        var e = new Expression("Round(1.99, 2)");

        Assert.AreEqual(1.99d, e.Evaluate());

        e.EvaluateFunction += delegate(string name, FunctionArgs args)
        {
            if (name == "Round")
                args.Result = 3;
        };

        Assert.AreEqual(3, e.Evaluate());
    }

    [TestMethod]
    public void ShouldEvaluateInOperator()
    {
        // The last argument should not be evaluated
        var ein = new Expression("in((2 + 2), [1], [2], 1 + 2, 4, 1 / 0)");
        ein.Parameters["1"] = 2;
        ein.Parameters["2"] = 5;

        Assert.AreEqual(true, ein.Evaluate());

        var eout = new Expression("in((2 + 2), [1], [2], 1 + 2, 3)");
        eout.Parameters["1"] = 2;
        eout.Parameters["2"] = 5;

        Assert.AreEqual(false, eout.Evaluate());

        // Should work with strings
        var estring = new Expression("in('to' + 'to', 'titi', 'toto')");

        Assert.AreEqual(true, estring.Evaluate());

    }

    [TestMethod]
    public void ShouldEvaluateOperators()
    {
        var expressions = new Dictionary<string, object>
        {
            {"!true", false},
            {"not false", true},
            {"Not false", true},
            {"NOT false", true},
            {"-10", -10},
            {"+20", 20},
            {"2**-1", 0.5},
            {"2**+2", 4.0},
            {"2 * 3", 6},
            {"6 / 2", 3d},
            {"7 % 2", 1},
            {"2 + 3", 5},
            {"2 - 1", 1},
            {"1 < 2", true},
            {"1 > 2", false},
            {"1 <= 2", true},
            {"1 <= 1", true},
            {"1 >= 2", false},
            {"1 >= 1", true},
            {"1 = 1", true},
            {"1 == 1", true},
            {"1 != 1", false},
            {"1 <> 1", false},
            {"1 & 1", 1},
            {"1 | 1", 1},
            {"1 ^ 1", 0},
            {"~1", ~1},
            {"2 >> 1", 1},
            {"2 << 1", 4},
            {"true && false", false},
            {"True and False", false},
            {"tRue aNd faLse", false},
            {"TRUE ANd fALSE", false},
            {"true AND FALSE", false},
            {"true || false", true},
            {"true or false", true},
            {"true Or false", true},
            {"true OR false", true},
            {"if(true, 0, 1)", 0},
            {"if(false, 0, 1)", 1}
        };

        foreach (KeyValuePair<string, object> pair in expressions)
        {
            Assert.AreEqual(pair.Value, new Expression(pair.Key).Evaluate(), pair.Key + " failed");
        }

    }

    [TestMethod]
    public void ShouldHandleOperatorsPriority()
    {
        Assert.AreEqual(8, new Expression("2+2+2+2").Evaluate());
        Assert.AreEqual(16, new Expression("2*2*2*2").Evaluate());
        Assert.AreEqual(6, new Expression("2*2+2").Evaluate());
        Assert.AreEqual(6, new Expression("2+2*2").Evaluate());

        Assert.AreEqual(9d, new Expression("1 + 2 + 3 * 4 / 2").Evaluate());
        Assert.AreEqual(13.5, new Expression("18/2/2*3").Evaluate());

        Assert.AreEqual(-1d, new Expression("-1 ** 2").Evaluate());
        Assert.AreEqual(1d, new Expression("(-1) ** 2").Evaluate());
        Assert.AreEqual(512d, new Expression("2 ** 3 ** 2").Evaluate());
        Assert.AreEqual(64d, new Expression("(2 ** 3) ** 2").Evaluate());
        Assert.AreEqual(18d, new Expression("2 * 3 ** 2").Evaluate());
        Assert.AreEqual(8d, new Expression("2 ** 4 / 2").Evaluate());
    }

    [TestMethod]
    public void ShouldNotLoosePrecision()
    {
        Assert.AreEqual(0.5, new Expression("3/6").Evaluate());
    }

    [TestMethod]
    public void ShouldThrowAnExceptionWhenInvalidNumber()
    {
        try
        {
            new Expression(". + 2").Evaluate();
            Assert.Fail();
        }
        catch (EvaluationException e)
        {
            Console.WriteLine("Error catched: " + e.Message);
        }
    }

    [TestMethod]
    public void ShouldNotRoundDecimalValues()
    {
        Assert.AreEqual(false, new Expression("0 <= -0.6").Evaluate());
    }

    [TestMethod]
    public void ShouldEvaluateTernaryExpression()
    {
        Assert.AreEqual(1, new Expression("1+2<3 ? 3+4 : 1").Evaluate());
    }

    [TestMethod]
    public void ShouldSerializeExpression()
    {
        Assert.AreEqual("True and False", new BinaryExpression(BinaryExpressionType.And, new ValueExpression(true), new ValueExpression(false)).ToString());
        Assert.AreEqual("1 / 2", new BinaryExpression(BinaryExpressionType.Div, new ValueExpression(1), new ValueExpression(2)).ToString());
        Assert.AreEqual("1 = 2", new BinaryExpression(BinaryExpressionType.Equal, new ValueExpression(1), new ValueExpression(2)).ToString());
        Assert.AreEqual("1 > 2", new BinaryExpression(BinaryExpressionType.Greater, new ValueExpression(1), new ValueExpression(2)).ToString());
        Assert.AreEqual("1 >= 2", new BinaryExpression(BinaryExpressionType.GreaterOrEqual, new ValueExpression(1), new ValueExpression(2)).ToString());
        Assert.AreEqual("1 < 2", new BinaryExpression(BinaryExpressionType.Lesser, new ValueExpression(1), new ValueExpression(2)).ToString());
        Assert.AreEqual("1 <= 2", new BinaryExpression(BinaryExpressionType.LesserOrEqual, new ValueExpression(1), new ValueExpression(2)).ToString());
        Assert.AreEqual("1 - 2", new BinaryExpression(BinaryExpressionType.Minus, new ValueExpression(1), new ValueExpression(2)).ToString());
        Assert.AreEqual("1 % 2", new BinaryExpression(BinaryExpressionType.Modulo, new ValueExpression(1), new ValueExpression(2)).ToString());
        Assert.AreEqual("1 != 2", new BinaryExpression(BinaryExpressionType.NotEqual, new ValueExpression(1), new ValueExpression(2)).ToString());
        Assert.AreEqual("True or False", new BinaryExpression(BinaryExpressionType.Or, new ValueExpression(true), new ValueExpression(false)).ToString());
        Assert.AreEqual("1 + 2", new BinaryExpression(BinaryExpressionType.Plus, new ValueExpression(1), new ValueExpression(2)).ToString());
        Assert.AreEqual("1 * 2", new BinaryExpression(BinaryExpressionType.Times, new ValueExpression(1), new ValueExpression(2)).ToString());

        Assert.AreEqual("-(True and False)",new UnaryExpression(UnaryExpressionType.Negate, new BinaryExpression(BinaryExpressionType.And, new ValueExpression(true), new ValueExpression(false))).ToString());
        Assert.AreEqual("!(True and False)",new UnaryExpression(UnaryExpressionType.Not, new BinaryExpression(BinaryExpressionType.And, new ValueExpression(true), new ValueExpression(false))).ToString());

        Assert.AreEqual("test(True and False, -(True and False))",new Function(new Identifier("test"), new LogicalExpression[] { new BinaryExpression(BinaryExpressionType.And, new ValueExpression(true), new ValueExpression(false)), new UnaryExpression(UnaryExpressionType.Negate, new BinaryExpression(BinaryExpressionType.And, new ValueExpression(true), new ValueExpression(false))) }).ToString());

        Assert.AreEqual("True", new ValueExpression(true).ToString());
        Assert.AreEqual("False", new ValueExpression(false).ToString());
        Assert.AreEqual("1", new ValueExpression(1).ToString());
        Assert.AreEqual("1.234", new ValueExpression(1.234).ToString());
        Assert.AreEqual("'hello'", new ValueExpression("hello").ToString());
        Assert.AreEqual("#" + new DateTime(2009, 1, 1) + "#", new ValueExpression(new DateTime(2009, 1, 1)).ToString());

        Assert.AreEqual("Sum(1 + 2)", new Function(new Identifier("Sum"), new [] { new BinaryExpression(BinaryExpressionType.Plus, new ValueExpression(1), new ValueExpression(2))}).ToString());
    }

    [TestMethod]
    public void ShouldHandleStringConcatenation()
    {
        Assert.AreEqual("toto", new Expression("'to' + 'to'").Evaluate());
        Assert.AreEqual("one2", new Expression("'one' + 2").Evaluate());
        Assert.AreEqual(3M, new Expression("1 + '2'").Evaluate());
    }

    [TestMethod]
    public void ShouldDetectSyntaxErrorsBeforeEvaluation()
    {
        var e = new Expression("a + b * (");
        Assert.IsNull(e.Error);
        Assert.IsTrue(e.HasErrors());
        Assert.IsTrue(e.HasErrors());
        Assert.IsNotNull(e.Error);

        e = new Expression("* b ");
        Assert.IsNull(e.Error);
        Assert.IsTrue(e.HasErrors());
        Assert.IsNotNull(e.Error);
    }

    [TestMethod]
    public void ShouldReuseCompiledExpressionsInMultiThreadedMode()
    {
        // Repeats the tests n times
        for (int cpt = 0; cpt < 20; cpt++)
        {
            const int nbthreads = 30;
            _exceptions = new List<Exception>();
            var threads = new Thread[nbthreads];

            // Starts threads
            for (int i = 0; i < nbthreads; i++)
            {
                var thread = new Thread(WorkerThread);
                thread.Start();
                threads[i] = thread;
            }

            // Waits for end of threads
            bool running = true;
            while (running)
            {
                Thread.Sleep(100);
                running = false;
                for (int i = 0; i < nbthreads; i++)
                {
                    if (threads[i].ThreadState == ThreadState.Running)
                        running = true;
                }
            }

            if (_exceptions.Count > 0)
            {
                Console.WriteLine(_exceptions[0].StackTrace);
                Assert.Fail(_exceptions[0].Message);
            }
        }
    }

    private List<Exception> _exceptions;

    private void WorkerThread()
    {
        try
        {
            var r1 = new Random((int)DateTime.Now.Ticks);
            var r2 = new Random((int)DateTime.Now.Ticks);
            int n1 = r1.Next(10);
            int n2 = r2.Next(10);

            // Constructs a simple addition randomly. Odds are that the same expression gets constructed multiple times by different threads
            var exp = n1 + " + " + n2;
            var e = new Expression(exp);
            Assert.IsTrue(e.Evaluate().Equals(n1 + n2));
        }
        catch (Exception e)
        {
            _exceptions.Add(e);
        }
    }

    [TestMethod]
    public void ShouldHandleCaseSensitiveness()
    {
        Assert.AreEqual(1M, new Expression("aBs(-1)", EvaluateOptions.IgnoreCase).Evaluate());
        Assert.AreEqual(1M, new Expression("Abs(-1)", EvaluateOptions.None).Evaluate());

        try
        {
            Assert.AreEqual(1M, new Expression("aBs(-1)", EvaluateOptions.None).Evaluate());
        }
        catch (ArgumentException)
        {
            return;
        }
        catch (Exception)
        {
            Assert.Fail("Unexpected exception");
        }

        Assert.Fail("Should throw ArgumentException");
    }

    [TestMethod]
    public void ShouldHandleCustomParametersWhenNoSpecificParameterIsDefined()
    {
        var e = new Expression("Round(Pow([Pi], 2) + Pow([Pi], 2) + 10, 2)");

        e.EvaluateParameter += delegate(string name, ParameterArgs arg)
        {
            if (name == "Pi")
                arg.Result = 3.14;
        };

        e.Evaluate();
    }

    [TestMethod]
    public void ShouldHandleCustomFunctionsInFunctions()
    {
        var e = new Expression("if(true, func1(x) + func2(func3(y)), 0)");

        e.EvaluateFunction += delegate(string name, FunctionArgs arg)
        {
            switch (name)
            {
                case "func1": arg.Result = 1;
                    break;
                case "func2": arg.Result = 2 * Convert.ToDouble(arg.Parameters[0].Evaluate());
                    break;
                case "func3": arg.Result = 3 * Convert.ToDouble(arg.Parameters[0].Evaluate());
                    break;
            }
        };

        e.EvaluateParameter += delegate(string name, ParameterArgs arg)
        {
            switch (name)
            {
                case "x": arg.Result = 1;
                    break;
                case "y": arg.Result = 2;
                    break;
                case "z": arg.Result = 3;
                    break;
            }
        };

        Assert.AreEqual(13d, e.Evaluate());
    }


    [TestMethod]
    public void ShouldParseScientificNotation()
    {
        Assert.AreEqual(12.2d, new Expression("1.22e1").Evaluate());
        Assert.AreEqual(100d, new Expression("1e2").Evaluate());
        Assert.AreEqual(100d, new Expression("1e+2").Evaluate());
        Assert.AreEqual(0.01d, new Expression("1e-2").Evaluate());
        Assert.AreEqual(0.001d, new Expression(".1e-2").Evaluate());
        Assert.AreEqual(10000000000d, new Expression("1e10").Evaluate());
    }

    [TestMethod]
    public void ShouldEvaluateArrayParameters()
    {
        var e = new Expression("x * x", EvaluateOptions.IterateParameters);
        e.Parameters["x"] = new [] { 0, 1, 2, 3, 4 };

        var result = (IList)e.Evaluate();

        Assert.AreEqual(0, result[0]);
        Assert.AreEqual(1, result[1]);
        Assert.AreEqual(4, result[2]);
        Assert.AreEqual(9, result[3]);
        Assert.AreEqual(16, result[4]);
    }

    [TestMethod]
    public void CustomFunctionShouldReturnNull()
    {
        var e = new Expression("SecretOperation(3, 6)");

        e.EvaluateFunction += delegate(string name, FunctionArgs args)
        {
            Assert.IsFalse(args.HasResult);
            if (name == "SecretOperation")
                args.Result = null;
            Assert.IsTrue(args.HasResult);
        };

        Assert.AreEqual(null, e.Evaluate());
    }

    [TestMethod]
    public void CustomParametersShouldReturnNull()
    {
        var e = new Expression("x");

        e.EvaluateParameter += delegate(string name, ParameterArgs args)
        {
            Assert.IsFalse(args.HasResult);
            if (name == "x")
                args.Result = null;
            Assert.IsTrue(args.HasResult);
        };

        Assert.AreEqual(null, e.Evaluate());
    }

    [TestMethod]
    public void ShouldCompareDates()
    {
        Assert.AreEqual(true, new Expression("#1/1/2009#==#1/1/2009#").Evaluate());
        Assert.AreEqual(false, new Expression("#2/1/2009#==#1/1/2009#").Evaluate());
    }

    [TestMethod]
    public void ShouldRoundAwayFromZero()
    {
        Assert.AreEqual(22d, new Expression("Round(22.5, 0)").Evaluate());
        Assert.AreEqual(23d, new Expression("Round(22.5, 0)", EvaluateOptions.RoundAwayFromZero).Evaluate());
    }

    [TestMethod]
    public void ShouldEvaluateSubExpressions()
    {
        var volume = new Expression("[surface] * h");
        var surface = new Expression("[l] * [L]");
        volume.Parameters["surface"] = surface;
        volume.Parameters["h"] = 3;
        surface.Parameters["l"] = 1;
        surface.Parameters["L"] = 2;

        Assert.AreEqual(6, volume.Evaluate());
    }

    [TestMethod]
    public void ShouldHandleLongValues()
    {
        Assert.AreEqual(40_000_000_000 + 1, new Expression("40000000000+1").Evaluate());
    }

    [TestMethod]
    public void ShouldCompareLongValues()
    {
        Assert.AreEqual(false, new Expression("(0=1500000)||(((0+2200000000)-1500000)<0)").Evaluate());
    }

    [TestMethod, ExpectedException(typeof(InvalidOperationException))]
    public void ShouldDisplayErrorIfUncompatibleTypes()
    {
        var e = new Expression("(a > b) + 10");
        e.Parameters["a"] = 1;
        e.Parameters["b"] = 2;
        e.Evaluate();
    }

    [TestMethod]
    public void ShouldNotConvertRealTypes()
    {
        var e = new Expression("x/2");
        e.Parameters["x"] = 2F;
        Assert.AreEqual(typeof(float), e.Evaluate().GetType());

        e = new Expression("x/2");
        e.Parameters["x"] = 2D;
        Assert.AreEqual(typeof(double), e.Evaluate().GetType());

        e = new Expression("x/2");
        e.Parameters["x"] = 2m;
        Assert.AreEqual(typeof(decimal), e.Evaluate().GetType());

        e = new Expression("a / b * 100");
        e.Parameters["a"] = 20M;
        e.Parameters["b"] = 20M;
        Assert.AreEqual(100M, e.Evaluate());

    }

    [TestMethod]
    public void ShouldShortCircuitBooleanExpressions()
    {
        var e = new Expression("([a] != 0) && ([b]/[a]>2)");
        e.Parameters["a"] = 0;

        Assert.AreEqual(false, e.Evaluate());
    }

    [TestMethod]
    public void ShouldAddDoubleAndDecimal()
    {
        var e = new Expression("1.8 + Abs([var1])");
        e.Parameters["var1"] = 9.2;

        Assert.AreEqual(11M, e.Evaluate());
    }

    [TestMethod]
    public void ShouldSubtractDoubleAndDecimal()
    {
        var e = new Expression("1.8 - Abs([var1])");
        e.Parameters["var1"] = 0.8;

        Assert.AreEqual(1M, e.Evaluate());
    }

    [TestMethod]
    public void ShouldMultiplyDoubleAndDecimal()
    {
        var e = new Expression("1.8 * Abs([var1])");
        e.Parameters["var1"] = 9.2;

        Assert.AreEqual(16.56M, e.Evaluate());
    }

    [TestMethod]
    public void ShouldDivideDoubleAndDecimal()
    {
        var e = new Expression("1.8 / Abs([var1])");
        e.Parameters["var1"] = 0.5;

        Assert.AreEqual(3.6M, e.Evaluate());
    }

    [TestMethod]
    public void IncorrectCalculation_NCalcAsync_Issue_4()
    {
        Expression e = new Expression("(1604326026000-1604325747000)/60000");
        var evalutedResult = e.Evaluate();

        Assert.IsInstanceOfType(evalutedResult, typeof(double));
        Assert.AreEqual(4.65, (double)evalutedResult, 0.001);
    }

    [TestMethod]
    public void Should_Throw_Exception_On_Lexer_Errors_Issue_6()
    {
        // https://github.com/ncalc/ncalc-async/issues/6
            
        var result1 = Assert.ThrowsException<EvaluationException>(() => Expression.Compile("\"0\"", EvaluateOptions.NoCache));
        Assert.AreEqual($"token recognition error at: '\"' at 1:1{Environment.NewLine}token recognition error at: '\"' at 1:3", result1.Message);

        var result2 = Assert.ThrowsException<EvaluationException>(() => Expression.Compile("Format(\"{0:(###) ###-####}\", \"9999999999\")", EvaluateOptions.NoCache));
        Assert.IsTrue(result2.InnerException?.GetType() == typeof(FormatException));
    }

    [TestMethod]
    public void Should_Divide_Decimal_By_Double_Issue_16()
    {
        // https://github.com/ncalc/ncalc/issues/16

        var e = new Expression("x / 1.0");
        e.Parameters["x"] = 1m;

        Assert.AreEqual(1m, e.Evaluate());
    }

    [TestMethod]
    public void Should_Divide_Decimal_By_Single()
    {
        var e = new Expression("x / y");
        e.Parameters["x"] = 1m;
        e.Parameters["y"] = 1f;

        Assert.AreEqual(1m, e.Evaluate());
    }

    [TestMethod]
    public void ShouldParseInvariantCulture()
    {
        var originalCulture = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
        try
        {
            var culture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
            culture.NumberFormat.NumberDecimalSeparator = ",";
            Thread.CurrentThread.CurrentCulture = culture;
            var exceptionThrown = false;
            try
            {
                var expr = new Expression("[a]<2.0") { Parameters = { ["a"] = "1.7" } };
                expr.Evaluate();
            }
            catch (FormatException)
            {
                exceptionThrown = true;
            }

            Assert.IsTrue(exceptionThrown);

            var e = new Expression("[a]<2.0", CultureInfo.InvariantCulture) { Parameters = { ["a"] = "1.7" } };
            Assert.AreEqual(true, e.Evaluate());
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = originalCulture;
        }
    }

    [TestMethod]
    public void Should_Add_All_Numeric_Types_Issue_58()
    {
        // https://github.com/ncalc/ncalc/issues/58
        var expectedResult = 100;
        var operand = "+";
        var lhsValue = "50";
        var rhsValue = "50";

        var allTypes = new List<TypeCode>()
        {
            TypeCode.Boolean, TypeCode.Byte, TypeCode.SByte, TypeCode.Int16, TypeCode.UInt16, TypeCode.Int32,
            TypeCode.UInt32, TypeCode.Int64, TypeCode.UInt64, TypeCode.Single, TypeCode.Double, TypeCode.Decimal
        };

        var shouldNotWork = new Dictionary<TypeCode, List<TypeCode>>();

        // We want to test all of the cases in numbers.cs which means we need to test both LHS/RHS
        shouldNotWork[TypeCode.Boolean] = allTypes;
        shouldNotWork[TypeCode.Byte] = new List<TypeCode> { TypeCode.Boolean };
        shouldNotWork[TypeCode.SByte] = new List<TypeCode> { TypeCode.Boolean, TypeCode.UInt64 };
        shouldNotWork[TypeCode.Int16] = new List<TypeCode> { TypeCode.Boolean, TypeCode.UInt64 };
        shouldNotWork[TypeCode.UInt16] = new List<TypeCode> { TypeCode.Boolean };
        shouldNotWork[TypeCode.Int32] = new List<TypeCode> { TypeCode.Boolean, TypeCode.UInt64 };
        shouldNotWork[TypeCode.UInt32] = new List<TypeCode> { TypeCode.Boolean };
        shouldNotWork[TypeCode.Int64] = new List<TypeCode> { TypeCode.Boolean, TypeCode.UInt64 };
        shouldNotWork[TypeCode.UInt64] = new List<TypeCode>
            { TypeCode.Boolean, TypeCode.SByte, TypeCode.Int16, TypeCode.Int32, TypeCode.Int64 };
        shouldNotWork[TypeCode.Single] = new List<TypeCode> { TypeCode.Boolean };
        shouldNotWork[TypeCode.Double] = new List<TypeCode> { TypeCode.Boolean };
        shouldNotWork[TypeCode.Decimal] = new List<TypeCode> { TypeCode.Boolean };

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
                    Assert.IsTrue(Convert.ToInt64(result) == expectedResult,
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
                Assert.ThrowsException<InvalidOperationException>(() => new Expression(expr, CultureInfo.InvariantCulture)
                        {
                            Parameters =
                            {
                                ["x"] = Convert.ChangeType(1, typecodeA),
                                ["y"] = Convert.ChangeType(1, typecodeB)
                            }
                        }
                        .Evaluate(),$"{expr}: {typecodeA}, {typecodeB}");
            }
        }
    }

    [TestMethod]
    public void Should_Subtract_All_Numeric_Types_Issue_58()
    {
        // https://github.com/ncalc/ncalc/issues/58
        var expectedResult = 0;
        var operand = "-";
        var lhsValue = 50;
        var rhsValue = 50;

        var allTypes = new List<TypeCode>()
        {
            TypeCode.Boolean, TypeCode.Byte, TypeCode.SByte, TypeCode.Int16, TypeCode.UInt16, TypeCode.Int32,
            TypeCode.UInt32, TypeCode.Int64, TypeCode.UInt64, TypeCode.Single, TypeCode.Double, TypeCode.Decimal
        };

        var shouldNotWork = new Dictionary<TypeCode, List<TypeCode>>();

        // We want to test all of the cases in numbers.cs which means we need to test both LHS/RHS
        shouldNotWork[TypeCode.Boolean] = allTypes;
        shouldNotWork[TypeCode.Byte] = new List<TypeCode> { TypeCode.Boolean };
        shouldNotWork[TypeCode.SByte] = new List<TypeCode> { TypeCode.Boolean, TypeCode.UInt64 };
        shouldNotWork[TypeCode.Int16] = new List<TypeCode> { TypeCode.Boolean, TypeCode.UInt64 };
        shouldNotWork[TypeCode.UInt16] = new List<TypeCode> { TypeCode.Boolean };
        shouldNotWork[TypeCode.Int32] = new List<TypeCode> { TypeCode.Boolean, TypeCode.UInt64 };
        shouldNotWork[TypeCode.UInt32] = new List<TypeCode> { TypeCode.Boolean };
        shouldNotWork[TypeCode.Int64] = new List<TypeCode> { TypeCode.Boolean, TypeCode.UInt64 };
        shouldNotWork[TypeCode.UInt64] = new List<TypeCode>
            { TypeCode.Boolean, TypeCode.SByte, TypeCode.Int16, TypeCode.Int32, TypeCode.Int64 };
        shouldNotWork[TypeCode.Single] = new List<TypeCode> { TypeCode.Boolean, TypeCode.Decimal };
        shouldNotWork[TypeCode.Double] = new List<TypeCode> { TypeCode.Boolean };
        shouldNotWork[TypeCode.Decimal] = new List<TypeCode> { TypeCode.Boolean };

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
                    Assert.IsTrue(Convert.ToInt64(result) == expectedResult,
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
                Assert.ThrowsException<InvalidOperationException>(() => new Expression(expr, CultureInfo.InvariantCulture)
                        {
                            Parameters =
                            {
                                ["x"] = Convert.ChangeType(lhsValue, typecodeA),
                                ["y"] = Convert.ChangeType(rhsValue, typecodeB)
                            }
                        }
                        .Evaluate(),$"{expr}: {typecodeA}, {typecodeB}");
            }
        }
    }

    [TestMethod]
    public void Should_Multiply_All_Numeric_Types_Issue_58()
    {
        // https://github.com/ncalc/ncalc/issues/58
        var expectedResult = 64;
        var operand = "*";
        var lhsValue = 8;
        var rhsValue = 8;

        var allTypes = new List<TypeCode>()
        {
            TypeCode.Boolean, TypeCode.Byte, TypeCode.SByte, TypeCode.Int16, TypeCode.UInt16, TypeCode.Int32,
            TypeCode.UInt32, TypeCode.Int64, TypeCode.UInt64, TypeCode.Single, TypeCode.Double, TypeCode.Decimal
        };

        var shouldNotWork = new Dictionary<TypeCode, List<TypeCode>>();

        // We want to test all of the cases in numbers.cs which means we need to test both LHS/RHS
        shouldNotWork[TypeCode.Boolean] = allTypes;
        shouldNotWork[TypeCode.Byte] = new List<TypeCode> { TypeCode.Boolean };
        shouldNotWork[TypeCode.SByte] = new List<TypeCode> { TypeCode.Boolean, TypeCode.UInt64 };
        shouldNotWork[TypeCode.Int16] = new List<TypeCode> { TypeCode.Boolean, TypeCode.UInt64 };
        shouldNotWork[TypeCode.UInt16] = new List<TypeCode> { TypeCode.Boolean };
        shouldNotWork[TypeCode.Int32] = new List<TypeCode> { TypeCode.Boolean, TypeCode.UInt64 };
        shouldNotWork[TypeCode.UInt32] = new List<TypeCode> { TypeCode.Boolean };
        shouldNotWork[TypeCode.Int64] = new List<TypeCode> { TypeCode.Boolean, TypeCode.UInt64 };
        shouldNotWork[TypeCode.UInt64] = new List<TypeCode>
            { TypeCode.Boolean, TypeCode.SByte, TypeCode.Int16, TypeCode.Int32, TypeCode.Int64 };
        shouldNotWork[TypeCode.Single] = new List<TypeCode> { TypeCode.Boolean };
        shouldNotWork[TypeCode.Double] = new List<TypeCode> { TypeCode.Boolean };
        shouldNotWork[TypeCode.Decimal] = new List<TypeCode> { TypeCode.Boolean };

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
                    Assert.IsTrue(Convert.ToInt64(result) == expectedResult,
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
                Assert.ThrowsException<InvalidOperationException>(() => new Expression(expr, CultureInfo.InvariantCulture)
                        {
                            Parameters =
                            {
                                ["x"] = Convert.ChangeType(lhsValue, typecodeA),
                                ["y"] = Convert.ChangeType(rhsValue, typecodeB)
                            }
                        }
                        .Evaluate(),$"{expr}: {typecodeA}, {typecodeB}");
            }
        }
    }

    [TestMethod]
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

        var shouldNotWork = new Dictionary<TypeCode, List<TypeCode>>();

        // We want to test all of the cases in numbers.cs which means we need to test both LHS/RHS
        shouldNotWork[TypeCode.Boolean] = allTypes;
        shouldNotWork[TypeCode.Byte] = new List<TypeCode> { TypeCode.Boolean };
        shouldNotWork[TypeCode.SByte] = new List<TypeCode> { TypeCode.Boolean, TypeCode.UInt64 };
        shouldNotWork[TypeCode.Int16] = new List<TypeCode> { TypeCode.Boolean, TypeCode.UInt64 };
        shouldNotWork[TypeCode.UInt16] = new List<TypeCode> { TypeCode.Boolean };
        shouldNotWork[TypeCode.Int32] = new List<TypeCode> { TypeCode.Boolean, TypeCode.UInt64 };
        shouldNotWork[TypeCode.UInt32] = new List<TypeCode> { TypeCode.Boolean };
        shouldNotWork[TypeCode.Int64] = new List<TypeCode> { TypeCode.Boolean, TypeCode.UInt64 };
        shouldNotWork[TypeCode.UInt64] = new List<TypeCode>
            { TypeCode.Boolean, TypeCode.SByte, TypeCode.Int16, TypeCode.Int32, TypeCode.Int64 };
        shouldNotWork[TypeCode.Single] = new List<TypeCode> { TypeCode.Boolean, TypeCode.Decimal };
        shouldNotWork[TypeCode.Double] = new List<TypeCode> { TypeCode.Boolean, TypeCode.Decimal };
        shouldNotWork[TypeCode.Decimal] = new List<TypeCode> { TypeCode.Boolean, TypeCode.Single, TypeCode.Double };

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
                    Assert.IsTrue(Convert.ToInt64(result) == expectedResult,
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
                Assert.ThrowsException<InvalidOperationException>(() => new Expression(expr, CultureInfo.InvariantCulture)
                        {
                            Parameters =
                            {
                                ["x"] = Convert.ChangeType(lhsValue, typecodeA),
                                ["y"] = Convert.ChangeType(rhsValue, typecodeB)
                            }
                        }
                        .Evaluate(),$"{expr}: {typecodeA}, {typecodeB}");
            }
        }
    }

    [TestMethod]
    public void ShouldCorrectlyParseCustomCultureParameter()
    {
        var cultureDot = (CultureInfo)CultureInfo.InvariantCulture.Clone();
        cultureDot.NumberFormat.NumberGroupSeparator = " ";
        var cultureComma = (CultureInfo)CultureInfo.InvariantCulture.Clone();
        cultureComma.NumberFormat.CurrencyDecimalSeparator = ",";
        cultureComma.NumberFormat.NumberGroupSeparator = " ";

        //use 1*[A] to avoid evaluating expression parameters as string - force numeric conversion
        ExecuteTest("1*[A]-[B]", 1.5m);
        ExecuteTest("1*[A]+[B]", 2.5m);
        ExecuteTest("1*[A]/[B]", 4m);
        ExecuteTest("1*[A]*[B]", 1m);
        ExecuteTest("1*[A]>[B]", true);
        ExecuteTest("1*[A]<[B]", false);


        void ExecuteTest(string formula, object expectedValue)
        {
            //Correctly evaluate with decimal dot culture and parameter with dot
            Assert.AreEqual(expectedValue, new Expression(formula, cultureDot)
            {
                Parameters = new Dictionary<string, object>
                {
                    {"A","2.0"},
                    {"B","0.5"}

                }
            }.Evaluate());

            //Correctly evaluate with decimal comma and parameter with comma
            Assert.AreEqual(expectedValue, new Expression(formula, cultureComma)
            {
                Parameters = new Dictionary<string, object>
                {
                    {"A","2.0"},
                    {"B","0.5"}

                }
            }.Evaluate());

            //combining decimal dot and comma fails
            Assert.ThrowsException<FormatException>(() => new Expression(formula, cultureComma)
            {
                Parameters = new Dictionary<string, object>
                {
                    {"A","2,0"},
                    {"B","0.5"}

                }
            }.Evaluate());

            //combining decimal dot and comma fails
            Assert.ThrowsException<FormatException>(() => new Expression(formula, cultureDot)
            {
                Parameters = new Dictionary<string, object>
                {
                    {"A","2,0"},
                    {"B","0.5"}

                }
            }.Evaluate());

        }
    }

    [TestMethod]
    public void SerializeAndDeserialize_ShouldWork()
    {
            
        ExecuteTest(@"(waterlevel > 1 AND waterlevel <= 3)", false, 3.2);
        ExecuteTest(@"(waterlevel > 3 AND waterlevel <= 5)", true, 3.2);
        ExecuteTest(@"(waterlevel > 1 AND waterlevel <= 3)", false, 3.1);
        ExecuteTest(@"(waterlevel > 3 AND waterlevel <= 5)", true, 3.1);
        ExecuteTest(@"(3 < waterlevel AND 5 >= waterlevel)", true, 3.1);
        ExecuteTest(@"(3.2 < waterlevel AND 5.3 >= waterlevel)", true, 4);

        void ExecuteTest(string expression, bool expected, double inputValue)
        {
            var compiled = Expression.Compile(expression, EvaluateOptions.NoCache);
            var serialized = JsonConvert.SerializeObject(compiled, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All // We need this to allow serializing abstract classes
            });

            var deserialized = JsonConvert.DeserializeObject<LogicalExpression>(serialized, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });

            Expression.CacheEnabled = false;
            var exp = new Expression(deserialized);
            exp.Parameters = new Dictionary<string, object> {
                {"waterlevel", inputValue}
            };

            object evaluated;
            try {
                evaluated = exp.Evaluate();
            } catch {
                evaluated = false;
            }

            // Assert
            Assert.AreEqual(evaluated, expected, expression);
        }
    }
        
    [TestMethod]
    public void Should_Use_Case_Insensitive_Comparer_Issue_85()
    {
        var eif = new Expression("PageState == 'LIST'", EvaluateOptions.CaseInsensitiveComparer);
        eif.Parameters["PageState"] = "List";

        Assert.AreEqual(true, eif.Evaluate());
    }
        
    [TestMethod]
    public void Should_Get_Parameters_Issue_103()
    {
        var eif = new Expression("PageState == 'LIST' && a == 1 && customFunction() == true || in(1 + 1, 1, 2, 3)", EvaluateOptions.CaseInsensitiveComparer)
        {
            Parameters =
            {
                ["a"] = 1
            }
        };
        eif.EvaluateParameter += (name, args) =>
        {
            if (name == "PageState")
                args.Result = "List";
        };
            
        eif.EvaluateFunction += (name, args) =>
        {
            if (name == "customfunction")
                args.Result = "true";
        };

        var parameters = eif.GetParametersNames();
        Assert.IsTrue(parameters.Contains("a"));
        Assert.IsTrue(parameters.Contains("PageState"));
        Assert.IsTrue(parameters.Length == 2);
    }

    [TestMethod]
    public void Should_Not_Throw_Function_Not_Found_Issue_110()
    {
        const string expressionStr = "IN([acp_associated_person_transactions], 'T', 'Z', 'A')";
        var expression = new Expression(expressionStr) {
            Options = EvaluateOptions.RoundAwayFromZero | EvaluateOptions.IgnoreCase,
            Parameters =
            {
                ["acp_associated_person_transactions"] = 'T'
            }
        };

        Assert.AreEqual(true, expression.Evaluate());
    }

        
    [TestMethod]
    public void Should_Evaluate_Ifs()
    {
        // Test first case true, return next value
        var eifs = new Expression("ifs([divider] != 0, [divided] / [divider], -1)");
        eifs.Parameters["divider"] = 5;
        eifs.Parameters["divided"] = 5;

        Assert.AreEqual(1d, eifs.Evaluate());

        // Test first case false, no next case, return default value
        eifs = new Expression("ifs([divider] != 0, [divided] / [divider], -1)");
        eifs.Parameters["divider"] = 0;
        eifs.Parameters["divided"] = 5;

        Assert.AreEqual(-1, eifs.Evaluate());

        // Test first case false, next case true, return next value (eg 4th expr)

        eifs = new Expression("ifs([number] == 3, 5, [number] == 5, 3, 8)");
        eifs.Parameters["number"] = 5;
        Assert.AreEqual(3, eifs.Evaluate());

        // Test first case false, next case false, return default value (eg 5th expr)

        eifs = new Expression("ifs([number] == 3, 5, [number] == 5, 3, 8)");
        eifs.Parameters["number"] = 1337;

        Assert.AreEqual(8, eifs.Evaluate());
    }

    [TestMethod]
    public void Ifs_With_Improper_Arguments_Should_Throw_Exceptions()
    {
        Assert.ThrowsException<ArgumentException>(() => new Expression("ifs()").Evaluate());
        Assert.ThrowsException<ArgumentException>(() => new Expression("ifs([divider] > 0)").Evaluate());
        Assert.ThrowsException<ArgumentException>(() => new Expression("ifs([divider] > 0, [divider] / [divided])").Evaluate());
        Assert.ThrowsException<ArgumentException>(() => new Expression("ifs([divider] > 0, [divider] / [divided], [divider < 0], [divider] + [divided])").Evaluate());
    }
        
    [TestMethod]
    public void Compare_Using_Most_Precise_Type_Issue_102()
    {
        var issueExp = new Expression("a == b")
        {
            Parameters =
            {
                ["a"] = null,
                ["b"] = 2
            }
        };

        Assert.IsFalse((bool)issueExp.Evaluate());
            
        var numericExp = new Expression("a == b")
        {
            Parameters =
            {
                ["a"] = 2,
                ["b"] = 2L
            }
        };

        Assert.IsTrue((bool)numericExp.Evaluate());

        var obj = new Tuple<string,string>("Hello", "World");
        var objExp = new Expression("a == b")
        {
            Parameters =
            {
                ["a"] = obj,
                ["b"] = obj
            }
        };

        Assert.IsTrue((bool)objExp.Evaluate());
    }

              
    [TestMethod]
    public void Should_Evaluate_Function_Only_Once_Issue_107()
    {
        var counter = 0;
        var totalCounter = 0;

        var expression = new Expression("MyFunc()");

        expression.EvaluateFunction += Expression_EvaluateFunction;

        for (var i = 0; i < 10; i++)
        {
            counter = 0;
            _ = expression.Evaluate();
        }


        void Expression_EvaluateFunction(string name, FunctionArgs args)
        {
            if (name != "MyFunc") return;
            args.Result = 1;
            counter++;
            totalCounter++;
        }

        Assert.AreEqual(totalCounter, 10);
    }

    [TestMethod]
    public void Should_Use_Decimal_When_Configured() {
        var expression = new Expression("12.34", EvaluateOptions.DecimalAsDefault);

        var result = expression.Evaluate();
        Assert.IsTrue(result is decimal);
        Assert.AreEqual(12.34m, result);
    }
        
    [TestMethod]
    public void Decimals_Should_Not_Loose_Precision() {
        var expression = new Expression("0.3 - 0.2 - 0.1", EvaluateOptions.DecimalAsDefault);

        var result = (decimal)expression.Evaluate();
        Assert.AreEqual("0.0", result.ToString(CultureInfo.InvariantCulture)); // Fails without decimals due to FP rounding
    }
        
                
    [TestMethod]
    public void Should_Compare_Bool_Issue_122()
    {
        var eif = new Expression("foo = true");
        eif.Parameters["foo"] = "true";

        Assert.AreEqual(true, eif.Evaluate());
    }
    
    [TestMethod]
    public void Should_Use_Correct_BitwiseXOr_133()
    {
        const EvaluateOptions options = EvaluateOptions.None;
        var logicalExpression = Expression.Compile(expression: "1 ^ 2", options);

        var serializedString = logicalExpression.ToString();

        Assert.AreEqual("1 ^ 2", serializedString);
        Assert.AreEqual(3, new Expression(logicalExpression).Evaluate());
    }
}