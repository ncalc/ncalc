# Overriding Evaluation Behavior

NCalc provides a flexible way to evaluate expressions, and in some cases, you may need to customize its evaluation
behavior. This can be done by creating a custom `Expression` and overriding `CreateEvaluationVisitor` or
`CreateAsyncEvaluationVisitor`.

## Custom Evaluation Visitor

By creating a custom evaluation visitor, you can modify how specific expressions are interpreted. The following example
overrides the behavior of value expressions, returning a special string when encountering the number `42`:

```csharp
private class CustomVisitor(ExpressionContext context, CancellationToken cancellationToken) : EvaluationVisitor(context, cancellationToken)
{
    public override object? Visit(ValueExpression expression)
    {
        // My custom behavior.
        if(expression.Value is 42)
            return "The answer";

        // Use the normal behavior.
        return base.Visit(expression);
    }
}

private class CustomAsyncVisitor(ExpressionContext context, CancellationToken cancellationToken) : AsyncEvaluationVisitor(context, cancellationToken)
{
    public override Task<object?> Visit(ValueExpression expression)
    {
        // My custom behavior.
        if(expression.Value is 42)
            return Task.FromResult<object?>("The answer async");

        // Use the normal behavior.
        return base.Visit(expression, cancellationToken);
    }
}
```

## Custom Expression

To integrate the custom visitor with NCalc, inherit from `Expression` and override the visitor creation method:

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

    protected override EvaluationVisitor CreateEvaluationVisitor()
    {
        return new CustomVisitor(Context);
    }

    protected override AsyncEvaluationVisitor CreateAsyncEvaluationVisitor()
    {
        return new CustomAsyncVisitor(Context);
    }
}
```

## Using `IExpressionFactory`

Use a custom implementation of <xref:NCalc.Factories.IExpressionFactory> to return your custom expression type:

```csharp
internal class CustomExpressionFactory(
    ILogicalExpressionFactory logicalExpressionFactory,
    ILogicalExpressionCache cache) : IExpressionFactory
{
    public Expression Create(string expression, ExpressionContext? expressionContext = null)
    {
        return new CustomExpression(expression, expressionContext ?? new(), logicalExpressionFactory, cache);
    }

    public Expression Create(LogicalExpression logicalExpression, ExpressionContext? expressionContext = null)
    {
        return new CustomExpression(logicalExpression, expressionContext ?? new(), logicalExpressionFactory, cache);
    }
}
```

Register the factory with dependency injection:

```csharp
services.AddNCalc()
        .WithExpressionFactory<CustomExpressionFactory>();
```
