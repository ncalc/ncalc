using NCalc.Domain;

namespace NCalc.Visitors;

/// <summary>
/// Visitor dedicated to extract <see cref="Identifier"/> names from a <see cref="LogicalExpression"/>.
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

    public List<string> Visit(LogicalExpressionList list)
    {
        var parameters = new List<string>();
        foreach (var value in list)
        {
            if (value is Identifier identifier)
            {
                if (!parameters.Contains(identifier.Name))
                {
                    parameters.Add(identifier.Name);
                }
            }
            else
            {
                parameters.AddRange(value.Accept(this));
            }
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

        var innerParameters = function.Parameters.Accept(this);
        parameters.AddRange(innerParameters);

        return parameters.Distinct().ToList();
    }

    public List<string> Visit(ValueExpression expression) => [];
}