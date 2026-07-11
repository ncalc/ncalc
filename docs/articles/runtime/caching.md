## Caching

When <xref:NCalc.Expression.Evaluate(System.Threading.CancellationToken)> is called on an expression, it is parsed once. If the same expression is reused, parsing is skipped and only the expression tree is evaluated again.

Each parsed expression is cached internally, which means you do not need to manually reuse an <xref:NCalc.Expression> instance for the parser cache to help.

The default cache keeps strong references and evicts the least recently used entries when it reaches its capacity.

You can disable caching with <xref:NCalc.ExpressionConfiguration.CacheEnabled>.

```csharp
static readonly ExpressionConfiguration ExpressionConfiguration = new()
{
    CacheEnabled = false
};
```
```

```csharp
var configuration = ExpressionConfiguration.FromOptions(ExpressionOptions.NoCache);
```

## Default cache size

The default cache size is `128` entries.

If you are on modern .NET runtimes (.NET 8+) you can override it before the first expression is parsed by setting an `AppContext` value:

```csharp
AppContext.SetData("NCalc.LogicalExpressionCache.DefaultCapacity", "256");
```

The value must be a positive integer string. Set it during application startup so the default singleton cache picks it up before first use.

You can customize the cache implementation using [Dependency Injection](../extensibility/dependency_injection.md).

If you need expiration control or want the cache to be governed by `IMemoryCache`, use the [Memory Cache plugin](../extensibility/plugins/memory_cache.md) instead of the built-in cache.
