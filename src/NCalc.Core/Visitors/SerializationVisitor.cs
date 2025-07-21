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

        result += expression.Type switch
        {
            BinaryExpressionType.And => "and ",
            BinaryExpressionType.Or => "or ",
            BinaryExpressionType.Div => "/ ",
            BinaryExpressionType.Equal => "= ",
            BinaryExpressionType.Greater => "> ",
            BinaryExpressionType.GreaterOrEqual => ">= ",
            BinaryExpressionType.Lesser => "< ",
            BinaryExpressionType.LesserOrEqual => "<= ",
            BinaryExpressionType.Minus => "- ",
            BinaryExpressionType.Modulo => "% ",
            BinaryExpressionType.NotEqual => "!= ",
            BinaryExpressionType.Plus => "+ ",
            BinaryExpressionType.Times => "* ",
            BinaryExpressionType.BitwiseAnd => "& ",
            BinaryExpressionType.BitwiseOr => "| ",
            BinaryExpressionType.BitwiseXOr => "^ ",
            BinaryExpressionType.LeftShift => "<< ",
            BinaryExpressionType.RightShift => ">> ",
            BinaryExpressionType.Exponentiation => "** ",
            BinaryExpressionType.In => "in ",
            BinaryExpressionType.NotIn => "not in ",
            BinaryExpressionType.Like => "like ",
            BinaryExpressionType.NotLike => "not like ",
            BinaryExpressionType.Unknown => "unknown ",
            _ => throw new ArgumentOutOfRangeException()
        };

        result += EncapsulateNoValue(expression.RightExpression);
        return result;
    }

    public string Visit(UnaryExpression expression)
    {
        string result = expression.Type switch
        {
            UnaryExpressionType.Not => "!",
            UnaryExpressionType.Negate => "-",
            UnaryExpressionType.BitwiseNot => "~",
            _ => string.Empty
        };

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
        var resultBuilder = new StringBuilder(function.Identifier.Name + '(');

        for (int i = 0; i < function.Parameters.Count; i++)
        {
            resultBuilder.Append(function.Parameters[i].Accept(this));
            if (i < function.Parameters.Count - 1)
            {
                resultBuilder.Remove(resultBuilder.Length - 1, 1);
                resultBuilder.Append(", ");
            }
        }

        while (resultBuilder[^1] == ' ')
            resultBuilder.Remove(resultBuilder.Length - 1, 1);

        resultBuilder.Append(") ");
        return resultBuilder.ToString();
    }

    public string Visit(Identifier identifier)
    {
        return $"[{identifier.Name}]";
    }

    public string Visit(LogicalExpressionList list)
    {
        var resultBuilder = new StringBuilder('(');
        for (var i = 0; i < list.Count; i++)
        {
            resultBuilder.Append(list[i].Accept(this).TrimEnd());
            if (i < list.Count - 1)
            {
                resultBuilder.Append(',');
            }
        }
        resultBuilder.Append(')');
        return resultBuilder.ToString();
    }

    protected virtual string EncapsulateNoValue(LogicalExpression expression)
    {
        if (expression is ValueExpression valueExpression)
            return valueExpression.Accept(this);

        string result = expression.Accept(this);
        return $"({result.TrimEnd(' ')}) ";
    }
}