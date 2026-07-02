# Typed Evaluation

NCalc can return a typed result directly through `Evaluate<T>()` and `EvaluateAsync<T>()`.
These overloads are useful when the expected result type is already known, and you want to avoid manual casts in calling code.

## Synchronous typed evaluation

```csharp
var expression = new Expression("1 + 2");

var result = expression.Evaluate<int>();
Debug.Assert(result == 3);
```

## Asynchronous typed evaluation

```csharp
var expression = new Expression("database_operation('SELECT COUNT(*)')");

expression.AsyncFunctions["database_operation"] = async args =>
{
    await Task.Delay(100, args.CancellationToken);
    return 3;
};

var result = await expression.EvaluateAsync<int>();
Debug.Assert(result == 3);
```

## Conversion behavior

Both typed overloads first evaluate the expression normally and then apply the following rules:

- If the result is `null`, they return `default(T)`.
- If the result is already of type `T`, they return it directly.
- If the result implements `IConvertible`, NCalc converts it with `Convert.ChangeType` using `ExpressionContext.CultureInfo`.
- If the result cannot be converted to `T`, NCalc throws `NCalcEvaluationException`.

```csharp
var expression = new Expression("'10'");

var result = expression.Evaluate<int>();
Debug.Assert(result == 10);
```

## When to use typed evaluation

Prefer `Evaluate<T>()` or `EvaluateAsync<T>()` when:

- The expression has a stable expected result type such as `bool`, `int`, `decimal`, or `DateTime`.
- You want conversion to follow the expression context culture.
- You want a single API call instead of evaluating to `object` and casting manually.

Use the non-generic `Evaluate()` or `EvaluateAsync()` overloads when the result type is not known ahead of time or may vary between evaluations.
