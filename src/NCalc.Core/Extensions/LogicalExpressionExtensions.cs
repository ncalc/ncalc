using NCalc.Visitors;

namespace NCalc.Extensions;

public static class LogicalExpressionExtensions
{
    extension(LogicalExpression expression)
    {
        public string ToExpressionString(CancellationToken ct = default)
        {
#if NET
            ArgumentNullException.ThrowIfNull(expression);
#endif
            return expression.Accept(new SerializationVisitor(), ct).TrimEnd();
        }
    }
}
