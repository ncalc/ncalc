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
        var resultBuilder = new StringBuilder();
        resultBuilder.Append(EncapsulateNoValue(expression.LeftExpression));
        resultBuilder.Append("? ");
        resultBuilder.Append(EncapsulateNoValue(expression.MiddleExpression));
        resultBuilder.Append(": ");
        resultBuilder.Append(EncapsulateNoValue(expression.RightExpression));
        return resultBuilder.ToString();
    }

    public string Visit(BinaryExpression expression)
    {
        var resultBuilder = new StringBuilder();
        resultBuilder.Append(EncapsulateNoValue(expression.LeftExpression));

        resultBuilder.Append(expression.Type switch
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
        });

        resultBuilder.Append(EncapsulateNoValue(expression.RightExpression));
        return resultBuilder.ToString();
    }

    public string Visit(UnaryExpression expression)
    {
        var resultBuilder = new StringBuilder();

        resultBuilder.Append(expression.Type switch
        {
            UnaryExpressionType.Not => "!",
            UnaryExpressionType.Negate => "-",
            UnaryExpressionType.BitwiseNot => "~",
            _ => string.Empty
        });

        resultBuilder.Append(EncapsulateNoValue(expression.Expression));

        return resultBuilder.ToString();
    }

    public string Visit(PercentExpression expression)
    {
        return EncapsulateNoValue(expression.Expression).TrimEnd() + "%";
    }

    public string Visit(ValueExpression expression)
    {
        var resultBuilder = new StringBuilder();

        switch (expression.Type)
        {
            case ValueType.Boolean:
                resultBuilder.Append(expression.Value).Append(' ');
                break;
            case ValueType.DateTime or ValueType.TimeSpan:
                resultBuilder.Append('#').Append(expression.Value).Append('#').Append(' ');
                break;
            case ValueType.Float:
                resultBuilder.Append(decimal.Parse(expression.Value?.ToString() ?? string.Empty).ToString(_numberFormatInfo))
                    .Append(' ');
                break;
            case ValueType.Integer:
                resultBuilder.Append(expression.Value).Append(' ');
                break;
            case ValueType.String or ValueType.Char:
                resultBuilder.Append('\'').Append(expression.Value).Append('\'').Append(' ');
                break;
        }

        return resultBuilder.ToString();
    }

    public string Visit(Function function)
    {
        var resultBuilder = new StringBuilder();
        resultBuilder.Append(function.Identifier.Name).Append('(');

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
        var resultBuilder = new StringBuilder().Append('(');
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

        var resultBuilder = new StringBuilder().Append('(');
        resultBuilder.Append(expression.Accept(this));

        while (resultBuilder[^1] == ' ')
            resultBuilder.Length--;

        resultBuilder.Append(") ");
        return resultBuilder.ToString();
    }
}
