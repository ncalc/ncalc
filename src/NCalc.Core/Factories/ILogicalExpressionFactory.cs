using NCalc.Domain;

namespace NCalc.Factories;

public interface ILogicalExpressionFactory
{
    public LogicalExpression Create(string expression, ExpressionOptions options = ExpressionOptions.None);

    public LogicalExpression Create(string expression, CultureInfo cultureInfo, ExpressionOptions options = ExpressionOptions.None, AdvancedExpressionOptions? extendedOptions = null);
}