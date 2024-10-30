using NCalc.Domain;
using ValueType = NCalc.Domain.ValueType;

namespace NCalc.Visitors;

/// <summary>
/// Class responsible to converting a <see cref="LogicalExpression"/> into a <see cref="string"/> representation.
/// </summary>
public class SerializationVisitor : ILogicalExpressionVisitor<string>
{
    private readonly NumberFormatInfo _numberFormatInfo = new()
    {
        NumberDecimalSeparator = "."
    };

    public string Visit(TernaryExpression expression)
    {
        string result = EncapsulateNoValue(expression.LeftExpression) + "? ";
        result += EncapsulateNoValue(expression.MiddleExpression) + ": ";
        result += EncapsulateNoValue(expression.RightExpression);

        return result;
    }

    public string Visit(BinaryExpression expression)
    {
        string result = EncapsulateNoValue(expression.LeftExpression);

        switch (expression.Type)
        {
            case BinaryExpressionType.And:
                result += "and ";
                break;
            case BinaryExpressionType.Or:
                result += "or ";
                break;
            case BinaryExpressionType.Div:
                result += "/ ";
                break;
            case BinaryExpressionType.Equal:
                result += "= ";
                break;
            case BinaryExpressionType.Greater:
                result += "> ";
                break;
            case BinaryExpressionType.GreaterOrEqual:
                result += ">= ";
                break;
            case BinaryExpressionType.Lesser:
                result += "< ";
                break;
            case BinaryExpressionType.LesserOrEqual:
                result += "<= ";
                break;
            case BinaryExpressionType.Minus:
                result += "- ";
                break;
            case BinaryExpressionType.Modulo:
                result += "% ";
                break;
            case BinaryExpressionType.NotEqual:
                result += "!= ";
                break;
            case BinaryExpressionType.Plus:
                result += "+ ";
                break;
            case BinaryExpressionType.Times:
                result += "* ";
                break;
            case BinaryExpressionType.BitwiseAnd:
                result += "& ";
                break;
            case BinaryExpressionType.BitwiseOr:
                result += "| ";
                break;
            case BinaryExpressionType.BitwiseXOr:
                result += "^ ";
                break;
            case BinaryExpressionType.LeftShift:
                result += "<< ";
                break;
            case BinaryExpressionType.RightShift:
                result += ">> ";
                break;
            case BinaryExpressionType.Exponentiation:
                result += "** ";
                break;
            case BinaryExpressionType.In:
                result += "in ";
                break;
            case BinaryExpressionType.NotIn:
                result += "not in ";
                break;
            case BinaryExpressionType.Like:
                result += "like ";
                break;
            case BinaryExpressionType.NotLike:
                result += "not like ";
                break;
            case BinaryExpressionType.Unknown:
                result += "unknown ";
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        result += EncapsulateNoValue(expression.RightExpression);
        return result;
    }

    public string Visit(UnaryExpression expression)
    {
        string result = "";

        switch (expression.Type)
        {
            case UnaryExpressionType.Not:
                result = "!";
                break;
            case UnaryExpressionType.Negate:
                result = "-";
                break;
            case UnaryExpressionType.BitwiseNot:
                result = "~";
                break;
        }

        result += EncapsulateNoValue(expression.Expression);
        return result;
    }

    public string Visit(ValueExpression expression)
    {
        return expression.Type switch
        {
            ValueType.Boolean or ValueType.Integer => $"{expression.Value} ",
            ValueType.DateTime or ValueType.TimeSpan => $"#{expression.Value}# ",
            ValueType.Float => $"{decimal.Parse(expression.Value?.ToString() ?? string.Empty).ToString(_numberFormatInfo)} ",
            ValueType.String or ValueType.Char => $"'{expression.Value}' ",
            _ => "",
        };
    }

    public string Visit(Function function)
    {
        var result = new StringBuilder();
        result.Append(function.Identifier.Name).Append('(');

        for (int i = 0; i < function.Parameters.Count; i++)
        {
            result.Append(function.Parameters[i].Accept(this));
            if (i < function.Parameters.Count - 1)
            {
                result.Remove(result.Length - 1, 1);
                result.Append(", ");
            }
        }

        while (result[^1] == ' ')
            result.Remove(result.Length - 1, 1);

        result.Append(") ");
        return result.ToString();
    }

    public string Visit(Identifier identifier)
    {
        return $"[{identifier.Name}] ";
    }

    public string Visit(LogicalExpressionList list)
    {
        var result = new StringBuilder();
        result.Append('(');
        for (var i = 0; i < list.Count; i++)
        {
            result.Append(list[i].Accept(this).TrimEnd());
            if (i < list.Count - 1)
            {
                result.Append(',');
            }
        }
        result.Append(')');
        return result.ToString();
    }

    protected virtual string EncapsulateNoValue(LogicalExpression expression)
    {
        if (expression is ValueExpression valueExpression)
        {
            return valueExpression.Accept(this);
        }

        string result = expression.Accept(this);
        return $"({result.TrimEnd(' ')}) ";
    }
}
