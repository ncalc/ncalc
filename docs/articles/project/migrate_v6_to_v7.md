# Migrate from v6 to v7

The new version separates immutable expression configuration from mutable runtime state. This makes configuration safe to share and reuse, while each evaluation can use its own parameters, functions, and handlers.

The compatibility `ExpressionOptions` constructors and the `Expression.Options` setter remain available to ease an incremental migration. Prefer the explicit configuration APIs for all new or updated code.

## Move configuration out of `ExpressionContext`

In v6, `ExpressionContext` held both configuration and runtime values:

```csharp
var context = new ExpressionContext(
    ExpressionOptions.DecimalAsDefault | ExpressionOptions.OverflowProtection,
    CultureInfo.InvariantCulture);

context.StaticParameters["Price"] = 12.5m;

var expression = new Expression("Price * 2", context);
```

In v7, place parsing, evaluation, math, and cache settings in an `ExpressionConfiguration`, and place parameters in an `ExpressionContext`:

```csharp
using NCalc;
using NCalc.Helpers;

var configuration = new ExpressionConfiguration
{
    Parsing = new LogicalExpressionParserOptions
    {
        FloatingPointNumberType = FloatingPointNumberType.Decimal
    },
    Evaluation = new ExpressionEvaluationOptions
    {
        Math = new MathOptions
        {
            FloatingPointNumberType = FloatingPointNumberType.Decimal,
            OverflowProtection = true
        }
    }
};

var context = new ExpressionContext
{
    Parameters =
    {
        ["Price"] = 12.5m
    }
};

var expression = new Expression("Price * 2", configuration, context, CultureInfo.InvariantCulture);
```

`ExpressionConfiguration` and the options it contains should be treated as immutable and can be shared. `ExpressionContext` is mutable and is intended for a single evaluation flow, request, or other isolated scope. See [Configuration](../evaluation/configuration.md) and [Expression Context](../evaluation/expression_context.md) for the current model.

## Update common API usage

| v6 | v7 |
| --- | --- |
| `ExpressionContext.StaticParameters` | `ExpressionContext.Parameters` |
| `ExpressionContext.Options` | `ExpressionConfiguration.Parsing`, `ExpressionConfiguration.Evaluation`, and `ExpressionConfiguration.CacheEnabled` |
| `ExpressionContext.CultureInfo` | `Expression.CultureInfo` or the `cultureInfo` constructor argument |
| `ExpressionContext.MathHelperOptions` | `ExpressionEvaluationOptions.Math` |
| `ExpressionContext.ComparisonOptions` | `ExpressionEvaluationOptions.StringComparer` |
| `Expression.Options` getter | `Expression.Configuration`, `Expression.ParserOptions`, or `Expression.EvaluationOptions` |

`Expression.Options` is now write-only. Assigning it converts the flags to a new `ExpressionConfiguration`:

```csharp
var expression = new Expression("1.2 + 3.4");
expression.Options = ExpressionOptions.DecimalAsDefault;
```

This is useful while migrating, but direct configuration is the recommended permanent form.

## Translate `ExpressionOptions`

Most former flags map directly to a property on the new configuration objects:

| `ExpressionOptions` flag | v7 setting |
| --- | --- |
| `NoCache` | `ExpressionConfiguration.CacheEnabled = false` |
| `AllowCharValues` | `LogicalExpressionParserOptions.AllowCharValues = true` |
| `DecimalAsDefault` | Set `FloatingPointNumberType.Decimal` on both parser and `MathOptions` |
| `LongAsDefault` | Set `IntegerNumberType.Int64` on both parser and `MathOptions` |
| `AllowBooleanCalculation`, `OverflowProtection`, `RoundAwayFromZero` | Properties on `MathOptions` |
| `IgnoreCaseAtBuiltInFunctions`, `IterateParameters`, `AllowNullParameter`, `StringConcat`, `NoStringTypeCoercion`, `AllowNullOrEmptyExpressions`, `ArithmeticNullOrEmptyStringAsZero`, `StrictTypeMatching` | Properties on `ExpressionEvaluationOptions` |
| `CaseInsensitiveStringComparer` and `OrdinalStringComparer` | Choose the appropriate `StringComparer` on `ExpressionEvaluationOptions` |

For example, replace string-comparison flags with a comparer directly:

```csharp
var configuration = new ExpressionConfiguration
{
    Evaluation = new ExpressionEvaluationOptions
    {
        StringComparer = StringComparer.OrdinalIgnoreCase
    }
};
```

## Supply runtime state through the context

`DynamicParameters`, `AsyncParameters`, `Functions`, and `AsyncFunctions` on `Expression` are now read-only views of the dictionaries in its context. Add entries to them, update the corresponding `ExpressionContext` dictionary, or replace the entire context.

```csharp
var expression = new Expression("Discount(Price)");

expression.Context.Functions["Discount"] = args => args.Evaluate(0);

// Replace all runtime state at once when appropriate.
expression.Context = new ExpressionContext
{
    Parameters = { ["Price"] = 20m }
};
```

Events remain available on `Expression`; they are stored by its `ExpressionContext`. When reusing an expression with a new context, attach required handlers to the new context or add them again through the expression.

## Update parser and extension APIs

The parser now receives culture separately from parser options. If your application calls the parser directly, update calls to pass both values:

```csharp
var parserOptions = new LogicalExpressionParserOptions
{
    ArgumentSeparator = ArgumentSeparator.Semicolon
};

var expression = LogicalExpressionFactory.Create(
    "Round(1.25; 1)",
    parserOptions,
    CultureInfo.InvariantCulture);
```

Also rename these public APIs:

| v6 | v7 |
| --- | --- |
| `LogicalExpressionArgumentSeparator` | `ArgumentSeparator` |
| `LogicalExpressionParserContext` | `LogicalExpressionParseContext` |
| `MathHelperOptions` | `MathOptions` |
| `DecimalAsDefault` / `LongAsDefault` parser options | `FloatingPointNumberType` / `IntegerNumberType` |

`LogicalExpression` extension methods `Evaluate(...)` and `EvaluateAsync(...)` were removed. Evaluate through `Expression`, or create the applicable evaluation visitor when working directly with a logical-expression tree.

### Evaluate custom-function arguments

This also changes how handlers evaluate the arguments supplied by `EvaluateFunction`. In NCalc 6, a handler could evaluate the logical-expression arguments by passing the context and cancellation token explicitly:

```csharp
if (name == "SecretOperation")
{
    args.Result = (int)args.Parameters[0].Evaluate(args.Context, args.CancellationToken) +
        (int)args.Parameters[1].Evaluate(args.Context, args.CancellationToken);
}
```

In v7, use `FunctionData.Evaluate(index)`. It evaluates the argument with the function's current context, evaluation options, culture, and cancellation token:

```csharp
if (name == "SecretOperation")
{
    args.Result = (int)args.Parameters.Evaluate(0) +
        (int)args.Parameters.Evaluate(1);
}
```

For asynchronous custom functions, use `await args.Parameters.EvaluateAsync(index)` instead.

## Update custom factories and visitors

Factory and visitor interfaces now accept explicit configuration rather than `ExpressionOptions`:

- `IExpressionFactory.Create` accepts an `ExpressionConfiguration`.
- `ILogicalExpressionFactory.Create` accepts `LogicalExpressionParserOptions?` and `CultureInfo?`.
- `IEvaluationVisitorFactory`, `EvaluationVisitor`, and `AsyncEvaluationVisitor` receive `ExpressionEvaluationOptions` and `CultureInfo`.

Pass `expression.EvaluationOptions` and `expression.CultureInfo` when adapting a custom visitor factory. This preserves the expression's configured evaluation behavior.

## Remove ANTLR usage

The `NCalc.Antlr` plugin and package are no longer part of v7, its a high maintaince cost for us with little benefit. Use the default parser, or provide a custom `ILogicalExpressionFactory` if you require a different parser implementation.
