using Microsoft.Extensions.ObjectPool;
using NCalc.Domain;
using ValueType = NCalc.Domain.ValueType;

namespace NCalc.Visitors;

/// <summary>
/// Class responsible to converting a <see cref="LogicalExpression"/> into a <see cref="string"/> representation.
/// </summary>
public class SerializationVisitor : ILogicalExpressionVisitor<string>
{
    private static readonly ObjectPool<StringBuilder> StringBuilderPool;

    static SerializationVisitor()
    {
        StringBuilderPool = new DefaultObjectPoolProvider().CreateStringBuilderPool();
    }

    private readonly NumberFormatInfo _numberFormatInfo = new()
    {
        NumberDecimalSeparator = "."
    };

    public string Visit(TernaryExpression expression)
    {
        var resultBuilder = StringBuilderPool.Get();

        resultBuilder.Append(EncapsulateNoValue(expression.LeftExpression));
        resultBuilder.Append("? ");
        resultBuilder.Append(EncapsulateNoValue(expression.MiddleExpression));
        resultBuilder.Append(": ");
        resultBuilder.Append(EncapsulateNoValue(expression.RightExpression));

        var result = resultBuilder.ToString();
        StringBuilderPool.Return(resultBuilder);

        return result;
    }

    public string Visit(BinaryExpression expression)
    {
        var resultBuilder = StringBuilderPool.Get();
        resultBuilder.Append(EncapsulateNoValue(expression.LeftExpression));

        switch (expression.Type)
        {
            case BinaryExpressionType.And:
                resultBuilder.Append("and ");
                break;
            case BinaryExpressionType.Or:
                resultBuilder.Append("or ");
                break;
            case BinaryExpressionType.Div:
                resultBuilder.Append("/ ");
                break;
            case BinaryExpressionType.Equal:
                resultBuilder.Append("= ");
                break;
            case BinaryExpressionType.Greater:
                resultBuilder.Append("> ");
                break;
            case BinaryExpressionType.GreaterOrEqual:
                resultBuilder.Append(">= ");
                break;
            case BinaryExpressionType.Lesser:
                resultBuilder.Append("< ");
                break;
            case BinaryExpressionType.LesserOrEqual:
                resultBuilder.Append("<= ");
                break;
            case BinaryExpressionType.Minus:
                resultBuilder.Append("- ");
                break;
            case BinaryExpressionType.Modulo:
                resultBuilder.Append("% ");
                break;
            case BinaryExpressionType.NotEqual:
                resultBuilder.Append("!= ");
                break;
            case BinaryExpressionType.Plus:
                resultBuilder.Append("+ ");
                break;
            case BinaryExpressionType.Times:
                resultBuilder.Append("* ");
                break;
            case BinaryExpressionType.BitwiseAnd:
                resultBuilder.Append("& ");
                break;
            case BinaryExpressionType.BitwiseOr:
                resultBuilder.Append("| ");
                break;
            case BinaryExpressionType.BitwiseXOr:
                resultBuilder.Append("^ ");
                break;
            case BinaryExpressionType.LeftShift:
                resultBuilder.Append("<< ");
                break;
            case BinaryExpressionType.RightShift:
                resultBuilder.Append(">> ");
                break;
            case BinaryExpressionType.Exponentiation:
                resultBuilder.Append("** ");
                break;
            case BinaryExpressionType.In:
                resultBuilder.Append("in ");
                break;
            case BinaryExpressionType.NotIn:
                resultBuilder.Append("not in ");
                break;
            case BinaryExpressionType.Like:
                resultBuilder.Append("like ");
                break;
            case BinaryExpressionType.NotLike:
                resultBuilder.Append("not like ");
                break;
            case BinaryExpressionType.Unknown:
                resultBuilder.Append("unknown ");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        resultBuilder.Append(EncapsulateNoValue(expression.RightExpression));

        var result = resultBuilder.ToString();
        StringBuilderPool.Return(resultBuilder);

        return result;
    }

    public string Visit(UnaryExpression expression)
    {
        var resultBuilder = StringBuilderPool.Get();

        switch (expression.Type)
        {
            case UnaryExpressionType.Not:
                resultBuilder.Append('!');
                break;
            case UnaryExpressionType.Negate:
                resultBuilder.Append('-');
                break;
            case UnaryExpressionType.BitwiseNot:
                resultBuilder.Append('~');
                break;
        }

        resultBuilder.Append(EncapsulateNoValue(expression.Expression));

        var result = resultBuilder.ToString();
        StringBuilderPool.Return(resultBuilder);

        return result;
    }

    public string Visit(ValueExpression expression)
    {
        var resultBuilder = StringBuilderPool.Get();

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

        var result = resultBuilder.ToString();
        StringBuilderPool.Return(resultBuilder);

        return result;
    }

    public string Visit(Function function)
    {
        var resultBuilder = StringBuilderPool.Get();
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

        var result = resultBuilder.ToString();
        StringBuilderPool.Return(resultBuilder);

        return result;
    }

    public string Visit(Identifier identifier)
    {
        var resultBuilder = StringBuilderPool.Get();
        resultBuilder.Append('[').Append(identifier.Name).Append("] ");

        var result = resultBuilder.ToString();
        StringBuilderPool.Return(resultBuilder);

        return result;
    }

    public string Visit(LogicalExpressionList list)
    {
        var resultBuilder = StringBuilderPool.Get();
        resultBuilder.Append('(');
        for (var i = 0; i < list.Count; i++)
        {
            resultBuilder.Append(list[i].Accept(this).TrimEnd());
            if (i < list.Count - 1)
            {
                resultBuilder.Append(',');
            }
        }
        resultBuilder.Append(')');

        var result = resultBuilder.ToString();
        StringBuilderPool.Return(resultBuilder);

        return result;
    }

    protected virtual string EncapsulateNoValue(LogicalExpression expression)
    {
        if (expression is ValueExpression valueExpression)
        {
            return valueExpression.Accept(this);
        }

        var resultBuilder = StringBuilderPool.Get();
        resultBuilder.Append('(');
        resultBuilder.Append(expression.Accept(this));

        // trim spaces before adding a closing paren
        while (resultBuilder[^1] == ' ')
            resultBuilder.Remove(resultBuilder.Length - 1, 1);

        resultBuilder.Append(") ");

        var result = resultBuilder.ToString();
        StringBuilderPool.Return(resultBuilder);

        return result;
    }
}