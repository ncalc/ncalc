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

    public virtual string Visit(TernaryExpression expression, CancellationToken ct = default)
    {
        string result = EncapsulateNoValue(expression.LeftExpression, ct) + "? ";
        result += EncapsulateNoValue(expression.MiddleExpression, ct) + ": ";
        result += EncapsulateNoValue(expression.RightExpression, ct);

        return result;
    }

    public virtual string Visit(BinaryExpression expression, CancellationToken ct = default)
    {
        string result = EncapsulateNoValue(expression.LeftExpression, ct);

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

        result += EncapsulateNoValue(expression.RightExpression, ct);
        return result;
    }

    public virtual string Visit(UnaryExpression expression, CancellationToken ct = default)
    {
        string result = expression.Type switch
        {
            UnaryExpressionType.Not => "!",
            UnaryExpressionType.Negate => "-",
            UnaryExpressionType.BitwiseNot => "~",
            _ => string.Empty
        };

        result += EncapsulateNoValue(expression.Expression, ct);
        return result;
    }

    public virtual string Visit(ValueExpression expression, CancellationToken ct = default)
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

    public virtual string Visit(Function function, CancellationToken ct = default)
    {
        var resultBuilder = new StringBuilder(function.Identifier.Name + '(');

        for (int i = 0; i < function.Parameters.Count; i++)
        {
            resultBuilder.Append(function.Parameters[i].Accept(this, ct));
            if (i < function.Parameters.Count - 1)
            {
                if (resultBuilder[^1] == ' ')
                    resultBuilder.Remove(resultBuilder.Length - 1, 1);

                resultBuilder.Append(", ");
            }
        }

        while (resultBuilder[^1] == ' ')
            resultBuilder.Remove(resultBuilder.Length - 1, 1);

        resultBuilder.Append(") ");
        return resultBuilder.ToString();
    }

    public virtual string Visit(Identifier identifier, CancellationToken ct = default)
    {
        return $"[{identifier.Name}]";
    }

    public virtual string Visit(LogicalExpressionList list, CancellationToken ct = default)
    {
        var resultBuilder = new StringBuilder("(");
        for (var i = 0; i < list.Count; i++)
        {
            resultBuilder.Append(list[i].Accept(this, ct).TrimEnd());
            if (i < list.Count - 1)
            {
                resultBuilder.Append(',');
            }
        }
        resultBuilder.Append(')');
        return resultBuilder.ToString();
    }

    protected virtual string EncapsulateNoValue(LogicalExpression expression, CancellationToken ct = default)
    {
        if (expression is ValueExpression valueExpression)
            return valueExpression.Accept(this, ct);

        string result = expression.Accept(this, ct);
        return $"({result.TrimEnd(' ')}) ";
    }
}