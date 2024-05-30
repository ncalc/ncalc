# Compilation of expressions to CLR lambdas

If you need a better preformance you should use compilation of expressions to CLR lambdas feature. 

## Functionalities
**Simple Expressions**

```c#
var expr = new Expression("1 + 2");
Func<int> f = expr.ToLambda<int>();
Debug.Assert(f());
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

var expr = new Expression("Foo([Param1], 2) = 4 && [Param2] = 'test'");
Func<Context, bool> f = expr.ToLambda<Context, bool>();

var context = new Context { Param1 = 2, Param2 = "test" };
Debug.Assert(f(context));
```