using NCalc.Domain;
using System.Collections.Generic;

namespace NCalc
{
    internal class ParameterExtractionVisitor : LogicalExpressionVisitor
    {
        public readonly HashSet<string> Parameters = new HashSet<string>();

        public override void Visit(Identifier function) => this.Parameters.Add(function.Name);

        public override void Visit(UnaryExpression expression) => expression.Accept(this);

        public override void Visit(BinaryExpression expression)
        {
            expression.LeftExpression.Accept(this);
            expression.RightExpression.Accept(this);
        }

        public override void Visit(TernaryExpression expression)
        {
            expression.LeftExpression.Accept(this);
            expression.RightExpression.Accept(this);
            expression.MiddleExpression.Accept(this);
        }

        public override void Visit(Function function)
        {
            foreach (LogicalExpression expression in function.Expressions)
                expression.Accept(this);
        }

        public override void Visit(LogicalExpression expression) => expression.Accept(this);

        public override void Visit(ValueExpression expression)
        {
        }
    }
}