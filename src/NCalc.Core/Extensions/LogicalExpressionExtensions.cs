using NCalc.Visitors;

namespace NCalc.Extensions;

public static class LogicalExpressionExtensions
{
    extension(LogicalExpression expression)
    {
        public ValueTask<object?> EvaluateAsync(ExpressionContext context, CancellationToken cancellationToken = default)
        {
#if NET
            ArgumentNullException.ThrowIfNull(expression);
#endif
            return expression.Accept(new EvaluationVisitor(context), cancellationToken);
        }

        public object? Evaluate(ExpressionContext context, CancellationToken cancellationToken = default)
        {
            var valueTask = expression.EvaluateAsync(context, cancellationToken);

            if (valueTask.IsCompletedSuccessfully)
                return valueTask.Result;

            return valueTask.AsTask().GetAwaiter().GetResult();
        }

        public string ToExpressionString(CancellationToken cancellationToken = default)
        {
#if NET
            ArgumentNullException.ThrowIfNull(expression);
#endif
            return expression.Accept(new SerializationVisitor(), cancellationToken).TrimEnd();
        }
    }
}
