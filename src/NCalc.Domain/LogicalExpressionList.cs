using NCalc.Visitors;

namespace NCalc;

public sealed class LogicalExpressionList : LogicalExpression, IReadOnlyList<LogicalExpression>
{
    private readonly LogicalExpression[] _logicalExpressions;

    public LogicalExpressionList()
    {
        _logicalExpressions = [];
    }

    public LogicalExpressionList(IReadOnlyList<LogicalExpression> values)
    {
        _logicalExpressions = values.ToArray();
    }

    public int Count => _logicalExpressions.Length;

    public Span<LogicalExpression> AsSpan() => _logicalExpressions.AsSpan();

    public LogicalExpression this[int index] => _logicalExpressions[index];

    public IEnumerator<LogicalExpression> GetEnumerator()
    {
        return ((IEnumerable<LogicalExpression>)_logicalExpressions).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public override T Accept<T>(ILogicalExpressionVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
};