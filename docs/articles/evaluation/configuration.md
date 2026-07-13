# Configuration

NCalc separates expression configuration from runtime state.

Use <xref:NCalc.ExpressionConfiguration> for parsing, evaluation, math, comparison, and cache settings. Use
<xref:NCalc.ExpressionContext> for runtime data such as parameters, functions, and event handlers.

```csharp
using NCalc;
using NCalc.Helpers;
using NCalc.Parser;

static readonly ExpressionConfiguration Configuration = new(
    parserOptions: new LogicalExpressionParserOptions
    {
        AllowCharValues = true,
        FloatingPointNumberType = FloatingPointNumberType.Decimal,
        IntegerNumberType = IntegerNumberType.Int32,
        ArgumentSeparator = ArgumentSeparator.Comma | ArgumentSeparator.Semicolon
    },
    evaluationOptions: new ExpressionEvaluationOptions
    {
        IgnoreCaseAtBuiltInFunctions = true,
        StringComparer = StringComparer.OrdinalIgnoreCase,
        Math = new MathOptions
        {
            FloatingPointNumberType = FloatingPointNumberType.Decimal,
            OverflowProtection = true,
            MidpointRounding = MidpointRounding.AwayFromZero
        }
    });

var expression = new Expression("ROUND('1.25'; 1)", Configuration, CultureInfo.InvariantCulture);
```

## Thread safety

<xref:NCalc.ExpressionConfiguration> and the option objects it owns are thread-safe when treated as immutable:

- <xref:NCalc.Parser.LogicalExpressionParserOptions>
- <xref:NCalc.ExpressionEvaluationOptions>
- <xref:NCalc.Helpers.MathOptions>

Create them once, cache them, and reuse them across expressions. This avoids repeatedly allocating configuration objects
and keeps configuration shared consistently across your application.

<xref:NCalc.ExpressionContext> is not thread-safe. It contains mutable dictionaries and event handlers for a single
evaluation flow. Create a new context per evaluation, request, or mutable scope.

```csharp
static readonly ExpressionConfiguration Configuration = new()
{
    Evaluation = new ExpressionEvaluationOptions
    {
        StringComparer = StringComparer.CurrentCultureIgnoreCase,
        Math = new MathOptions
        {
            OverflowProtection = true
        }
    }
};

public object? Evaluate(string expressionText, IDictionary<string, object?> parameters)
{
    var context = new ExpressionContext(parameters);
    var expression = new Expression(expressionText, Configuration, context, CultureInfo.InvariantCulture);

    return expression.Evaluate();
}
```

## Dependency Injection

Because configuration is immutable and thread-safe, register it as a singleton when using dependency injection:

```csharp
builder.Services.AddSingleton(new ExpressionConfiguration
{
    Parsing = new LogicalExpressionParserOptions
    {
        FloatingPointNumberType = FloatingPointNumberType.Decimal
    },
    Evaluation = new ExpressionEvaluationOptions
    {
        StringComparer = StringComparer.OrdinalIgnoreCase,
        Math = new MathOptions
        {
            FloatingPointNumberType = FloatingPointNumberType.Decimal,
            OverflowProtection = true
        }
    }
});
```

Then inject it anywhere expressions are created:

```csharp
public class RuleEvaluator(
    IExpressionFactory expressionFactory,
    ExpressionConfiguration configuration)
{
    public object? Evaluate(string expressionText, IDictionary<string, object?> parameters)
    {
        var expression = expressionFactory.Create(expressionText, configuration);
        expression.Parameters = parameters;

        return expression.Evaluate();
    }
}
```

## ExpressionOptions

<xref:NCalc.ExpressionOptions> is kept as a legacy helper, it will convert old flag-based configuration into <xref:NCalc.ExpressionConfiguration>:

```csharp
var expression = new Expression("1.2 + 3.4",  ExpressionOptions.DecimalAsDefault | ExpressionOptions.CaseInsensitiveStringComparer);
```

The <xref:NCalc.Expression.Options> setter also replaces the expression's
current <xref:NCalc.Expression.Configuration> with the configuration created by
<xref:NCalc.ExpressionConfiguration.FromOptions(NCalc.ExpressionOptions)>:

```csharp
var expression = new Expression("Round(22.5, 0)");
expression.Options = ExpressionOptions.RoundAwayFromZero;
```

For new code, prefer constructing <xref:NCalc.ExpressionConfiguration>, <xref:NCalc.Parser.LogicalExpressionParserOptions>,
<xref:NCalc.ExpressionEvaluationOptions>, and <xref:NCalc.Helpers.MathOptions> directly.

Using <xref:NCalc.ExpressionConfiguration> directly also avoids using `Enum.HasFlag` because of its unnecessary overhead.
