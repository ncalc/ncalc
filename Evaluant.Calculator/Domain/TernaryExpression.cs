using System;
using System.Collections.Generic;
using System.Text;

namespace NCalc.Domain
{
	public class TernaryExpression : LogicalExpression
	{
        public TernaryExpression(LogicalExpression leftExpression, LogicalExpression middleExpression, LogicalExpression rightExpression)
		{
            this.leftExpression = leftExpression;
            this.middleExpression = middleExpression;
            this.rightExpression = rightExpression;
		}

        private LogicalExpression leftExpression;

        public LogicalExpression LeftExpression
		{
			get { return leftExpression; }
			set { leftExpression = value; }
		}

        private LogicalExpression middleExpression;

        public LogicalExpression MiddleExpression
        {
            get { return middleExpression; }
            set { middleExpression = value; }
        }

        private LogicalExpression rightExpression;

        public LogicalExpression RightExpression
		{
			get { return rightExpression; }
			set { rightExpression = value; }
		}

        public override void Accept(LogicalExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }

    }   
}