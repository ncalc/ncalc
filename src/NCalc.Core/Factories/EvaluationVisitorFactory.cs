using NCalc.Visitors;

namespace NCalc.Factories;

public sealed class EvaluationVisitorFactory : IEvaluationVisitorFactory
{
    public EvaluationVisitor CreateEvaluationVisitor(
        ExpressionContext context,
        CancellationToken cancellationToken = default)
    {
        return new EvaluationVisitor(context, cancellationToken, this);
    }

    public AsyncEvaluationVisitor CreateAsyncEvaluationVisitor(
        ExpressionContext context,
        CancellationToken cancellationToken = default)
    {
        return new AsyncEvaluationVisitor(context, cancellationToken, this);
    }
}
