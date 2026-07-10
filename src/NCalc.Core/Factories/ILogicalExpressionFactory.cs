using NCalc.Parser;

namespace NCalc.Factories;

public interface ILogicalExpressionFactory
{
    public LogicalExpression Create(string expression, LogicalExpressionParserOptions? options = null,
        CultureInfo? cultureInfo = null, CancellationToken cancellationToken = default);
}