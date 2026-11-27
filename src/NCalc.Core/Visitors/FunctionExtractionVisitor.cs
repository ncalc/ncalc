using System.Runtime.CompilerServices;

namespace NCalc.Visitors;

/// <summary>
/// Visitor dedicated to extract <see cref="Function"/> names from a <see cref="LogicalExpression"/>.
/// </summary>
/// <remarks>
/// Names are returned in order of first appearance, without duplicates. The tree is walked once into a single
/// result list; merging and de-duplicating a list at every node made the cost grow with the depth of the tree
/// times its width.
/// </remarks>
public sealed class FunctionExtractionVisitor : ILogicalExpressionVisitor<List<string>>
{
    public List<string> Visit(Identifier identifier) => [];

    public List<string> Visit(LogicalExpressionList list) => Collect(list);

    public List<string> Visit(UnaryExpression expression) => Collect(expression);

    public List<string> Visit(BinaryExpression expression) => Collect(expression);

    public List<string> Visit(TernaryExpression expression) => Collect(expression);

    public List<string> Visit(Function function) => Collect(function);

    public List<string> Visit(ValueExpression expression) => [];

    private static List<string> Collect(LogicalExpression root)
    {
        var names = new List<string>();
        Collect(root, names);
        return names;
    }

    private static void Collect(LogicalExpression expression, List<string> names)
    {
        RuntimeHelpers.EnsureSufficientExecutionStack();

        switch (expression)
        {
            case UnaryExpression unary:
                Collect(unary.Expression, names);
                break;

            case BinaryExpression binary:
                Collect(binary.LeftExpression, names);
                Collect(binary.RightExpression, names);
                break;

            case TernaryExpression ternary:
                Collect(ternary.LeftExpression, names);
                Collect(ternary.MiddleExpression, names);
                Collect(ternary.RightExpression, names);
                break;

            case Function function:
                if (!names.Contains(function.Identifier.Name))
                    names.Add(function.Identifier.Name);
                Collect(function.Parameters, names);
                break;

            case LogicalExpressionList list:
                foreach (var item in list)
                    Collect(item, names);
                break;
        }
    }
}
