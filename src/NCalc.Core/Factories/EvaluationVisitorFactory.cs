using NCalc.Visitors;

namespace NCalc.Factories;

public sealed class EvaluationVisitorFactory : IEvaluationVisitorFactory
{
    public EvaluationVisitor CreateEvaluationVisitor(
        ExpressionContext context,
        ExpressionEvaluationOptions options,
        CultureInfo cultureInfo,
        CancellationToken cancellationToken = default)
    {
        return new EvaluationVisitor(context, options, cultureInfo, this, cancellationToken);
    }

    public AsyncEvaluationVisitor CreateAsyncEvaluationVisitor(
        ExpressionContext context,
        ExpressionEvaluationOptions options,
        CultureInfo cultureInfo,
        CancellationToken cancellationToken = default)
    {
        return new AsyncEvaluationVisitor(context, options, cultureInfo, this, cancellationToken);
    }
}
