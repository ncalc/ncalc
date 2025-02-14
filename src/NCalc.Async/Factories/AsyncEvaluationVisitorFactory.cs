using NCalc.Visitors;

namespace NCalc.Factories;

public sealed class AsyncEvaluationVisitorFactory : IAsyncEvaluationVisitorFactory
{
    public AsyncEvaluationVisitor Create(AsyncExpressionContext context)
    {
        return new AsyncEvaluationVisitor(context);
    }
}