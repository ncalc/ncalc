using System;

namespace NCalc.Domain
{
	public class Function : LogicalExpression
	{
		public Function(Identifier identifier, LogicalExpression[] expressions)
		{
            this.identifier = identifier;
            this.expressions = expressions;
		}

        private Identifier identifier;

        public Identifier Identifier
        {
            get { return identifier; }
            set { identifier = value; }
        }

        private LogicalExpression[] expressions;

        public LogicalExpression[] Expressions
        {
            get { return expressions; }
            set { expressions = value; }
        }

        public override void Accept(LogicalExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
