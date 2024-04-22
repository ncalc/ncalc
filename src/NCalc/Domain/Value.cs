using System;

namespace NCalc.Domain;

public class ValueExpression : LogicalExpression
{
    public ValueExpression() {}
        
    public ValueExpression(object value, ValueType type)
    {
        Value = value;
        Type = type;
    }

    public ValueExpression(object value)
    {
        Type = value switch
        {
            bool => ValueType.Boolean,
            DateTime => ValueType.DateTime,
            decimal or double or float => ValueType.Float,
            byte or sbyte or short or int or long or ushort or uint or ulong => ValueType.Integer,
            string => ValueType.String,
            _ => throw new EvaluationException("This value could not be handled: " + value)
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
        Value = value;
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

    public object Value { get; set; }
    public ValueType Type { get; set; }

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