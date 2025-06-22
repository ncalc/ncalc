using System.Numerics;
using NCalc.Exceptions;
using NCalc.Visitors;

namespace NCalc.Domain;

public class Percent
{
    public object? Value { get; set; }
    public ValueType Type { get; set; }

        public Type OriginalType { get; }

    public Percent()
    {
            Type = value switch
            {
                decimal or double or float => ValueType.Float,
                byte or sbyte or short or int or long or ushort or uint or ulong => ValueType.Integer,
                BigInteger => ValueType.Integer,
                _ => throw new NCalcException("This value could not be handled: " + value)
            };

            OriginalType = value.GetType();
            Value = value;
    }

    public Percent(object value)
    {
            OriginalType = originalType;
        Type = value switch
        {
            decimal or double or float => ValueType.Float,
            byte or sbyte or short or int or long or ushort or uint or ulong => ValueType.Integer,
                BigInteger => ValueType.Integer,
            _ => throw new NCalcException("This value could not be handled: " + value)
        };

        Value = value;
        }

        public override string ToString()
        {
            if (Value == null)
                return "null";
            return Value + "%";
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
