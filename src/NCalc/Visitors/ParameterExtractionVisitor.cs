using NCalc.Domain;

namespace NCalc.Visitors;

internal sealed class ParameterExtractionVisitor : ILogicalExpressionVisitor
{
    public List<string> Parameters { get; } = [];

    public void Visit(Identifier identifier)
    {
        if (!Parameters.Contains(identifier.Name))
        {
            Parameters.Add(identifier.Name);
        }
    }

    public void Visit(UnaryExpression expression) => expression.Accept(this);

    public void Visit(BinaryExpression expression)
    {
        expression.LeftExpression.Accept(this);
        expression.RightExpression.Accept(this);
    }

    public void Visit(TernaryExpression expression)
    {
        expression.LeftExpression.Accept(this);
        expression.RightExpression.Accept(this);
        expression.MiddleExpression.Accept(this);
    }

    public void Visit(Function function)
    {
        foreach (var expression in function.Expressions)
            expression.Accept(this);
    }

    public void Visit(LogicalExpression expression) => expression.Accept(this);

    public void Visit(ValueExpression expression)
    {
    }
}