using NCalc.Domain;

using ValueType = NCalc.Domain.ValueType;

namespace NCalc.Visitors;

/// <summary>
/// Class responsible to converting a <see cref="LogicalExpression"/> into a <see cref="string"/> representation.
/// </summary>
public class SerializationVisitor(SerializationContext context) : ILogicalExpressionVisitor<string>
{
    private readonly NumberFormatInfo _numberFormatInfo = new()
    {
        NumberDecimalSeparator = (context.AdvancedOptions == null) ? "." : context.AdvancedOptions.GetDecimalSeparatorChar().ToString()
    };

    public string Visit(TernaryExpression expression)
    {
        expression.SetOptions(context.Options, context.CultureInfo, context.AdvancedOptions);

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
        expression.SetOptions(context.Options, context.CultureInfo, context.AdvancedOptions);

        var resultBuilder = new StringBuilder();

        if (expression.Type == BinaryExpressionType.Factorial)
        {
            if ((expression.RightExpression is ValueExpression valueExpression) && (valueExpression.Type == ValueType.Integer) && (valueExpression.Value != null))
            {
                resultBuilder.Append(EncapsulateNoValue(expression.LeftExpression, false));

                var step = (int)valueExpression.Value;
                StringBuilder builder = new StringBuilder(step + 1);
                for (int i = 0; i < step; i++)
                    builder.Append('!');
                resultBuilder.Append(builder);
                return resultBuilder.ToString();
            }
        }
        else
        {
            resultBuilder.Append(EncapsulateNoValue(expression.LeftExpression));

            resultBuilder.Append(expression.Type switch
            {
                BinaryExpressionType.Assignment => context.Options.HasFlag(ExpressionOptions.UseCStyleAssignments) ? "= " : ":= ",
                BinaryExpressionType.And => "and ",
                BinaryExpressionType.Or => "or ",
                BinaryExpressionType.XOr => "xor ",
                BinaryExpressionType.Div => context.Options.HasFlag(ExpressionOptions.UseUnicodeCharsForOperations) ? "\u00F7 " : "/ ",
                BinaryExpressionType.Equal => context.Options.HasFlag(ExpressionOptions.UseCStyleAssignments) ? "== " : "= ",
                BinaryExpressionType.Greater => "> ",
                BinaryExpressionType.GreaterOrEqual => context.Options.HasFlag(ExpressionOptions.UseUnicodeCharsForOperations) ? "\u2265 " : ">= ",
                BinaryExpressionType.Less => "< ",
                BinaryExpressionType.LessOrEqual => context.Options.HasFlag(ExpressionOptions.UseUnicodeCharsForOperations) ? "\u2264 " : "<= ",
                BinaryExpressionType.Minus => "- ",
                BinaryExpressionType.Modulo => (context.AdvancedOptions != null && context.AdvancedOptions.Flags.HasFlag(AdvExpressionOptions.CalculatePercent)) ? "mod " : "% ",
                BinaryExpressionType.NotEqual => context.Options.HasFlag(ExpressionOptions.UseUnicodeCharsForOperations) ? "\u2260 " : "!= ",
                BinaryExpressionType.Plus => "+ ",
                BinaryExpressionType.Times => context.Options.HasFlag(ExpressionOptions.UseUnicodeCharsForOperations) ? "\u00D7 " :  "* ",
                BinaryExpressionType.BitwiseAnd => context.Options.HasFlag(ExpressionOptions.SkipLogicalAndBitwiseOpChars) ? "bit_and " :  "& ",
                BinaryExpressionType.BitwiseOr => context.Options.HasFlag(ExpressionOptions.SkipLogicalAndBitwiseOpChars) ? "bit_or " : "| ",
                BinaryExpressionType.BitwiseXOr => context.Options.HasFlag(ExpressionOptions.SkipLogicalAndBitwiseOpChars) ? "bit_xor " : "^ ",
                BinaryExpressionType.LeftShift => "<< ",
                BinaryExpressionType.RightShift => ">> ",
                BinaryExpressionType.Exponentiation => context.Options.HasFlag(ExpressionOptions.SkipLogicalAndBitwiseOpChars) ? (context.Options.HasFlag(ExpressionOptions.UseUnicodeCharsForOperations) ? "\u2291 " : "^ ") : "** ",
                BinaryExpressionType.In => context.Options.HasFlag(ExpressionOptions.UseUnicodeCharsForOperations) ? "\u2208 " : "in ",
                BinaryExpressionType.NotIn => context.Options.HasFlag(ExpressionOptions.UseUnicodeCharsForOperations) ? "\u2209 " : "not in ",
                BinaryExpressionType.Like => "like ",
                BinaryExpressionType.NotLike => "not like ",
                BinaryExpressionType.Unknown => "unknown ",
                _ => throw new ArgumentOutOfRangeException()
            });
        }
        resultBuilder.Append(EncapsulateNoValue(expression.RightExpression));
        return resultBuilder.ToString();
    }

    public string Visit(UnaryExpression expression)
    {
        expression.SetOptions(context.Options, context.CultureInfo, context.AdvancedOptions);

        var resultBuilder = new StringBuilder();

        resultBuilder.Append(expression.Type switch
        {
            UnaryExpressionType.Not => "!",
            UnaryExpressionType.Negate => "-",
            UnaryExpressionType.BitwiseNot => context.Options.HasFlag(ExpressionOptions.SkipLogicalAndBitwiseOpChars) ? "bit_xor " : "~",
            UnaryExpressionType.SqRoot => "\u221a",
#if NET8_0_OR_GREATER
            UnaryExpressionType.CbRoot => "\u221b",
#endif
            UnaryExpressionType.FourthRoot => "\u221c",
            _ => string.Empty
        });

        resultBuilder.Append(EncapsulateNoValue(expression.Expression));

        return resultBuilder.ToString();
    }

    public string Visit(PercentExpression expression)
    {
        expression.SetOptions(context.Options, context.CultureInfo, context.AdvancedOptions);

        return EncapsulateNoValue(expression.Expression).TrimEnd() + "%";
    }

    public string Visit(ValueExpression expression)
    {
        expression.SetOptions(context.Options, context.CultureInfo, context.AdvancedOptions);

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
        function.SetOptions(context.Options, context.CultureInfo, context.AdvancedOptions);

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
        list.SetOptions(context.Options, context.CultureInfo, context.AdvancedOptions);

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

    protected virtual string EncapsulateNoValue(LogicalExpression expression, bool appendSpace = true)
    {
        if (expression is ValueExpression valueExpression)
        {
            string result = valueExpression.Accept(this);
            if (!appendSpace)
                result = result.TrimEnd();
            return result;
        }

        var resultBuilder = new StringBuilder();

        // Factorials don't need parenthesis around them
        bool parensNeeded = true;

        if (((expression is BinaryExpression binaryExpression) && (binaryExpression.Type == BinaryExpressionType.Factorial)) || (expression is PercentExpression))
            parensNeeded = false;

        if (parensNeeded)
            resultBuilder.Append('(');
        resultBuilder.Append(expression.Accept(this));

        while (resultBuilder[^1] == ' ')
            resultBuilder.Length--;

        if (parensNeeded)
        {
            if (appendSpace)
                resultBuilder.Append(") ");
            else
                resultBuilder.Append(')');
        }
        else
        if (appendSpace)
            resultBuilder.Append(' ');

        return resultBuilder.ToString();
    }
}
