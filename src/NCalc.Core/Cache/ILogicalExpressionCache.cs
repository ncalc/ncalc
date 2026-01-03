using NCalc.Domain;
using NCalc.Parser;

namespace NCalc.Cache;

public record class LogicalExpressionCacheKey(string Expression, ExpressionOptions Options, string CultureInfoName, ArgumentSeparator ArgumentSeparator);

public interface ILogicalExpressionCache
{
    public bool TryGetValue(LogicalExpressionCacheKey key, out LogicalExpression? logicalExpression);
    public void Set(LogicalExpressionCacheKey key, LogicalExpression logicalExpression);
}