# Memory Cache

## Introduction

If you want to use [IMemoryCache](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.caching.memory.imemorycache) to cache the parsing of expressions, you can use this plugin. The advantage of this plugin is that you can set a custom cache expiration.

## Installation and Usage

First, you will need to add a reference to `NCalc.MemoryCache` to your project.
```shell
dotnet add package NCalc.MemoryCache
```

After this, you will need DI in your project. See [Dependency Injection](../dependency_injection.md) for more info.

Add a [IMemoryCache](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.caching.memory.imemorycache) implementation to your project. 
In this example, we will use [Microsoft.Extensions.Caching.Memory](https://www.nuget.org/packages/Microsoft.Extensions.Caching.Memory).

```
builder.Services.AddMemoryCache() //From Microsoft.Extensions.Caching.Memory
builder.Services.AddNCalc().WithMemoryCache(options =>
{
    options.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5); //The default value is 15 minutes.
})
```

After this, simply create expressions from <xref:NCalc.Factories.IExpressionFactory>. For more information see [this article](../dependency_injection.md).