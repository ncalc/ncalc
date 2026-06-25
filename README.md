# NCalc

[![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/ncalc/ncalc/build-test.yml)](https://github.com/ncalc/ncalc/actions/workflows/build-test.yml)
[![Coverage](https://img.shields.io/codecov/c/github/ncalc/ncalc.svg)](https://codecov.io/gh/ncalc/ncalc)
[![NuGet](https://img.shields.io/nuget/v/NCalcSync.signed.svg?label=nuget&color=004880)](https://nuget.org/packages/NCalcSync.signed)
[![NuGet Downloads](https://img.shields.io/nuget/dt/NCalcSync.svg?color=004880)](https://nuget.org/packages/NCalcSync.signed)
[![Discord](https://img.shields.io/discord/1237181265426387005?color=5b62ef&label=discord)](https://discord.gg/TeJkmXbqFk)

NCalc is a fast, lightweight expression evaluator for .NET. It parses and evaluates mathematical and logical expressions
with literals, operators, parameters, built-in functions, and custom functions.

NCalc targets modern .NET, .NET Standard 2.0, and .NET Framework 4.8.

Want to try it first? Open the [NCalc playground](https://ncalc.github.io/ncalc/playground.html) and run expressions directly in your browser powered by Blazor WASM.

## Documentation

Start with the [NCalc docs](https://ncalc.github.io/ncalc) for the full language reference, runtime options, plugins, and
API reference.

Useful entry points:

- [Language reference](https://ncalc.github.io/ncalc/articles/language/)
- [Evaluation options](https://ncalc.github.io/ncalc/articles/evaluation/)
- [Runtime features](https://ncalc.github.io/ncalc/articles/runtime/)
- [Extensibility and plugins](https://ncalc.github.io/ncalc/articles/extensibility/)

## Install

Use `NCalc` for the expression engine:

```bash
dotnet add package NCalc
```

> [!IMPORTANT]
> NCalc, NCalcSync, and NCalcAsync are redirect packages that currently point directly to NCalc.Core, preserving compatibility from before the community efforts were consolidated into the same repository.

Add optional packages only when you need them:

```bash
dotnet add package NCalc.LambdaCompilation
dotnet add package NCalc.DependencyInjection
```

## Quick Examples

### Evaluate an expression

```csharp
using NCalc;

var result = new Expression("2 + 3 * 5").Evaluate();
// 17
```

### Use parameters

```csharp
var expression = new Expression("Round(Pow([x], 2) + [offset], 2)");
expression.Parameters["x"] = 3.14;
expression.Parameters["offset"] = 10;

var result = expression.Evaluate();
// 19.86
```

### Add custom functions

```csharp
var expression = new Expression("SecretOperation(3, 6)");
expression.Functions["SecretOperation"] = args =>
    (int)args.Evaluate(0) + (int)args.Evaluate(1);

var result = expression.Evaluate();
// 9
```

### Compile to a lambda

Install `NCalc.LambdaCompilation` first.

```csharp
using NCalc.LambdaCompilation;

var expression = new Expression("1 + 2");
Func<int> function = expression.ToLambda<int>();

var result = function();
// 3
```

For data types, operators, built-in functions, async evaluation, caching, expression serialization, and advanced
configuration, check the [docs](https://ncalc.github.io/ncalc).

## Help

Open an [issue](https://github.com/ncalc/ncalc/issues/new/choose) for bugs or usage problems. Include the expression,
the expected result, the actual result, and your NCalc version.

<!--  I'm not giving supoort on discord anymore, please open a issue. For discussion, support, and project news, join the [Discord server](https://discord.gg/TeJkmXbqFk). -->

## Learn More

- [How to execute mathematical expressions in a string in .NET](https://www.jjconsulting.com.br/en-us/blog/programming/ncalc)
- [State of the Art Expression Evaluation](https://www.codeproject.com/Articles/18880/State-of-the-Art-Expression-Evaluation)

## Related Projects

| Project | Description |
| --- | --- |
| [FastExpressionCompiler](https://github.com/dadhi/FastExpressionCompiler) | Fast compiler for C# expression trees. NCalc uses it for lambda compilation. |
| [Jint](https://github.com/sebastienros/jint) | JavaScript interpreter for .NET by [Sebastien Ros](https://github.com/sebastienros), the original author of NCalc. |
| [JJMasterData](https://github.com/JJConsulting/JJMasterData/) | Runtime form generator that uses NCalc for field visibility and other dynamic behavior. |
| [NCalc101](https://ncalc101.magicsuite.net) | Web application for trying NCalc expressions, developed by [Panoramic Data](https://github.com/panoramicdata). |
| [NCalcJS](https://github.com/thomashambach/ncalcjs) | TypeScript/JavaScript port of NCalc. |
| [PanoramicData.NCalcExtensions](https://github.com/panoramicdata/PanoramicData.NCalcExtensions) | Extension functions for NCalc, including string functions, switch, if, in, typeOf, and cast. |
| [Parlot](https://github.com/sebastienros/parlot) | Fast and lightweight parser creation tools by [Sebastien Ros](https://github.com/sebastienros). NCalc uses Parlot in its default parser. |

Using NCalc? Create a PR and add your project here!

## Versioning

NCalc uses [Nerdbank.GitVersioning](https://github.com/dotnet/Nerdbank.GitVersioning). Each library build can be traced
back to the original git commit. Read more in the [release documentation](https://ncalc.github.io/ncalc/articles/project/new_release.html).

## Star History

<a href="https://star-history.com/#ncalc/ncalc&Date">
 <picture>
   <source media="(prefers-color-scheme: dark)" srcset="https://api.star-history.com/svg?repos=ncalc/ncalc&type=Date&theme=dark" />
   <source media="(prefers-color-scheme: light)" srcset="https://api.star-history.com/svg?repos=ncalc/ncalc&type=Date" />
   <img alt="Star History Chart" src="https://api.star-history.com/svg?repos=ncalc/ncalc&type=Date" />
 </picture>
</a>
