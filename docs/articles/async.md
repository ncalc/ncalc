# Async Support

NCalc supports running asynchronous evaluating an expression. Unlike popular belief, `async` is not faster than `sync`, but scales better.
It's recommended to use `async` if your expression is doing any CPU or IO bound work, so your UI don't freeze or your web-server can another things.
Learn more about `async` [in this article](https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/async-scenarios).

# Usage
```cs
var expression = new Expression("database_operation('SELECT FOO') == 'FOO'");
expression.EvaluateFunctionAsync += async (name, args) =>
{
    if (name == "database_operation")
    {
        //My heavy database work.
        await Task.Delay(100);

        args.Result = "FOO";
    }
};

var result = await expression.EvaluateAsync();
Debug.Assert(true,result);
```