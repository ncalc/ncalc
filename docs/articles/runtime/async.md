# Async Support

NCalc supports asynchronous evaluation through `Expression.EvaluateAsync`.
Async evaluation is meant for expressions that call asynchronous custom code, such as database queries, HTTP APIs,
file operations, queues, or other I/O-bound work.

`async` is not faster than synchronous execution. It usually adds a small amount of overhead, but lets the caller release
the current thread while waiting for I/O. For CPU-bound workloads, especially workloads that evaluate many expressions or
call many custom functions, prefer `Expression.Evaluate`.
Learn more about `async` [in this article](https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/async-scenarios).

## Choosing Evaluate or EvaluateAsync

NCalc keeps the synchronous and asynchronous evaluation paths separate:

| API | Use when | Custom callbacks used |
| --- | --- | --- |
| `Evaluate()` | The expression is CPU-bound or all custom work is synchronous. | `Functions`, `DynamicParameters`, `EvaluateFunction`, `EvaluateBinary`, `EvaluateParameter` |
| `EvaluateAsync()` | The expression may call async custom functions, parameters, or binary handlers. | Sync callbacks first, then async callbacks when needed |

This split avoids sync-over-async in `Evaluate()`. In other words, synchronous evaluation does not block on
`Task`/`ValueTask` returned by async callbacks. That keeps CPU-bound workloads close to the performance profile of a
fully synchronous evaluator.

Async callbacks registered on an expression do not change normal synchronous evaluation:

- `Evaluate()` does not invoke `AsyncFunctions`.
- `Evaluate()` does not invoke `AsyncParameters`.
- `Evaluate()` does not invoke `EvaluateAsyncFunction`.
- `Evaluate()` does not invoke `EvaluateAsyncParameter`.
- `Evaluate()` does not invoke `EvaluateBinaryAsync`.
- If a function only exists in `AsyncFunctions`, `Evaluate()` treats it like an unknown function and the normal function-not-found behavior applies.
- If a parameter only exists in `AsyncParameters`, `Evaluate()` treats it like an undefined parameter and the normal parameter-not-defined behavior applies.
- If both sync and async handlers are registered, `Evaluate()` uses only the sync handlers.

Use `EvaluateAsync()` whenever the expression is expected to use async callbacks.

## Usage

```csharp
var expression = new Expression("database_operation('SELECT FOO') == 'FOO'");

expression.AsyncFunctions["database_operation"] = async args =>
{
    // My heavy database work.
    await Task.Delay(100, args.CancellationToken);

    return "FOO";
};

var result = await expression.EvaluateAsync();
Debug.Assert((bool)result!);
```

When the expected result type is known, use `EvaluateAsync<T>()`:

```csharp
var countExpression = new Expression("database_operation('SELECT COUNT(*)')");

countExpression.AsyncFunctions["database_operation"] = async args =>
{
    await Task.Delay(100, args.CancellationToken);
    return 3;
};

var count = await countExpression.EvaluateAsync<int>();
Debug.Assert(count == 3);
```

## Async Function Handlers

`EvaluateAsyncFunction` is only used by `EvaluateAsync()`. During async evaluation, NCalc invokes
`EvaluateFunction` first. If the synchronous handler sets `Result`, the async handler is skipped. If the synchronous
handler does not set `Result`, NCalc invokes `EvaluateAsyncFunction`.

```csharp
var expression = new Expression("database_operation('SELECT FOO') == 'FOO'");

expression.EvaluateAsyncFunction += async (name, args) =>
{
    if (name != "database_operation")
        return;

    await Task.Delay(100, args.CancellationToken);
    args.Result = "FOO";
};

var result = await expression.EvaluateAsync();
Debug.Assert((bool)result!);
```

Calling `Evaluate()` for the same expression does not invoke `EvaluateAsyncFunction`. Register a synchronous
`EvaluateFunction` handler or a `Functions` entry if the expression must also work through `Evaluate()`.

## Async Parameters

Parameters follow the same async rules as functions during `EvaluateAsync()`:

- `EvaluateParameter` runs first.
- If it does not set `Result`, `EvaluateAsyncParameter` runs.
- If no event handler resolves the parameter, NCalc checks `Parameters`, then `DynamicParameters`, then `AsyncParameters`.
- The synchronous `Evaluate()` path only uses `Parameters`, `DynamicParameters`, and `EvaluateParameter`.

```csharp
var expression = new Expression("userName == 'alice'");

expression.AsyncParameters["userName"] = async args =>
{
    await Task.Delay(100, args.CancellationToken);
    return "alice";
};

var result = await expression.EvaluateAsync();
Debug.Assert((bool)result!);
```

```csharp
var expression = new Expression("tenantId == 42");

expression.EvaluateAsyncParameter += async (name, args) =>
{
    if (name != "tenantId")
        return;

    await Task.Delay(100, args.CancellationToken);
    args.Result = 42;
};

var result = await expression.EvaluateAsync();
Debug.Assert((bool)result!);
```
