# Dependency Injection

## Introduction to Dependency Injection (DI)

Dependency Injection (DI) is a design pattern used to implement Inversion of Control (IoC) between classes and their
dependencies. It allows the creation of dependent objects outside a class and provides those objects to the class in
various ways. This leads to more flexible, reusable, and testable code. For more information on Dependency Injection,
refer to the [Microsoft documentation](https://docs.microsoft.com/en-us/dotnet/core/extensions/dependency-injection).

## Installation and Usage

First, you will need to add a reference to `NCalc.DependencyInjection` to your project.
If you are using ASP.NET Core, no need to install a DI container.
If you are in a Console App or older framework, you will need to set up a DI
container, [Microsoft.Extensions.DependencyInjection](https://www.nuget.org/packages/Microsoft.Extensions.DependencyInjection)
is a DI container for example.

```shell
dotnet add package NCalc.DependencyInjection
```

At your `Program.cs` simply:

```cs
builder.Services.AddNCalc();

builder.Services.AddTransient<MyService>(); // This is just an example.
```

You will need to use <xref:NCalc.Factories.IExpressionFactory> to create expressions with injected services.

```cs
using NCalc.Factories

public class MyService(IExpressionFactory expressionFactory)
{
    public object? EvaluateExpression(string expressionString)
    {
        var expression = expressionFactory.Create(expression, ExpressionOptions.DecimalAsDefault);
        return expression.Evaluate(expressionString);
    }
}
```

## Methods

See <xref:NCalc.DependencyInjection.NCalcServiceBuilder> to see all methods.

### `WithExpressionFactory`
Use this method to specify a custom implementation of <xref:NCalc.Factories.IExpressionFactory>. This factory is
responsible for creating
<xref:NCalc.Expression> objets that NCalc will evaluate. You can for example create a custom implementation with an
object pool to re-use expression objects.

### `WithCache`

Use this method to specify a custom implementation of <xref:NCalc.Cache.ILogicalExpressionCache>. This cache is used to
store and
retrieve parsed <xref:NCalc.Domain.LogicalExpression> objects.

**Example:**

```csharp
services.AddNCalc()
        .WithCache<MyCustomCache>();
```

### `WithLogicalExpressionFactory`
Use this method to specify a custom implementation of <xref:NCalc.Factories.ILogicalExpressionFactory>. This factory is
responsible for creating
<xref:NCalc.Domain.LogicalExpression> objects. These objects represent a parsed string into an expression. You can for
example create a custom parser using another library instead of [Parlot](https://github.com/sebastienros/parlot) and
implement this interface.

**Example:**

```csharp
services.AddNCalc()
        .WithLogicalExpressionFactory<MyCustomLogicalExpressionFactory>();
```

### `WithEvaluationVisitorFactory`
Use this method to specify a custom implementation of <xref:NCalc.Factories.IEvaluationVisitorFactory>.
The created evaluation visitor is used to calculate the result of your expression after parsing.

**Example:**

```csharp
services.AddNCalc()
        .WithEvaluationVisitorFactory<MyCustomEvaluationVisitorFactory>();
```

By configuring these services, you can customize the behavior of NCalc to suit your application's needs.