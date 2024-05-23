using NCalc.Domain;

namespace NCalc.Cache;

public interface ILogicalExpressionCache
{
    public bool Enable { get; set; }
    public bool TryGetValue(string expression, out LogicalExpression? logicalExpression);
    public bool Set(string expression, LogicalExpression logicalExpression);
}