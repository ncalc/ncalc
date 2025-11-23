using NCalc.Domain;

namespace NCalc.Visitors;

/// <summary>
/// Visitor dedicated to extract <see cref="Identifier"/> names from a <see cref="LogicalExpression"/>.
/// </summary>
public sealed class ParameterExtractionVisitor : ILogicalExpressionVisitor<List<string>>
{
    public List<string> Visit(Identifier identifier, CancellationToken ct = default) => [identifier.Name];

    public List<string> Visit(LogicalExpressionList list, CancellationToken ct = default)
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
                parameters.AddRange(value.Accept(this, ct));
            }
        }
        return parameters;
    }

    public List<string> Visit(UnaryExpression expression, CancellationToken ct = default) =>
        expression.Expression.Accept(this, ct);

    public List<string> Visit(BinaryExpression expression, CancellationToken ct = default)
    {
        var leftParameters = expression.LeftExpression.Accept(this, ct);
        var rightParameters = expression.RightExpression.Accept(this, ct);

        leftParameters.AddRange(rightParameters);
        return leftParameters.Distinct().ToList();
    }

    public List<string> Visit(TernaryExpression expression, CancellationToken ct = default)
    {
        var leftParameters = expression.LeftExpression.Accept(this, ct);
        var middleParameters = expression.MiddleExpression.Accept(this, ct);
        var rightParameters = expression.RightExpression.Accept(this, ct);

        leftParameters.AddRange(middleParameters);
        leftParameters.AddRange(rightParameters);
        return leftParameters.Distinct().ToList();
    }

    public List<string> Visit(Function function, CancellationToken ct = default)
    {
        var parameters = new List<string>();

        var innerParameters = function.Parameters.Accept(this, ct);
        parameters.AddRange(innerParameters);

        return parameters.Distinct().ToList();
    }

    public List<string> Visit(ValueExpression expression, CancellationToken ct = default) => [];
}