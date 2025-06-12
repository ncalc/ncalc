using NCalc.Exceptions;
using NCalc.Visitors;

namespace NCalc.Domain
{
    public class Percent
    {
        public object? Value { get; set; }
        public ValueType Type { get; set; }

        public Percent()
        {
        }

        public Percent(object value)
        {
            Type = value switch
            {
                decimal or double or float => ValueType.Float,
                byte or sbyte or short or int or long or ushort or uint or ulong => ValueType.Integer,
                _ => throw new NCalcException("This value could not be handled: " + value)
            };

            Value = value;
        }
    }

    public sealed class PercentExpression : LogicalExpression
    {
        public LogicalExpression Expression { get; set; }

        public PercentExpression(LogicalExpression expression)
        {
            Expression = expression;
        }

        public override T Accept<T>(ILogicalExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
