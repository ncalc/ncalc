using NCalc.Visitors;

namespace NCalc.Factories;

public interface IEvaluationVisitorFactory
{
    EvaluationVisitor CreateEvaluationVisitor(ExpressionContext context, ExpressionEvaluationOptions options, CultureInfo cultureInfo, CancellationToken cancellationToken = default);
    AsyncEvaluationVisitor CreateAsyncEvaluationVisitor(ExpressionContext context, ExpressionEvaluationOptions options, CultureInfo cultureInfo, CancellationToken cancellationToken = default);
}
