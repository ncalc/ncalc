using System;

namespace NCalc.Domain
{
	public class Identifier : LogicalExpression
	{
		public Identifier(string name)
		{
            this.name = name;
		}

        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }


        public override void Accept(LogicalExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
