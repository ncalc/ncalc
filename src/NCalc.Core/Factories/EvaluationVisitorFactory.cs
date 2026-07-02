using NCalc.Visitors;

namespace NCalc.Factories;

public sealed class EvaluationVisitorFactory : IEvaluationVisitorFactory
{
    public EvaluationVisitor CreateEvaluationVisitor(
        ExpressionContext context,
        CancellationToken cancellationToken = default)
    {
        return new EvaluationVisitor(context, this, cancellationToken);
    }

    public AsyncEvaluationVisitor CreateAsyncEvaluationVisitor(
        ExpressionContext context,
        CancellationToken cancellationToken = default)
    {
        return new AsyncEvaluationVisitor(context, this, cancellationToken);
    }
}
