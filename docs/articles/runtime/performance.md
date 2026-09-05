# Performance

Most NCalc workloads spend time in parsing, evaluation, parameter lookup, and custom callback code. The best
optimization is to keep parsing and configuration out of the hot path.

## Cache configuration

<xref:NCalc.ExpressionConfiguration> and its nested option objects are thread-safe when treated as immutable. Create
them once and reuse them.

```csharp
builder.Services.AddSingleton(new ExpressionConfiguration
{
    Evaluation = new ExpressionEvaluationOptions
    {
        StringComparer = StringComparer.OrdinalIgnoreCase,
        Math = new MathOptions
        {
            OverflowProtection = true
        }
    }
});
```

Do not cache <xref:NCalc.ExpressionContext> as shared mutable state. It contains dictionaries and event handlers for a
single evaluation flow. Create a new context for each request, operation, or independent evaluation scope.

## Reuse expressions for repeated evaluation

If the same expression text is evaluated repeatedly, keep the <xref:NCalc.Expression> instance and update only its
runtime parameters.

```csharp
var expression = new Expression("Price * Quantity", configuration);

expression.Parameters["Price"] = 10m;
expression.Parameters["Quantity"] = 2;
var first = expression.Evaluate();

expression.Parameters["Price"] = 12m;
expression.Parameters["Quantity"] = 4;
var second = expression.Evaluate();
```

Use separate expression instances when evaluating concurrently because parameters and handlers are mutable.

## Prefer dictionaries for simple parameters and functions

For simple values, use <xref:NCalc.Expression.Parameters>. For simple functions, use <xref:NCalc.Expression.Functions>.
Event handlers and dynamic callbacks are useful extension points, but they add more dispatch and user code to each
evaluation.

When parameter or function names should be case-insensitive, use a dictionary with the desired
<xref:System.StringComparer> instead of normalizing names on each call.

```csharp
var context = new ExpressionContext
{
    Parameters = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
    {
        ["tenantId"] = 42
    }
};
```

## Compile hot expressions

For very hot expressions that are compatible with lambda compilation, use `NCalc.LambdaCompilation` and cache the
compiled delegate. Compiling is expensive; the performance benefit comes from compiling once and invoking many times.

```csharp
var expression = new Expression("A + B");
var lambda = expression.ToLambda<MyContext, int>();

var result = lambda(new MyContext { A = 1, B = 2 });
```

See [Lambda Compilation](../extensibility/lambda_compilation.md) and [Benchmarks](../project/benchmarks.md).

## Measure your workload

Performance depends heavily on expression shape, parameter sources, custom functions, and allocation patterns. Use
BenchmarkDotNet or another profiler against representative expressions before trading readability for micro-optimizations.
