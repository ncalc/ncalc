namespace NCalc.Exceptions;

public sealed class NCalcParserException : NCalcException
{
    public NCalcParserException(string message) : base(message)
    {
    }
    public NCalcParserException(string message, Exception exception) : base(message, exception)
    {
    }
}