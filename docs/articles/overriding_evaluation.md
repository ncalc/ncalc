# Overriding Evaluation Behavior

NCalc provides a flexible way to evaluate expressions, and in some cases, you may need to customize its evaluation
behavior. This can be done by overriding the default evaluation visitor using `IEvaluationVisitorFactory`.

## Custom Evaluation Visitor

By creating a custom evaluation visitor, you can modify how specific expressions are interpreted. The following example
overrides the behavior of value expressions, returning a special string when encountering the number `42`:

```csharp
private class CustomVisitor(ExpressionContext context) : EvaluationVisitor(context)
{
    public override object Visit(ValueExpression expression)
    {
        // My custom behavior.
        if(expression.Value is 42)
            return "The answer";

        // Use the normal behavior.
        return base.Visit(expression);
    }
}
```

## Custom Evaluation Visitor Factory

To integrate the custom visitor with NCalc, implement the `IEvaluationVisitorFactory` interface:

```csharp
private class CustomEvaluationVisitorFactory : IEvaluationVisitorFactory
{
    public EvaluationVisitor Create(ExpressionContext context)
    {
        return new CustomVisitor(context);
    }
}
```

## Using `WithEvaluationVisitorFactory`

Use the `WithEvaluationVisitorFactory` method to specify the custom implementation
of <xref:NCalc.Factories.IEvaluationVisitorFactory>. This ensures that the created evaluation visitor is used when
evaluating expressions.

### Example:

```csharp
services.AddNCalc()
        .WithEvaluationVisitorFactory<CustomEvaluationVisitorFactory>();
```

By overriding the evaluation visitor, you gain complete control over how expressions are interpreted and processed,
enabling customization to fit specific application needs.

