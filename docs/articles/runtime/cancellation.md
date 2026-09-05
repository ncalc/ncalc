## Cancellation Token Support

NCalc allows passing a `CancellationToken` when creating or evaluating an expression.
The token is forwarded to the parser and to all custom evaluation handlers invoked during `Evaluate` or `EvaluateAsync`.

NCalc does not cancel execution by itself except at parsing.
Subscribers are responsible for honoring the token inside their handlers.

### Example

```csharp
var cts = new CancellationTokenSource();

var expression = new Expression("getUserName(1) == 'admin'");
expression.AsyncFunctions["getUserName"] = async args =>
{
    var id = Convert.ToInt32(await args[0].EvaluateAsync(args.CancellationToken));

    args.CancellationToken.ThrowIfCancellationRequested();

    using var db = new AppDbContext();

    return await db.Users
        .Where(u => u.Id == id)
        .Select(u => u.Name)
        .FirstAsync(args.CancellationToken);
};

var result = await expression.EvaluateAsync(cts.Token);
```

### Learn more about cancellation tokens

Microsoft docs: https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken
