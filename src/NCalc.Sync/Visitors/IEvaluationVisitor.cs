using NCalc.Handlers;

namespace NCalc.Visitors;

public interface IEvaluationVisitor : ILogicalExpressionVisitor
{
    ExpressionContext Context { get; set; }

    public object? Result { get; }
}