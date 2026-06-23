using NCalc.Visitors;

namespace NCalc;

public sealed class LogicalExpressionList : LogicalExpression, IReadOnlyList<LogicalExpression>
{
    private readonly LogicalExpression[] _list;

    public LogicalExpressionList()
    {
        _list = [];
    }

    public LogicalExpressionList(IReadOnlyList<LogicalExpression> values)
    {
        _list = values.ToArray();
    }

    public int Count => _list.Length;

    public Span<LogicalExpression> AsSpan() => _list.AsSpan();

    public LogicalExpression this[int index] => _list[index];

    public IEnumerator<LogicalExpression> GetEnumerator()
    {
        return ((IEnumerable<LogicalExpression>)_list).GetEnumerator();
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