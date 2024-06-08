# NCalc

[![Appveyor](https://img.shields.io/appveyor/ci/yallie/ncalc.svg)](https://ci.appveyor.com/project/yallie/ncalc)
[![Coverage](https://img.shields.io/codecov/c/github/ncalc/ncalc.svg)](https://codecov.io/gh/ncalc/ncalc)
[![Tests](https://img.shields.io/appveyor/tests/yallie/ncalc.svg)](https://ci.appveyor.com/project/yallie/ncalc/build/tests)
[![NuGet](https://img.shields.io/nuget/v/NCalcSync.svg?label=nuget%20unsigned
)](https://nuget.org/packages/NCalcSync)
[![NuGet](https://img.shields.io/nuget/v/NCalcSync.signed.svg?label=nuget%20signed
)](https://nuget.org/packages/NCalcSync.signed)
[![Discord](https://img.shields.io/discord/1237181265426387005?logo=discord&logoColor=white&label=%20&labelColor=%23697EC4&color=%237289DA
)](https://discord.gg/TeJkmXbqFk)

NCalc is a mathematical expression evaluator in .NET. NCalc can parse any expression and evaluate the result, including static or dynamic parameters and custom functions.

## Docs
Need help or want to learn more? [Check our docs.](https://ncalc.github.io/ncalc)


## Project Description

NCalc is a .NET library for evaluating mathematical expressions. It can handle various types of expressions, including those with static or dynamic parameters, as well as custom functions.
It is supported by any target framework that accommodates .NET Standard 2.0.

For additional information on the technique we used to create this framework please read this article: https://www.codeproject.com/Articles/18880/State-of-the-Art-Expression-Evaluation.

> [!IMPORTANT]
> If you need help, please open an issue and include the expression to help us better understand the problem. 
> Providing this information will aid in resolving the issue effectively.

## Getting Started
If you want to evaluate simple expressions:
```
dotnet add package NCalcSync 
```
Want async support at event handlers?
```
dotnet add package NCalcAsync 
```
Dependency Injection? We got you covered:
```
dotnet add package NCalc.DependencyInjection
```

## Functionalities
**Simple Expressions**

```c#
var expression = new Expression("2 + 3 * 5");
Debug.Assert(17 == expression.Evaluate());
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
var expression = new Expression("SecretOperation(3, 6)");
expression.EvaluateFunction += delegate(string name, FunctionArgs args)
    {
        if (name == "SecretOperation")
            args.Result = (int)args.Parameters[0].Evaluate() + (int)args.Parameters[1].Evaluate();
    };

Debug.Assert(9 == expression.Evaluate());
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
var expression = new Expression("Round(Pow([Pi], 2) + Pow([Pi2], 2) + [X], 2)");

expression.Parameters["Pi2"] = new Expression("Pi * [Pi]");
expression.Parameters["X"] = 10;

expression.EvaluateParameter += delegate(string name, ParameterArgs args)
  {
    if (name == "Pi")
    args.Result = 3.14;
  };

Debug.Assert(117.07 == expression.Evaluate());
```

**Caching in a distributed cache**

This example uses [Newtonsoft.Json](https://www.nuget.org/packages/Newtonsoft.Json/).

Serializing
```c#
var parsedExpression = LogicalExpressionFactory.Create(expression, ExpressionOptions.NoCache);
var serialized = JsonConvert.SerializeObject(parsedExpression, new JsonSerializerSettings
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

var expression = new Expression(deserialized);
expression.Parameters = new Dictionary<string, object> {
    {"waterlevel", inputValue}
};

var result = expression.Evaluate();
```

**Lambda Expressions**
```cs
var expression = new Expression("1 + 2");
Func<int> function = expression.ToLambda<int>();
Debug.Assert(function()); //3
```


## Related projects

### [Parlot](https://github.com/sebastienros/parlot)

Fast and lightweight parser creation tools by [Sébastien Ros](https://github.com/sebastienros) that NCalc uses at its parser.

### [PanoramicData.NCalcExtensions](https://github.com/panoramicdata/PanoramicData.NCalcExtensions)

Extension functions for NCalc to handle many general functions,  
including string functions, switch, if, in, typeOf, cast etc.  
Developed by David, Dan and all at [Panoramic Data](https://github.com/panoramicdata).

### [Jint](https://github.com/sebastienros/jint)

JavaScript Interpreter for .NET by [Sébastien Ros](https://github.com/sebastienros), the author of NCalc library.  
Runs on any modern .NET platform as it supports .NET Standard 2.0 and .NET 4.6.1 targets (and up).

### [NCalcJS](https://github.com/thomashambach/ncalcjs)

A TypeScript/JavaScript port of NCalc.

### [NCalc101](https://ncalc101.magicsuite.net)

NCalc 101 is a simple web application that allows you to try out the NCalc expression evaluator, developed by [Panoramic Data](https://github.com/panoramicdata).

### [JJMasterData.NCalc](https://md.jjconsulting.tech/articles/plugins/ncalc.html)

Plugin of NCalc used to evaluate [JJMasterData](https://github.com/jjconsulting/jjmasterdata) expressions. JJMasterData is a runtime form generator from database metadata.

# NCalc versioning

The project uses [Nerdbank.GitVersioning](https://github.com/dotnet/Nerdbank.GitVersioning) tool to manage versions.  
Each library build can be traced back to the original git commit.

## Preparing and publishing a new release

1. Make sure that `nbgv` dotnet CLI tool is installed and is up-to-date
2. Run `nbgv prepare-release` to create a stable branch for the upcoming release, i.e. release/v1.0
3. Switch to the release branch: `git checkout release/v1.0`
4. Execute unit tests, update the README, release notes in csproj file, etc. Commit and push your changes.
5. Run `dotnet pack -c Release` and check that it builds Nuget packages with the right version number.
6. Run `nbgv tag release/v1.0` to tag the last commit on the release branch with your current version number, i.e. v1.0.7.
7. Push tags as suggested by nbgv tool: `git push origin v1.0.7`
8. Go to GitHub project page and create a release out of the last tag v1.0.7.
9. Verify that github workflow for publishing the nuget package has completed.
10. Switch back to master and merge the release branch.

## Discord Server

If you want to talk with us, get support or just get the latest NCalc news, [come to our discord server](https://discord.gg/TeJkmXbqFk).
