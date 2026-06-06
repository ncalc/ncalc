namespace NCalc.Factories;

public interface ILogicalExpressionFactory
{
    public LogicalExpression Create(string expression, ExpressionOptions options = ExpressionOptions.None, CancellationToken cancellationToken = default);

    public LogicalExpression Create(string expression, CultureInfo cultureInfo, ExpressionOptions options = ExpressionOptions.None, CancellationToken cancellationToken = default);
}