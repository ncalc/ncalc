## Cancellation Token Support

NCalc allows passing a `CancellationToken` when creating or evaluating an expression.
The token is forwarded to the parser and to all custom evaluation handlers invoked during `Evaluate` or `EvaluateAsync`.

NCalc does not cancel execution by itself except at parsing.
Subscribers are responsible for honoring the token inside their handlers.

### Example

```csharp
var cts = new CancellationTokenSource();

var expression = new AsyncExpression("getUserName(1) == 'admin'");
expression.Functions["getUserName"] = async args =>
{
    var id = (int)(await args[0].EvaluateAsync(args.CancellationToken)!)!;

    args.CancellationToken.ThrowIfCancellationRequested();

    using var db = new AppDbContext();

    return await db.Users
        .Where(u => u.Id == id)
        .Select(u => u.Name)
        .FirstAsync(args.CancellationToken);
};

var result = await expression.EvaluateAsync(args.CancellationToken);
```

### Learn more about cancellation tokens

Microsoft docs: https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken
