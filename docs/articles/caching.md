## Caching

When <xref:NCalc.Expression.Evaluate> is called on an expression, it is parsed once. If the same expression is reused the parse is not executed again. Thus, you can reuse <xref:NCalc.Expression>  instances by changing the parameters, and you will gain in performance because only the traversal of the expression tree will be done.

Moreover, each parsed expression is cached internally, which means you don't even have to care about reusing an <xref:NCalc.Expression> instance, the framework will do it for you.
The cache is automatically cleaned like the GC does when an Expression is no more used, or memory is needed (i.e. using [WeakReference<LogicalExpression>](https://learn.microsoft.com/en-us/dotnet/api/system.weakreference-1?view=net-8.0)).

You can disable this behavior at the framework level by setting false to `CacheEnabled`.

```c#
 Expression.CacheEnabled = false;
```

You can also tell a specific <xref:NCalc.Expression> instance not to be taken from the cache.

```c#
 var expression = new Expression("1 + 1", ExpressionOptions.NoCache);
```

You can customize the cache implementation using [Dependency Injection](dependency_injection.md).