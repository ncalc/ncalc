using NCalc.Domain;

namespace NCalc.Factories;

public interface ILogicalExpressionFactory
{
    public LogicalExpression Create(string expression, ExpressionOptions options = ExpressionOptions.None, CancellationToken ct = default);

    public LogicalExpression Create(string expression, CultureInfo cultureInfo, ExpressionOptions options = ExpressionOptions.None, CancellationToken ct = default);
}