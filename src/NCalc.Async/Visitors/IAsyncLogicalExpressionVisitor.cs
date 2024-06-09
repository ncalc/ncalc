
using NCalc.Domain;

namespace NCalc.Visitors;

public interface IAsyncLogicalExpressionVisitor
{
    public Task VisitAsync(TernaryExpression expression);
    public Task VisitAsync(BinaryExpression expression);
    public Task VisitAsync(UnaryExpression expression);
    public Task VisitAsync(ValueExpression expression);
    public Task VisitAsync(Function function);
    public Task VisitAsync(Identifier identifier);
}