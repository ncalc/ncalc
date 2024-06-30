using NCalc.Domain;
using ValueType = NCalc.Domain.ValueType;

namespace NCalc.Visitors;

public class SerializationVisitor : ILogicalExpressionVisitor<string>
{
    private readonly NumberFormatInfo _numberFormatInfo = new()
    {
        NumberDecimalSeparator = "."
    };

    public string Visit(TernaryExpression expression)
    {
        var result = new StringBuilder();
        result.Append(EncapsulateNoValue(expression.LeftExpression));
        result.Append("? ");
        result.Append(EncapsulateNoValue(expression.MiddleExpression));
        result.Append(": ");
        result.Append(EncapsulateNoValue(expression.RightExpression));
        return result.ToString().Trim();
    }

    public string Visit(BinaryExpression expression)
    {
        var result = new StringBuilder();
        result.Append(EncapsulateNoValue(expression.LeftExpression));

        switch (expression.Type)
        {
            case BinaryExpressionType.And:
                result.Append("and ");
                break;
            case BinaryExpressionType.Or:
                result.Append("or ");
                break;
            case BinaryExpressionType.Div:
                result.Append("/ ");
                break;
            case BinaryExpressionType.Equal:
                result.Append("= ");
                break;
            case BinaryExpressionType.Greater:
                result.Append("> ");
                break;
            case BinaryExpressionType.GreaterOrEqual:
                result.Append(">= ");
                break;
            case BinaryExpressionType.Lesser:
                result.Append("< ");
                break;
            case BinaryExpressionType.LesserOrEqual:
                result.Append("<= ");
                break;
            case BinaryExpressionType.Minus:
                result.Append("- ");
                break;
            case BinaryExpressionType.Modulo:
                result.Append("% ");
                break;
            case BinaryExpressionType.NotEqual:
                result.Append("!= ");
                break;
            case BinaryExpressionType.Plus:
                result.Append("+ ");
                break;
            case BinaryExpressionType.Times:
                result.Append("* ");
                break;
            case BinaryExpressionType.BitwiseAnd:
                result.Append("& ");
                break;
            case BinaryExpressionType.BitwiseOr:
                result.Append("| ");
                break;
            case BinaryExpressionType.BitwiseXOr:
                result.Append("^ ");
                break;
            case BinaryExpressionType.LeftShift:
                result.Append("<< ");
                break;
            case BinaryExpressionType.RightShift:
                result.Append(">> ");
                break;
        }

        result.Append(EncapsulateNoValue(expression.RightExpression));
        return result.ToString().Trim();
    }

    public string Visit(UnaryExpression expression)
    {
        var result = new StringBuilder();

        switch (expression.Type)
        {
            case UnaryExpressionType.Not:
                result.Append('!');
                break;
            case UnaryExpressionType.Negate:
                result.Append('-');
                break;
            case UnaryExpressionType.BitwiseNot:
                result.Append('~');
                break;
        }

        result.Append(EncapsulateNoValue(expression.Expression));
        return result.ToString().Trim();
    }

    public string Visit(ValueExpression expression)
    {
        var result = new StringBuilder();

        switch (expression.Type)
        {
            case ValueType.Boolean:
                result.Append(expression.Value).Append(' ');
                break;
            case ValueType.DateTime or ValueType.TimeSpan:
                result.Append('#').Append(expression.Value).Append('#').Append(' ');
                break;
            case ValueType.Float:
                result.Append(decimal.Parse(expression.Value?.ToString() ?? string.Empty).ToString(_numberFormatInfo))
                    .Append(' ');
                break;
            case ValueType.Integer:
                result.Append(expression.Value).Append(' ');
                break;
            case ValueType.String:
                result.Append('\'').Append(expression.Value).Append('\'').Append(' ');
                break;
        }

        return result.ToString().Trim();
    }

    public string Visit(Function function)
    {
        var result = new StringBuilder();
        result.Append(function.Identifier.Name).Append('(');

        for (int i = 0; i < function.Expressions.Length; i++)
        {
            result.Append(function.Expressions[i].Accept(this));
            if (i < function.Expressions.Length - 1)
            {
                result.Append(", ");
            }
        }

        result.Append(") ");
        return result.ToString().Trim();
    }

    public string Visit(Identifier identifier)
    {
        var result = new StringBuilder();
        result.Append('[').Append(identifier.Name).Append("] ");
        return result.ToString().Trim();
    }

    protected virtual string EncapsulateNoValue(LogicalExpression expression)
    {
        if (expression is ValueExpression valueExpression)
        {
            return valueExpression.Accept(this);
        }

        var result = new StringBuilder();
        result.Append('(');
        result.Append(expression.Accept(this));
        result.Append(") ");
        return result.ToString().Trim();
    }
}
