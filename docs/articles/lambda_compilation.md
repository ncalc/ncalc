﻿# Compilation of expressions to CLR lambdas

If you need a better performance you should use compilation of expressions to CLR lambdas feature. 
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