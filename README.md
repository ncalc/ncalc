<div style="text-align:center">
    <img src="NCalc.png" alt="NCalc" style="width:100px;"/>
    <h1>NCalc</h1>
    <a href="https://github.com/ncalc/ncalc/actions/workflows/build-test.yml">
      <img src="https://img.shields.io/github/actions/workflow/status/ncalc/ncalc/build-test.yml" alt="GitHub Actions Workflow Status" />
    </a>
    <a href="https://codecov.io/gh/ncalc/ncalc">
      <img src="https://img.shields.io/codecov/c/github/ncalc/ncalc.svg" alt="Coverage" />
    </a>
    <a href="https://nuget.org/packages/NCalcSync.signed">
      <img src="https://img.shields.io/nuget/v/NCalcSync.signed.svg?label=nuget&color=004880" alt="NuGet" />
    </a>
    <a href="https://nuget.org/packages/NCalcSync.signed">
      <img src="https://img.shields.io/nuget/dt/NCalcSync.svg?color=004880" alt="NuGet Downloads" />
    </a>
    <a href="https://discord.gg/TeJkmXbqFk">
      <img src="https://img.shields.io/discord/1237181265426387005?color=5b62ef&label=discord" alt="Discord" />
    </a>
</div>




NCalc is a fast and lightweight expression evaluator library for .NET, designed for flexibility and high performance. It
supports a wide range of mathematical and logical operations. NCalc can parse any expression and evaluate the result,
including static or dynamic parameters and custom functions. NCalc targets .NET 8, .NET Standard 2.0 and NET Framework
4.8.

## Docs

Need help or want to learn more? [Check our docs.](https://ncalc.github.io/ncalc)

## Learn more

For additional information on the technique we used to create this framework please
read [this article.](https://www.codeproject.com/Articles/18880/State-of-the-Art-Expression-Evaluation)

## Help

> [!IMPORTANT]
> If you need help, [please open an issue](https://github.com/ncalc/ncalc/issues/new/choose) and include the expression
> to help us better understand the problem.
> Providing this information will aid in resolving the issue effectively.

## Getting Started

If you want to evaluate simple expressions:

```
dotnet add package NCalcSync 
```

Want `async` support at your functions and parameters?

```
dotnet add package NCalcAsync 
```

Dependency Injection? We got you covered:

```
dotnet add package NCalc.DependencyInjection
```

## Functionalities

### Simple Expressions

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
expression.Functions["SecretOperation"] = (args) => {
    return (int)args[0].Evaluate() + (int)args[1].Evaluate();
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

expression.DynamicParameters["Pi"] = _ => {
    Console.WriteLine("I'm evaluating π!");
    return 3.14;
};

Debug.Assert(117.07 == expression.Evaluate());
```

**Caching and Serializing**

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

You can also use our [Memory Cache plugin.](https://ncalc.github.io/ncalc/articles/plugins/memory_cache.html)

**Lambda Expressions**

```cs
var expression = new Expression("1 + 2");
Func<int> function = expression.ToLambda<int>();
Debug.Assert(function()); //3
```

## Related projects

### [Parlot](https://github.com/sebastienros/parlot)

Fast and lightweight parser creation tools by [Sébastien Ros](https://github.com/sebastienros) that NCalc uses at its
parser.

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

NCalc 101 is a simple web application that allows you to try out the NCalc expression evaluator, developed
by [Panoramic Data](https://github.com/panoramicdata).

### [JJMasterData](https://github.com/JJConsulting/JJMasterData/)

JJMasterData is a runtime form generator from database metadata. It uses NCalc to evaluate expressions used in field
visibility and other dynamic behaviors.

# NCalc versioning

The project uses [Nerdbank.GitVersioning](https://github.com/dotnet/Nerdbank.GitVersioning) tool to manage versions.  
Each library build can be traced back to the original git commit.
Read more about [versioning here.](https://ncalc.github.io/ncalc/articles/new_release.html)
## Discord Server

If you want to talk with us, get support or just get the latest NCalc
news, [come to our discord server](https://discord.gg/TeJkmXbqFk).
