using NCalc.Visitors;

namespace NCalc.Factories;

internal sealed class AsyncEvaluationVisitorFactory : IAsyncEvaluationVisitorFactory
{
    public AsyncEvaluationVisitor Create(AsyncExpressionContext context)
    {
        return new AsyncEvaluationVisitor(context);
    }
}