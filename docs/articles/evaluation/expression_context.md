# Expression Context

<xref:NCalc.ExpressionContext> stores runtime data for an expression evaluation:
[parameters](../language/parameters.md), [functions](../language/functions.md), async callbacks, and event handlers.

Use [configuration](configuration.md) for parser and evaluation settings.

## Creating a context

```csharp
var context = new ExpressionContext
{
    Parameters =
    {
        ["Price"] = 12.5m,
        ["Quantity"] = 4
    },
    Functions =
    {
        ["SecretOperation"] = _ => 42
    }
};

var expression = new Expression("Price * Quantity + SecretOperation()", context);
var result = expression.Evaluate();
```

To use configuration and context together, pass both to the expression:

```csharp
var configuration = new ExpressionConfiguration
{
    Evaluation = new ExpressionEvaluationOptions
    {
        IgnoreCaseAtBuiltInFunctions = true
    }
};

var context = new ExpressionContext
{
    Parameters =
    {
        ["Value"] = -42
    }
};

var expression = new Expression("ABS(Value)", configuration, context);
var result = expression.Evaluate();
```

## Lifetime

<xref:NCalc.ExpressionContext> is mutable and not thread-safe. Do not cache a single context as global shared state because
parameters, functions, or handlers can change.

Create a context per evaluation, request, or operation:

```csharp
public class EvaluationService(ExpressionConfiguration configuration)
{
    public object? Evaluate(string expressionText, IDictionary<string, object?> parameters)
    {
        var context = new ExpressionContext(parameters);
        var expression = new Expression(expressionText, configuration, context);
    
        return expression.Evaluate();
    }
}
```
