using NCalc.Visitors;

namespace NCalc.Domain;

public sealed class Identifier(string name) : LogicalExpression
{
    public string Name { get; } = name;
    
    public override void Accept(LogicalExpressionVisitor visitor)
    {
        visitor.Visit(this);
    }
}