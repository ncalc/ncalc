namespace NCalc.Visitors;

/// <summary>
/// Visitor dedicated to extract <see cref="Identifier"/> names from a <see cref="LogicalExpression"/>.
/// </summary>
public sealed class ParameterExtractionVisitor : ILogicalExpressionVisitor<List<string>>
{
    public List<string> Visit(Identifier identifier, CancellationToken cancellationToken = default) => [identifier.Name];

    public List<string> Visit(LogicalExpressionList list, CancellationToken cancellationToken = default)
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
                parameters.AddRange(value.Accept(this, cancellationToken));
            }
        }
        return parameters;
    }

    public List<string> Visit(UnaryExpression expression, CancellationToken cancellationToken = default) =>
        expression.Expression.Accept(this, cancellationToken);

    public List<string> Visit(BinaryExpression expression, CancellationToken cancellationToken = default)
    {
        var leftParameters = expression.LeftExpression.Accept(this, cancellationToken);
        var rightParameters = expression.RightExpression.Accept(this, cancellationToken);

        leftParameters.AddRange(rightParameters);
        return leftParameters.Distinct().ToList();
    }

    public List<string> Visit(TernaryExpression expression, CancellationToken cancellationToken = default)
    {
        var leftParameters = expression.LeftExpression.Accept(this, cancellationToken);
        var middleParameters = expression.MiddleExpression.Accept(this, cancellationToken);
        var rightParameters = expression.RightExpression.Accept(this, cancellationToken);

        leftParameters.AddRange(middleParameters);
        leftParameters.AddRange(rightParameters);
        return leftParameters.Distinct().ToList();
    }

    public List<string> Visit(Function function, CancellationToken cancellationToken = default)
    {
        var parameters = new List<string>();

        var innerParameters = function.Parameters.Accept(this, cancellationToken);
        parameters.AddRange(innerParameters);

        return parameters.Distinct().ToList();
    }

    public List<string> Visit(ValueExpression expression, CancellationToken cancellationToken = default) => [];
}