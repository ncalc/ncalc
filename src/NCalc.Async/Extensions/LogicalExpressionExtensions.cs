using NCalc.Domain;
using NCalc.Visitors;

namespace NCalc.Extensions;

public static class LogicalExpressionExtensions
{
    public static Task AcceptAsync<T>(this T logicalExpression, IAsyncLogicalExpressionVisitor visitor) where T : LogicalExpression
    {
        return logicalExpression switch
        {
            BinaryExpression binaryExpression => visitor.VisitAsync(binaryExpression),
            Function function => visitor.VisitAsync(function),
            Identifier identifier => visitor.VisitAsync(identifier),
            TernaryExpression ternaryExpression => visitor.VisitAsync(ternaryExpression),
            UnaryExpression unaryExpression => visitor.VisitAsync(unaryExpression),
            ValueExpression valueExpression => visitor.VisitAsync(valueExpression),
            _ => throw new ArgumentOutOfRangeException(nameof(logicalExpression))
        };
    }
}