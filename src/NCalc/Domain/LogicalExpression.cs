using NCalc.Visitors;

namespace NCalc.Domain;

public abstract class LogicalExpression
{
    public override string ToString()
    {
        var serializer = new SerializationVisitor();
        Accept(serializer);

        return serializer.Result.ToString().TrimEnd(' ');
    }

    public virtual void Accept(LogicalExpressionVisitor visitor)
    {
        visitor.Visit(this);
    }
}