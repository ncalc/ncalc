using Antlr4.Runtime;
using NCalc.Domain;
using NCalc.Exceptions;
using NCalc.Factories;

namespace NCalc.Antlr;

/// <summary>
/// Antlr implementation of <see cref="ILogicalExpressionFactory"/>.
/// </summary>
public sealed class AntlrLogicalExpressionFactory : ILogicalExpressionFactory
{
    public LogicalExpression Create(string expression, ExpressionOptions options)
    {
        LogicalExpression logicalExpression;
        var lexer = new NCalcLexer(new AntlrInputStream(expression));
        var errorListenerLexer = new ErrorListenerLexer();
        lexer.AddErrorListener(errorListenerLexer);

        var parser = new NCalcParser(new CommonTokenStream(lexer))
        {
            UseDecimal = options.HasFlag(ExpressionOptions.DecimalAsDefault)
        };

        var errorListenerParser = new ErrorListenerParser();
        parser.AddErrorListener(errorListenerParser);

        try
        {
            logicalExpression = parser.ncalcExpression().retValue;
        }
        catch (Exception ex)
        {
            var message = new StringBuilder(ex.Message);
            if (errorListenerLexer.Errors.Count != 0)
            {
                message.AppendLine();
                message.AppendLine(string.Join(Environment.NewLine, errorListenerLexer.Errors.ToArray()));
            }
            if (errorListenerParser.Errors.Count != 0)
            {
                message.AppendLine();
                message.AppendLine(string.Join(Environment.NewLine, errorListenerParser.Errors.ToArray()));
            }

            throw new NCalcParserException(message.ToString());
        }
        if (errorListenerLexer.Errors.Count != 0)
        {
            throw new NCalcParserException(string.Join(Environment.NewLine, errorListenerLexer.Errors.ToArray()));
        }
        if (errorListenerParser.Errors.Count != 0)
        {
            throw new NCalcParserException(string.Join(Environment.NewLine, errorListenerParser.Errors.ToArray()));
        }

        return logicalExpression;
    }
}