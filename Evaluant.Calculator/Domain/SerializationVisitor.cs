using System;
using System.Text;
using System.Collections;
using System.Globalization;

namespace NCalc.Domain
{
    public class SerializationVisitor : LogicalExpressionVisitor
    {
        private NumberFormatInfo numberFormatInfo;

        public SerializationVisitor()
        {
            numberFormatInfo = new NumberFormatInfo();
            numberFormatInfo.NumberDecimalSeparator = ".";
        }

        protected StringBuilder result = new StringBuilder();
        public StringBuilder Result
        {
            get { return result; }
        }

        public override void Visit(LogicalExpression expression)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override void Visit(TernaryExpression expression)
        {
            EncapsulateNoValue(expression.LeftExpression);

            result.Append("? ");

            EncapsulateNoValue(expression.MiddleExpression);

            result.Append(": ");

            EncapsulateNoValue(expression.RightExpression);
        }

        public override void Visit(BinaryExpression expression)
        {
            EncapsulateNoValue(expression.LeftExpression);

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
                    result.Append("~ ");
                    break;

                case BinaryExpressionType.LeftShift:
                    result.Append("<< ");
                    break;

                case BinaryExpressionType.RightShift:
                    result.Append(">> ");
                    break;
            }

            EncapsulateNoValue(expression.RightExpression);
        }

        public override void Visit(UnaryExpression expression)
        {
            switch (expression.Type)
            {
                case UnaryExpressionType.Not:
                    result.Append("!");
                    break;

                case UnaryExpressionType.Negate:
                    result.Append("-");
                    break;

                case UnaryExpressionType.BitwiseNot:
                    result.Append("~");
                    break;
            }

            EncapsulateNoValue(expression.Expression);
        }

        public override void Visit(ValueExpression expression)
        {
            switch (expression.Type)
            {
                case ValueType.Boolean:
                    result.Append(expression.Value.ToString()).Append(" ");
                    break;

                case ValueType.DateTime:
                    result.Append("#").Append(expression.Value.ToString()).Append("#").Append(" ");
                    break;

                case ValueType.Float:
                    result.Append(decimal.Parse(expression.Value.ToString()).ToString(numberFormatInfo)).Append(" ");
                    break;

                case ValueType.Integer:
                    result.Append(expression.Value.ToString()).Append(" ");
                    break;

                case ValueType.String:
                    result.Append("'").Append(expression.Value.ToString()).Append("'").Append(" ");
                    break;
            }
        }

        public override void Visit(Function function)
        {
            result.Append(function.Identifier.Name);

            result.Append("(");

            for(int i=0; i<function.Expressions.Length; i++)
            {
                function.Expressions[i].Accept(this);
                if (i < function.Expressions.Length-1)
                {
                    result.Remove(result.Length - 1, 1);
                    result.Append(", ");
                }
            }

            result.Append(") ");
        }

        public override void Visit(Identifier parameter)
        {
            result.Append("[").Append(parameter.Name).Append("] ");
        }

        protected void EncapsulateNoValue(LogicalExpression expression)
        {
            if (expression is ValueExpression)
            {
                expression.Accept(this);
            }
            else
            {
                result.Append("(");
                expression.Accept(this);
                result.Remove(result.Length - 1, 1);
                result.Append(")");
            }
        }

    }
}
