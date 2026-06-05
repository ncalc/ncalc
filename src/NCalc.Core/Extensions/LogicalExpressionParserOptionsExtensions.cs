using NCalc.Parser;

namespace NCalc.Extensions;

public static class LogicalExpressionParserOptionsExtensions
{
    extension(LogicalExpressionParserOptions)
    {
        public static LogicalExpressionParserOptions Create(ExpressionOptions options, CultureInfo cultureInfo)
        {
            return new LogicalExpressionParserOptions
            {
                DecimalAsDefault = options.HasFlag(ExpressionOptions.DecimalAsDefault),
                AllowCharValues = options.HasFlag(ExpressionOptions.AllowCharValues),
                LongAsDefault = options.HasFlag(ExpressionOptions.LongAsDefault),
                CultureInfo = cultureInfo
            };
        }
    }
}