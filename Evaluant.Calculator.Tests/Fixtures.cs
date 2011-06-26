using System;
using NCalc.Domain;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Collections;

namespace NCalc.Tests
{
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
            Assert.AreEqual(123.456d, new Expression("123.456").Evaluate());
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
            Assert.AreEqual(0d, new Expression("Log10(1)").Evaluate());
            Assert.AreEqual(9d, new Expression("Pow(3,2)").Evaluate());
            Assert.AreEqual(3.22d, new Expression("Round(3.222,2)").Evaluate());
            Assert.AreEqual(-1, new Expression("Sign(-10)").Evaluate());
            Assert.AreEqual(0d, new Expression("Sin(0)").Evaluate());
            Assert.AreEqual(2d, new Expression("Sqrt(4)").Evaluate());
            Assert.AreEqual(0d, new Expression("Tan(0)").Evaluate());
            Assert.AreEqual(1d, new Expression("Truncate(1.7)").Evaluate());
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
        public void ShouldEvaluateConditionnal()
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
                                      {"true and false", false},
                                      {"true || false", true},
                                      {"true or false", true},
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
        }

        [TestMethod]
        public void ShouldNotLoosePrecision()
        {
            Assert.AreEqual(0.5, new Expression("3/6").Evaluate());
        }

        [TestMethod]
        public void ShouldThrowAnExpcetionWhenInvalidNumber()
        {
            try
            {
                new Expression("4. + 2").Evaluate();
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

            e = new Expression("+ b ");
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
            Assert.AreEqual(40000000000 + 1f, new Expression("40000000000+1").Evaluate());
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
    }
}

