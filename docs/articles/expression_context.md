# Expression Context

<xref:NCalc.ExpressionContext> (or <xref:NCalc.AsyncExpressionContext>) at [NCalcAsync](async.md), is the class
responsible for storing contextual data of the expression, like [parameters](parameters.md)
and [functions](functions.md).

## Creating a context

```csharp
//Explicit
var context = new ExpressionContext
{
    Options = ExpressionOptions.IgnoreCaseAtBuiltInFunctions
};
var expression = new Expression("ABS(42)", context);

//Implicit
ExpressionContext context = ExpressionOptions.IgnoreCaseAtBuiltInFunctions;
var expression = new Expression("ABS(42)", context);

//Cloning
ExpressionContext context = ExpressionOptions.IgnoreCaseAtBuiltInFunctions;
var newContext = context with { CultureInfo = CultureInfo.CurrentUICulture }
var expression = new Expression("ABS(42)", context);
```

## Using a single context for all expressions

You can optimize your system to use a single context instance for all expressions.
The example below use [DI](dependency_injection.md) for creating a singleton instance.

```csharp
///At Program.cs:
builder.Services.AddSingleton<ExpressionContext>(_ =>
{
    return new ExpressionContext
    {
        Options = ExpressionOptions.OverflowProtection,
        Functions = new Dictionary<string, ExpressionFunction>()
        {
            {"SecretOperation", (arguments, context) => 42}
        }.ToFrozenDictionary()
    };
});

///At MyService.cs
public class MyService(IExpressionFactory expressionFactory, ExpressionContext context)
{
    public object? Evaluate(string expressionString)
    {
        //Using the `with` keyword is possible to create a copy with different options.
        var expression = expressionFactory.Create(expressionString, context with { Options = ExpressionOptions.IgnoreCaseAtBuiltInFunctions });
        return expression.Evaluate();
    }
}
```

> [!WARNING]
> The [with]() keyword will create a shallow copy of the object, any unchanged reference type like `IDictionary` inside
> the context will remain the same.