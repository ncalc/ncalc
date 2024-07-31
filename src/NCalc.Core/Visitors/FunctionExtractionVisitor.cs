using NCalc.Domain;

namespace NCalc.Visitors;

/// <summary>
/// Visitor dedicated to extract <see cref="Function"/> names from a <see cref="LogicalExpression"/>.
/// </summary>
public sealed class FunctionExtractionVisitor : ILogicalExpressionVisitor<List<string>>
{
    public List<string> Visit(Identifier identifier) => [];

    public List<string> Visit(LogicalExpressionList list)
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
            }
            else
            {
                functions.AddRange(value.Accept(this));
            }
        }
        return functions;
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
        var functions = new List<string> { function.Identifier.Name };

        var innerFunctions = function.Parameters.Accept(this);
        functions.AddRange(innerFunctions);

        return functions.Distinct().ToList();
    }

    public List<string> Visit(ValueExpression expression) => [];
}