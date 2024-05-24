using NCalc.Domain;

namespace NCalc.Cache;

public interface ILogicalExpressionCache
{
    public bool TryGetValue(string expression, out LogicalExpression? logicalExpression);
    public void Set(string expression, LogicalExpression logicalExpression);
}