namespace NCalc.Visitors;

/// <summary>
/// Visitor dedicated to extract <see cref="Function"/> names from a <see cref="LogicalExpression"/>.
/// </summary>
public sealed class FunctionExtractionVisitor : ILogicalExpressionVisitor<List<string>>
{
    public List<string> Visit(Identifier identifier, CancellationToken cancellationToken = default) => [];

    public List<string> Visit(LogicalExpressionList list, CancellationToken cancellationToken = default)
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
                        functions.AddRange(parameter.Accept(this, cancellationToken));
                    }
                }
            }
            else
            {
                functions.AddRange(value.Accept(this, cancellationToken));
            }
        }
        return functions;
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
        var functions = new List<string> { function.Identifier.Name };

        var innerFunctions = function.Parameters.Accept(this, cancellationToken);
        functions.AddRange(innerFunctions);

        return functions.Distinct().ToList();
    }

    public List<string> Visit(ValueExpression expression, CancellationToken cancellationToken = default) => [];
}