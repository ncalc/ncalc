# Lambda Compilation

Using the concept
of [lambda compilation](https://learn.microsoft.com/en-us/dotnet/api/system.linq.expressions.expression-1.compile?view=net-8.0),
NCalc can convert a <xref:NCalc.Domain.LogicalExpression> object to an anonymous function.
Using this you can write complex functions and even have greater performance when evaluating the expression.

Especial thanks to the [NCalc2 fork](https://github.com/sklose/NCalc2) for the original implementation.

## Functionalities

**Simple Expressions**

```c#
var expression = new Expression("1 + 2");
Func<int> function = expression.ToLambda<int>();
Debug.Assert(function()); //3
```

**Expressions with Functions and Parameters**

```c#
class Context
{
    public int Param1 { get; set; }
    public string Param2 { get; set; }
  
    public int Foo(int a, int b)
    {
        return a + b;
    }
}

var expression = new Expression("Foo([Param1], 2) = 4 && [Param2] = 'test'");
Func<Context, bool> function = expression.ToLambda<Context, bool>();

var context = new Context { Param1 = 2, Param2 = "test" };
Debug.Assert(function(context)); //true
```

## Compatibility
Since v5.5 by default NCalc using a [FastExpressionCompiler](https://github.com/dadhi/FastExpressionCompiler)
 to improve the compilation performance (which is 10-40x times faster than .Compile()). But if you faced some issues, you can switch back to built-in System.Linq.Expressions .Compile() method by using AppContext switch:

`AppContext.SetSwitch("NCalc.UseSystemLinqCompiler", true)` 

## Performance
You should cache the result of <xref:NCalc.Expression.ToLambda``1>. The evaluation is indeed faster, but the compilation of the lambda is slow.
See [benchmarks](benchmarks.md) for more info.