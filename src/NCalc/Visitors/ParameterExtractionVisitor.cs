using NCalc.Domain;

namespace NCalc.Visitors;

internal sealed class ParameterExtractionVisitor : LogicalExpressionVisitor
{
    public List<string> Parameters { get; } = [];

    public override void Visit(Identifier identifier)
    {
        if (!Parameters.Contains(identifier.Name))
        {
            Parameters.Add(identifier.Name);
        }
    }

    public override void Visit(UnaryExpression expression) => expression.Accept(this);

    public override void Visit(BinaryExpression expression)
    {
        expression.LeftExpression.Accept(this);
        expression.RightExpression.Accept(this);
    }

    public override void Visit(TernaryExpression expression)
    {
        expression.LeftExpression.Accept(this);
        expression.RightExpression.Accept(this);
        expression.MiddleExpression.Accept(this);
    }

    public override void Visit(Function function)
    {
        foreach (var expression in function.Expressions)
            expression.Accept(this);
    }

    public override void Visit(LogicalExpression expression) => expression.Accept(this);

    public override void Visit(ValueExpression expression)
    {
    }
}