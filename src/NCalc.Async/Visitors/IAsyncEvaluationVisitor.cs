using NCalc.Handlers;

namespace NCalc.Visitors;

public interface IAsyncEvaluationVisitor : IAsyncLogicalExpressionVisitor
{
    ExpressionContext Context { get; set; }

    public event AsyncEvaluateFunctionHandler? EvaluateFunctionAsync;
    public event AsyncEvaluateParameterHandler? EvaluateParameterAsync;

    public object? Result { get;  }
}