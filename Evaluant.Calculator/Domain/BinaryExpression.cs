using System;

namespace NCalc.Domain
{
	public class BinaryExpression : LogicalExpression
	{
		public BinaryExpression(BinaryExpressionType type, LogicalExpression leftExpression, LogicalExpression rightExpression)
		{
            this.type = type;
            this.leftExpression = leftExpression;
            this.rightExpression = rightExpression;
		}

		private LogicalExpression leftExpression;
		
        public LogicalExpression LeftExpression
		{
			get { return leftExpression; }
			set { leftExpression = value; }
		}

		private LogicalExpression rightExpression;
		
        public LogicalExpression RightExpression
		{
			get { return rightExpression; }
			set { rightExpression = value; }
		}

		private BinaryExpressionType type;
		
        public BinaryExpressionType Type
		{
			get { return type; }
			set { type = value; }
		}

        public override void Accept(LogicalExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

	public enum BinaryExpressionType
	{
		And,
		Or,
		NotEqual,
		LesserOrEqual,
		GreaterOrEqual,
		Lesser,
		Greater,
		Equal,
		Minus,
		Plus,
		Modulo,
		Div,
        Times,
        BitwiseOr,
        BitwiseAnd,
        BitwiseXOr,
        LeftShift,
        RightShift,
        Unknown
	}
}
