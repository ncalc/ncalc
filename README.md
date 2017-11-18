# NCalc

[![Appveyor](https://img.shields.io/appveyor/ci/yallie/ncalc.svg)](https://ci.appveyor.com/project/yallie/ncalc)
[![Coverage](https://img.shields.io/codecov/c/github/ncalc/ncalc.svg)](https://codecov.io/gh/ncalc/ncalc)
[![Tests](https://img.shields.io/appveyor/tests/yallie/ncalc.svg)](https://ci.appveyor.com/project/yallie/ncalc/build/tests)

NCalc is a mathematical expressions evaluator in .NET. NCalc can parse any expression and evaluate the result, including static or dynamic parameters and custom functions.

## Project Description

NCalc is a mathematical expressions evaluator in .NET. NCalc can parse any expression and evaluate the result, including static or dynamic parameters and custom functions.

For additional information on the technique we used to create this framework please read this article: http://www.codeproject.com/KB/recipes/sota_expression_evaluator.aspx.

For documentation here is the table of content:
* [description](https://github.com/ncalc/ncalc/wiki/Description): overall concepts, usage and extensibility points
* [operators](https://github.com/ncalc/ncalc/wiki/Operators): available standard operators and structures
* [values](https://github.com/ncalc/ncalc/wiki/Values): authorized values like types, functions, ...
* [functions](https://github.com/ncalc/ncalc/wiki/Functions): list of already implemented functions
* [parameters](https://github.com/ncalc/ncalc/wiki/Parameters): on how to use parameters expressions

## Functionalities
**Simple Expressions**

```c#
Expression e = new Expression("2 + 3 * 5");
Debug.Assert(17 == e.Evaluate());
```

**Evaluates .NET data types**

```c#
Debug.Assert(123456 == new Expression("123456").Evaluate()); // integers
Debug.Assert(new DateTime(2001, 01, 01) == new Expression("#01/01/2001#").Evaluate()); // date and times
Debug.Assert(123.456 == new Expression("123.456").Evaluate()); // floating point numbers
Debug.Assert(true == new Expression("true").Evaluate()); // booleans
Debug.Assert("azerty" == new Expression("'azerty'").Evaluate()); // strings
```

**Handles mathematical functional from System.Math**

```c#
Debug.Assert(0 == new Expression("Sin(0)").Evaluate());
Debug.Assert(2 == new Expression("Sqrt(4)").Evaluate());
Debug.Assert(0 == new Expression("Tan(0)").Evaluate());
```

**Evaluates custom functions**

```c#
Expression e = new Expression("SecretOperation(3, 6)");
e.EvaluateFunction += delegate(string name, FunctionArgs args)
    {
        if (name == "SecretOperation")
            args.Result = (int)args.Parameters[0].Evaluate() + (int)args.Parameters[1].Evaluate();
    };

Debug.Assert(9 == e.Evaluate());
```

**Handles unicode characters**

```c#
Debug.Assert("経済協力開発機構" == new Expression("'経済協力開発機構'").Evaluate());
Debug.Assert("Hello" == new Expression(@"'\u0048\u0065\u006C\u006C\u006F'").Evaluate());
Debug.Assert("だ" == new Expression(@"'\u3060'").Evaluate());
Debug.Assert("\u0100" == new Expression(@"'\u0100'").Evaluate());
```

**Define parameters, even dynamic or expressions**

```c#
Expression e = new Expression("Round(Pow([Pi], 2) + Pow([Pi2], 2) + [X], 2)");

e.Parameters["Pi2"] = new Expression("Pi * [Pi]");
e.Parameters["X"] = 10;

e.EvaluateParameter += delegate(string name, ParameterArgs args)
  {
    if (name == "Pi")
    args.Result = 3.14;
  };

Debug.Assert(117.07 == e.Evaluate());
```
