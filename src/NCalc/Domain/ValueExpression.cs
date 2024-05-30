using NCalc.Exceptions;
using NCalc.Visitors;

namespace NCalc.Domain;

public sealed class ValueExpression : LogicalExpression
{
    public object? Value { get; set; }
    public ValueType Type { get; set; }


    public ValueExpression()
    {
    }

    public ValueExpression(object value)
    {
        Type = value switch
        {
            bool => ValueType.Boolean,
            DateTime => ValueType.DateTime,
            TimeSpan => ValueType.TimeSpan,
            decimal or double or float => ValueType.Float,
            byte or sbyte or short or int or long or ushort or uint or ulong => ValueType.Integer,
            string => ValueType.String,
            _ => throw new NCalcEvaluationException("This value could not be handled: " + value)
        };

        Value = value;
    }

    public ValueExpression(string value)
    {
        Value = value;
        Type = ValueType.String;
    }

    public ValueExpression(int value)
    {
        Value = value;
        Type = ValueType.Integer;
    }

    public ValueExpression(long value)
    {
        if (value is > int.MaxValue or < int.MinValue)
        {
            Value = value;
            Type = ValueType.Integer;
        }
        else
        {
            Value = (int)value;
            Type = ValueType.Integer;
        }
    }

    public ValueExpression(double value)
    {
        Value = value;
        Type = ValueType.Float;
    }

    public ValueExpression(decimal value)
    {
        Value = value;
        Type = ValueType.Float;
    }

    public ValueExpression(DateTime value)
    {
        Value = value;
        Type = ValueType.DateTime;
    }


    public ValueExpression(TimeSpan value)
    {
        Value = value;
        Type = ValueType.TimeSpan;
    }

    public ValueExpression(bool value)
    {
        Value = value;
        Type = ValueType.Boolean;
    }

    public override void Accept(ILogicalExpressionVisitor visitor)
    {
        visitor.Visit(this);
    }
}