# NCalc

[![Appveyor](https://img.shields.io/appveyor/ci/yallie/ncalc.svg)](https://ci.appveyor.com/project/yallie/ncalc)
[![Coverage](https://img.shields.io/codecov/c/github/ncalc/ncalc.svg)](https://codecov.io/gh/ncalc/ncalc)
[![Tests](https://img.shields.io/appveyor/tests/yallie/ncalc.svg)](https://ci.appveyor.com/project/yallie/ncalc/build/tests)
[![NuGet](https://img.shields.io/nuget/v/NCalcSync.svg)](https://nuget.org/packages/NCalcSync)

NCalc is a mathematical expressions evaluator in .NET. NCalc can parse any expression and evaluate the result, including static or dynamic parameters and custom functions.

## Project Description

NCalc is a mathematical expressions evaluator in .NET. NCalc can parse any expression and evaluate the result, including static or dynamic parameters and custom functions.

For additional information on the technique we used to create this framework please read this article: https://www.codeproject.com/Articles/18880/State-of-the-Art-Expression-Evaluation.

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

**Caching in a distributed cache**

This example uses [Newtonsoft.Json](https://www.nuget.org/packages/Newtonsoft.Json/).

Serializing
```c#
var compiled = Expression.Compile(expression, true);
var serialized = JsonConvert.SerializeObject(compiled, new JsonSerializerSettings
{
    TypeNameHandling = TypeNameHandling.All // We need this to allow serializing abstract classes
});
```

Deserializing
```c#
var deserialized = JsonConvert.DeserializeObject<LogicalExpression>(serialized, new JsonSerializerSettings
{
    TypeNameHandling = TypeNameHandling.All
});

Expression.CacheEnabled = false; // We cannot use NCalc's built in cache at the same time.
var exp = new Expression(deserialized);
exp.Parameters = new Dictionary<string, object> {
    {"waterlevel", inputValue}
};

var evaluated = exp.Evaluate();
```

## Related projects

### [NCalc-Async](https://github.com/ncalc/ncalc-async/)

Pure asynchronous implementation of NCalc by [Peter Liljenberg](https://github.com/petli).

### [PanoramicData.NCalcExtensions](https://github.com/panoramicdata/PanoramicData.NCalcExtensions)

Extension functions for NCalc to handle many general functions,  
including string functions, switch, if, in, typeOf, cast etc.  
Developed by David, Dan and all at [Panoramic Data](https://github.com/panoramicdata).

### [Jint](https://github.com/sebastienros/jint)

Javascript Interpreter for .NET by [Sébastien Ros](https://github.com/sebastienros), the author of NCalc library.  
Runs on any modern .NET platform as it supports .NET Standard 2.0 and .NET 4.6.1 targets (and up).

### [NCalcJS](https://github.com/thomashambach/ncalcjs)

A Typescript/Javascript port of NCalc.
