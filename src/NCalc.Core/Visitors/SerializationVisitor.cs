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

    public virtual string Visit(TernaryExpression expression)
    {
        string result = EncapsulateNoValue(expression.LeftExpression) + "? ";
        result += EncapsulateNoValue(expression.MiddleExpression) + ": ";
        result += EncapsulateNoValue(expression.RightExpression);

        return result;
    }

    public virtual string Visit(BinaryExpression expression)
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
            BinaryExpressionType.Coalesce => "?? ",
            BinaryExpressionType.Unknown => "unknown ",
            _ => throw new ArgumentOutOfRangeException()
        };

        result += EncapsulateNoValue(expression.RightExpression);
        return result;
    }

    public virtual string Visit(UnaryExpression expression)
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

    public virtual string Visit(ValueExpression expression)
    {
        return expression.Type switch
        {
            ValueType.Boolean or ValueType.Integer or ValueType.Guid => $"{expression.Value} ",
            ValueType.DateTime when expression.Value is DateTime dateTime => $"#{FormatDateTime(dateTime, CultureInfo.CurrentCulture)}# ",
            ValueType.DateTime or ValueType.TimeSpan => $"#{expression.Value}# ",
            ValueType.Float => $"{((IFormattable)expression.Value!).ToString(null, _numberFormatInfo)} ",
            ValueType.String or ValueType.Char => $"'{EscapeString(Convert.ToString(expression.Value, CultureInfo.InvariantCulture))}' ",
            _ => "",
        };
    }

    // The parser reads date literals as number/number/number number:number:number{.fraction} with the
    // culture's separators, so write that shape instead of DateTime.ToString(), which can add AM/PM
    // designators or other text the parser rejects, and drops fractional seconds.
    private static string FormatDateTime(DateTime value, CultureInfo culture)
    {
        // Keep the culture's day, month and year order (and its MM/dd padding), but always use a four digit year.
        var pattern = culture.DateTimeFormat.ShortDatePattern;
        var dateParts = new List<string>(3);
        for (var i = 0; i < pattern.Length; i++)
        {
            var c = pattern[i];
            if (c is not ('y' or 'M' or 'd'))
                continue;

            var length = 1;
            while (i + 1 < pattern.Length && pattern[i + 1] == c)
            {
                length++;
                i++;
            }

            var part = c == 'y' ? "yyyy" : new string(c, Math.Min(length, 2));
            if (!dateParts.Exists(p => p[0] == c))
                dateParts.Add(part);
        }

        if (dateParts.Count != 3)
            dateParts = ["MM", "dd", "yyyy"];

        var result = value.ToString(string.Join("/", dateParts) + " HH:mm:ss", culture);

        var fraction = value.ToString("FFFFFFF", CultureInfo.InvariantCulture);
        if (fraction.Length > 0)
            result += culture.NumberFormat.NumberDecimalSeparator + fraction;

        return result;
    }

    private static string EscapeString(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        var builder = new StringBuilder(value!.Length);
        foreach (var c in value)
        {
            switch (c)
            {
                case '\\':
                    builder.Append("\\\\");
                    break;
                case '\'':
                    builder.Append("\\'");
                    break;
                case '\r':
                    builder.Append("\\r");
                    break;
                case '\n':
                    builder.Append("\\n");
                    break;
                default:
                    builder.Append(c);
                    break;
            }
        }

        return builder.ToString();
    }

    public virtual string Visit(Function function)
    {
        var resultBuilder = new StringBuilder(function.Identifier.Name + '(');

        for (int i = 0; i < function.Parameters.Count; i++)
        {
            resultBuilder.Append(function.Parameters[i].Accept(this));
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

    public virtual string Visit(Identifier identifier)
    {
        return $"[{identifier.Name}]";
    }

    public virtual string Visit(LogicalExpressionList list)
    {
        var resultBuilder = new StringBuilder("(");
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
