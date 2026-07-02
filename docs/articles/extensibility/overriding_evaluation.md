# Overriding Evaluation Behavior

NCalc provides a flexible way to evaluate expressions, and in some cases, you may need to customize its evaluation
behavior. You can do that by overriding the evaluation visitors, and, when you want NCalc to resolve them for you,
by plugging in `IEvaluationVisitorFactory` through `Expression` or dependency injection.

## Custom Evaluation Visitor

By creating a custom evaluation visitor, you can modify how specific expressions are interpreted. The following
example overrides the behavior of value expressions, returning a special string when encountering the number `42`:

```csharp
private class CustomVisitor(ExpressionContext context) : EvaluationVisitor(context)
{
    public override object? Visit(ValueExpression expression)
    {
        if (expression.Value is 42)
            return "The answer";

        return base.Visit(expression);
    }
}
```

If you are using a factory, the same visitor can still accept it in the constructor so nested evaluations keep the
same configuration.

If you also need to customize the async path, you can apply the same pattern:

```csharp
private class CustomAsyncVisitor(ExpressionContext context) : AsyncEvaluationVisitor(context)
{
    public override Task<object?> Visit(ValueExpression expression)
    {
        if (expression.Value is 42)
            return Task.FromResult<object?>("The answer async");

        return base.Visit(expression);
    }
}
```

## Custom Evaluation Visitor Factory

If you want NCalc to create your custom visitors for you, implement `IEvaluationVisitorFactory` and return the
visitors from there.

```csharp
private class CustomEvaluationVisitorFactory : IEvaluationVisitorFactory
{
    public EvaluationVisitor CreateEvaluationVisitor(ExpressionContext context, CancellationToken cancellationToken = default)
    {
        return new CustomVisitor(context, this, cancellationToken);
    }

    public AsyncEvaluationVisitor CreateAsyncEvaluationVisitor(ExpressionContext context, CancellationToken cancellationToken = default)
    {
        return new CustomAsyncVisitor(context, this, cancellationToken);
    }
}
```

## Custom Expression

If you also need to customize the `Expression` type itself, inherit from it and override the visitor creation methods.
This is useful when a custom expression type must inject additional state or change how visitors are built.

```csharp
internal class CustomExpression(
    string expression,
    ExpressionContext context,
    ILogicalExpressionFactory logicalExpressionFactory,
    ILogicalExpressionCache cache) : Expression(expression, context, logicalExpressionFactory, cache)
{
    public CustomExpression(
        LogicalExpression logicalExpression,
        ExpressionContext context,
        ILogicalExpressionFactory logicalExpressionFactory,
        ILogicalExpressionCache cache)
        : this(string.Empty, context, logicalExpressionFactory, cache)
    {
        LogicalExpression = logicalExpression;
    }

    protected override EvaluationVisitor CreateEvaluationVisitor(CancellationToken cancellationToken = default)
    {
        return new CustomVisitor(Context, EvaluationVisitorFactory, cancellationToken);
    }

    protected override AsyncEvaluationVisitor CreateAsyncEvaluationVisitor(CancellationToken cancellationToken = default)
    {
        return new CustomAsyncVisitor(Context, EvaluationVisitorFactory, cancellationToken);
    }
}
```

## Using `WithEvaluationVisitorFactory`

Register the factory with dependency injection when you want NCalc to resolve your custom visitors automatically.

```csharp
services.AddNCalc()
        .WithEvaluationVisitorFactory<CustomEvaluationVisitorFactory>();
```

The factory is the wiring point, while the visitors contain the specialized behavior. If you only need to customize a
specific visitor, you can override that visitor directly and keep the factory as the place where it is connected.
