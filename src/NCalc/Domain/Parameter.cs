namespace NCalc.Domain;

public class Identifier(string name) : LogicalExpression
{
    public string Name { get; set; } = name;


    public override void Accept(LogicalExpressionVisitor visitor)
    {
        visitor.Visit(this);
    }
}