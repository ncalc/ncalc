using NCalc.Domain;

namespace NCalc.Visitors;

/// <summary>
/// Visitor dedicated to extract parameters from an <see cref="LogicalExpression"/>.
/// </summary>
public sealed class ParameterExtractionVisitor : ILogicalExpressionVisitor<List<string>>
{
    public List<string> Visit(Identifier identifier)
    {
        var parameters = new List<string>();
        if (!parameters.Contains(identifier.Name))
        {
            parameters.Add(identifier.Name);
        }
        return parameters;
    }

    public List<string> Visit(UnaryExpression expression) => expression.Expression.Accept(this);

    public List<string> Visit(BinaryExpression expression)
    {
        var leftParameters = expression.LeftExpression.Accept(this);
        var rightParameters = expression.RightExpression.Accept(this);

        leftParameters.AddRange(rightParameters);
        return leftParameters.Distinct().ToList();
    }

    public List<string> Visit(TernaryExpression expression)
    {
        var leftParameters = expression.LeftExpression.Accept(this);
        var middleParameters = expression.MiddleExpression.Accept(this);
        var rightParameters = expression.RightExpression.Accept(this);

        leftParameters.AddRange(middleParameters);
        leftParameters.AddRange(rightParameters);
        return leftParameters.Distinct().ToList();
    }

    public List<string> Visit(Function function)
    {
        var parameters = new List<string>();
        foreach (var expression in function.Expressions)
        {
            var exprParameters = expression.Accept(this);
            parameters.AddRange(exprParameters);
        }
        return parameters.Distinct().ToList();
    }

    public List<string> Visit(ValueExpression expression) => [];
}