using NCalc.Visitors;

namespace NCalc.Extensions;

public static class LogicalExpressionExtensions
{
    extension(LogicalExpression expression)
    {
        public string ToExpressionString()
        {
            return expression.Accept(new SerializationVisitor()).TrimEnd();
        }
    }
}
