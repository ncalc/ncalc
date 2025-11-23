using NCalc.Domain;

namespace NCalc.Visitors;

/// <summary>
/// Visitor dedicated to extract <see cref="Function"/> names from a <see cref="LogicalExpression"/>.
/// </summary>
public sealed class FunctionExtractionVisitor : ILogicalExpressionVisitor<List<string>>
{
    public List<string> Visit(Identifier identifier, CancellationToken ct = default) => [];

    public List<string> Visit(LogicalExpressionList list, CancellationToken ct = default)
    {
        var functions = new List<string>();
        foreach (var value in list)
        {
            if (value is Function function)
            {
                if (!functions.Contains(function.Identifier.Name))
                {
                    functions.Add(function.Identifier.Name);
                }

                foreach (var parameter in function.Parameters)
                {
                    if (parameter is not null)
                    {
                        functions.AddRange(parameter.Accept(this, ct));
                    }
                }
            }
            else
            {
                functions.AddRange(value.Accept(this, ct));
            }
        }
        return functions;
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
        var functions = new List<string> { function.Identifier.Name };

        var innerFunctions = function.Parameters.Accept(this, ct);
        functions.AddRange(innerFunctions);

        return functions.Distinct().ToList();
    }

    public List<string> Visit(ValueExpression expression, CancellationToken ct = default) => [];
}