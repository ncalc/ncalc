using NCalc.Visitors;

namespace NCalc.Extensions;

public static class LogicalExpressionExtensions
{
    extension(LogicalExpression expression)
    {
        public Task<object?> EvaluateAsync(ExpressionContext context, CancellationToken cancellationToken = default)
        {
            return expression.Accept(new AsyncEvaluationVisitor(context, cancellationToken));
        }

        public object? Evaluate(ExpressionContext context, CancellationToken cancellationToken = default)
        {
            return expression.Accept(new EvaluationVisitor(context, cancellationToken));
        }

        public string ToExpressionString()
        {
            return expression.Accept(new SerializationVisitor()).TrimEnd();
        }
    }
}
