using System.Collections.Generic;
using System.IO;
using Antlr4.Runtime;

namespace NCalc
{
    public class ErrorListenerParser : IAntlrErrorListener<IToken>
    {
        public readonly List<string> Errors = new List<string>();

        public void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg,
            RecognitionException e)
        {
            string errorMessage = $"{msg} at {line}:{charPositionInLine + 1}";
            Errors.Add(errorMessage);
        }
    }

    public class ErrorListenerLexer : IAntlrErrorListener<int>
    {
        public readonly List<string> Errors = new List<string>();

        public void SyntaxError(TextWriter output, IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            string errorMessage = $"{msg} at {line}:{charPositionInLine + 1}";
            Errors.Add(errorMessage);
        }
    }
}