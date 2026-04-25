# Async Support

NCalc supports asynchronous evaluating an expression. Unlike popular belief, `async` is not faster than `sync`, but scales better.
It's recommended to use `async` if your expression is doing any CPU or IO bound work at some dynamic function or parameter, so your UI don't freeze or your web-server do can another things.
Learn more about `async` [in this article](https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/async-scenarios).

# Install
```shell
dotnet add package NCalcAsync 
```

# Usage
```cs
var expression = new AsyncExpression("database_operation('SELECT FOO') == 'FOO'");

expression.Functions["database_operation"] = async (args) => {
    // My heavy database work.
    await Task.Delay(100);

    return "FOO";
};

var result = await expression.EvaluateAsync();
Debug.Assert(true,result);
```