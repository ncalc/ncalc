# Async Support

NCalc supports asynchronous evaluation through `Expression.EvaluateAsync`. Unlike popular belief, `async` is not faster than `sync`, but scales better.
It's recommended to use `async` if your expression is doing any CPU or IO bound work in a dynamic function or parameter, so your UI does not freeze or your web server can keep handling other work.
Learn more about `async` [in this article](https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/async-scenarios).

# Usage
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

## Async Binary Handlers

Binary operators can also be overridden during async evaluation through
<xref:NCalc.Expression.EvaluateBinaryAsync>, which uses the
<xref:NCalc.Handlers.EvaluateBinaryAsyncHandler> delegate.

The handler receives <xref:NCalc.Handlers.BinaryEventArgs>, so you can lazily evaluate operands with
`LeftValueAsync()` and `RightValueAsync()` and then assign `Result` to bypass the built-in operator behavior.

```csharp
var expression = new Expression("[A] * [B]");

expression.Parameters["A"] = new Dummy(10);
expression.Parameters["B"] = 12;

expression.EvaluateBinaryAsync += async args =>
{
    if (args.BinaryExpression.Type != BinaryExpressionType.Times)
        return;

    var left = await args.LeftValueAsync();
    if (left is Dummy leftDummy)
        left = leftDummy.Value;

    var right = await args.RightValueAsync();
    if (right is Dummy rightDummy)
        right = rightDummy.Value;

    args.Result = Convert.ToDecimal(left) * Convert.ToDecimal(right);
};

var result = await expression.EvaluateAsync();
Debug.Assert((decimal)result! == 120m);

private record Dummy(int Value);
```

When both handlers are registered, NCalc evaluates `EvaluateBinary` first and then `EvaluateBinaryAsync`. If neither
handler sets `Result`, the built-in operator implementation runs as usual.
