# Intro

NCalc is a mathematical expressions evaluator in .NET. NCalc can parse any expression and evaluate the result, including static or dynamic parameters and custom functions.

## Project Description

NCalc is a mathematical expressions evaluator in .NET. NCalc can parse any expression and evaluate the result, including static or dynamic parameters and custom functions.

For additional information on the technique we used to create this framework please read this article: http://www.codeproject.com/KB/recipes/sota_expression_evaluator.aspx.

## Table of Contents
- [Operators](operators.md): Available standard operators and structures.
- [Values](values.md): Authorized values like types and functions.
- [Functions](functions.md):  List of already implemented functions.
- [Parameters](parameters.md):  How to use parameters expressions.
- [Handling Errors](handling_errors.md):  How to handle errors.
- [Case Sensitivity](case_sensitivity.md): Options in how to handle case sensitivity.
- [Caching](caching.md): How caching works.
- [Dependency Injection](dependency_injection.md): Bring expressions to the next level with dependency injection.

## <xref:NCalc.Expression> class
This is the main class of NCalc.
This class has a method <xref:NCalc.Expression.Evaluate> returning the actual value of its <xref:System.String> representation.

Example:

```c#
  var expression = new Expression("2 * 3");
  object result = e.Evaluate();
  
  Console.WriteLine(result);
```

This example above first creates an instance of <xref:NCalc.Expression> using a valued constructor. This constructor takes a <xref:System.String> as parameter.
Then the method <xref:NCalc.Expression.Evaluate> is called to parse the <xref:System.String>, and returns the actual value represented by the <xref:System.String>.

To create expressions you can combine several [Operators](operators.md) and [Values](values.md).

## Functionalities
**Simple Expressions**

```c#
var expression = new Expression("2 + 3 * 5");
Debug.Assert(17 == e.Evaluate());
```

**Evaluates .NET data types**

```c#
Debug.Assert(123456 == new Expression("123456").Evaluate()); // integers
Debug.Assert(new DateTime(2001, 01, 01) == new Expression("#01/01/2001#").Evaluate()); // datetime
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
var expression = new Expression("Round(Pow([Pi], 2) + Pow([Pi2], 2) + [X], 2)");

e.Parameters["Pi2"] = new Expression("Pi * [Pi]");
e.Parameters["X"] = 10;

e.EvaluateParameter += delegate(string name, ParameterArgs args)
  {
    if (name == "Pi")
    args.Result = 3.14;
  };

Debug.Assert(117.07 == e.Evaluate());
```