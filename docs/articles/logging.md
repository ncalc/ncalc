# Logging

NCalc integrates with [`Microsoft.Extensions.Logging`](https://www.nuget.org/packages/Microsoft.Extensions.Logging.Abstractions).
There is no built-in console or trace switch anymore. Logging follows the standard .NET logging pipeline and is only emitted when you provide loggers through dependency injection.

## Default behavior

If you use NCalc without DI, the built-in singleton implementations use `NullLoggerFactory`. In practice, this means logging is disabled by default.

This applies to:

- `LogicalExpressionCache.GetInstance()`
- `LogicalExpressionFactory.GetInstance()`

## Logged events

The current built-in log messages are:

- `Information` `EventId=0`: expression retrieved from cache
- `Information` `EventId=1`: expression added to cache
- `Information` `EventId=2`: expression removed from cache
- `Error` `EventId=3`: error creating a logical expression

## Using logging with DI

When you register NCalc with [`NCalc.DependencyInjection`](dependency_injection.md), NCalc uses the application's configured `ILoggerFactory` for services created by the container.

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NCalc.DependencyInjection;

var services = new ServiceCollection();

services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});

services.AddNCalc();
```

With the default registrations, `ILogicalExpressionCache` is created by DI, so cache events are logged through `ILogger<LogicalExpressionCache>`.

## Important note about `ILogicalExpressionFactory`

`AddNCalc()` currently registers `ILogicalExpressionFactory` by returning `LogicalExpressionFactory.GetInstance()`. That singleton is created with `NullLoggerFactory`, so the built-in parse-error log is not emitted through DI in that default registration.

If you want logging from the logical expression factory as well, replace the registration with your own implementation of <xref:NCalc.Factories.ILogicalExpressionFactory>, or register a DI-created factory instance yourself.

For general guidance on configuring providers, filters, and sinks, see the [Microsoft logging documentation](https://learn.microsoft.com/en-us/dotnet/core/extensions/logging).
