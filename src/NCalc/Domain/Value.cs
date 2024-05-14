using System;
using NCalc.Exceptions;
using NCalc.Visitors;

namespace NCalc.Domain;

public sealed class ValueExpression : LogicalExpression
{
    public object Value { get; }
    public ValueType Type { get; }
    
    public ValueExpression(object value)
    {
        Type = value switch
        {
            bool => ValueType.Boolean,
            DateTime => ValueType.DateTime,
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
        Value = (int)value;
        Type = ValueType.Integer;
    }

    public ValueExpression(double value)
    {
        Value = value;
        Type = ValueType.Float;
    }

    public ValueExpression(DateTime value)
    {
        Value = value;
        Type = ValueType.DateTime;
    }

    public ValueExpression(bool value)
    {
        Value = value;
        Type = ValueType.Boolean;
    }

    public override void Accept(LogicalExpressionVisitor visitor)
    {
        visitor.Visit(this);
    }
}

public enum ValueType
{
    Integer,
    String,
    DateTime,
    Float,
    Boolean
}