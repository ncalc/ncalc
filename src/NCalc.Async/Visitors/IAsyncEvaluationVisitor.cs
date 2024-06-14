namespace NCalc.Visitors;

public interface IAsyncEvaluationVisitor : IAsyncLogicalExpressionVisitor
{
    AsyncExpressionContext Context { get; set; }
    public object? Result { get;  }
}