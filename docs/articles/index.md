# Intro

NCalc is a mathematical expression evaluator in .NET. NCalc can parse any expression and evaluate the result, including static or dynamic parameters and custom functions.

## Project Description

NCalc is a mathematical expression evaluator in .NET. NCalc can parse any expression and evaluate the result, including static or dynamic parameters and custom functions.

## Table of Contents
- [Language](language/index.md): Core language reference for operators, values, functions, and parameters.
- [Evaluation](evaluation/index.md): Behavioral rules and options for expression execution.
- [Runtime](runtime/index.md): Execution-focused topics like async, caching, cancellation, and logging.
- [Extensibility](extensibility/index.md): Lambda compilation, dependency injection, overrides, and plugins.
- [Project](project/index.md): Architecture, benchmarks, changelog, and release process.

## Functionalities

### Simple Expressions

```c#
var expression = new Expression("2 + 3 * 5");
Debug.Assert(17 == expression.Evaluate());
```

### .NET Data Types

```c#
Debug.Assert(123456 == new Expression("123456").Evaluate()); // integers
Debug.Assert(new DateTime(2001, 01, 01) == new Expression("#01/01/2001#").Evaluate()); // datetime
Debug.Assert(123.456 == new Expression("123.456").Evaluate()); // floating point numbers
Debug.Assert(true == new Expression("true").Evaluate()); // booleans
Debug.Assert("azerty" == new Expression("'azerty'").Evaluate()); // strings
```

### Mathematical functional from [System.Math](https://learn.microsoft.com/en-us/dotnet/api/system.math?view=net-8.0)**

```c#
Debug.Assert(0 == new Expression("Sin(0)").Evaluate());
Debug.Assert(2 == new Expression("Sqrt(4)").Evaluate());
Debug.Assert(0 == new Expression("Tan(0)").Evaluate());
```

### Custom Functions

```c#
var expression = new Expression("SecretOperation(3, 6)");
expression.Functions["SecretOperation"] = (args) => {
    return (int)args[0].Evaluate() + (int)args[1].Evaluate();
};

Debug.Assert(9 == expression.Evaluate());
```

### Unicode Characters

```c#
Debug.Assert("経済協力開発機構" == new Expression("'経済協力開発機構'").Evaluate());
Debug.Assert("Hello" == new Expression(@"'\u0048\u0065\u006C\u006C\u006F'").Evaluate());
Debug.Assert("だ" == new Expression(@"'\u3060'").Evaluate());
Debug.Assert("\u0100" == new Expression(@"'\u0100'").Evaluate());
```

### Parameters - Static and Dynamic

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

### Lambda Expressions
```cs
var expression = new Expression("1 + 2");
Func<int> function = expression.ToLambda<int>();
Debug.Assert(function()); //3
```
