using NCalc.Visitors;

namespace NCalc.Factories;

public interface IAsyncEvaluationVisitorFactory
{
    public AsyncEvaluationVisitor Create(AsyncExpressionContext context);
}