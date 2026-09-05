namespace NCalc.Factories;

public interface IExpressionFactory
{
    public Expression Create(
        string expression,
        ExpressionConfiguration? configuration = null,
        ExpressionContext? context = null,
        CultureInfo? cultureInfo = null);

    public Expression Create(
        LogicalExpression logicalExpression,
        ExpressionConfiguration? configuration = null,
        ExpressionContext? context = null,
        CultureInfo? cultureInfo = null);
}
