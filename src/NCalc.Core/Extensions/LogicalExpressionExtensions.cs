using NCalc.Visitors;

namespace NCalc.Extensions;

public static class LogicalExpressionExtensions
{
    extension(LogicalExpression expression)
    {
        public ValueTask<object?> EvaluateAsync(ExpressionContext context, CancellationToken ct = default)
        {
#if NET
            ArgumentNullException.ThrowIfNull(expression);
#endif
            return expression.Accept(new EvaluationVisitor(context), ct);
        }

        public object? Evaluate(ExpressionContext context, CancellationToken ct = default)
        {
            var valueTask = expression.EvaluateAsync(context, ct);

            if (valueTask.IsCompletedSuccessfully)
                return valueTask.Result;

            return valueTask.AsTask().GetAwaiter().GetResult();
        }

        public string ToExpressionString(CancellationToken ct = default)
        {
#if NET
            ArgumentNullException.ThrowIfNull(expression);
#endif
            return expression.Accept(new SerializationVisitor(), ct).TrimEnd();
        }
    }
}
