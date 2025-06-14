namespace NCalc.Exceptions;

public sealed class NCalcParserException : NCalcException
{
    Parlot.TextPosition _position;

    public Parlot.TextPosition Position => _position;

    public NCalcParserException(string message) : base(message)
    {
    }

    public NCalcParserException(string message, Parlot.TextPosition position) : base(message)
    {
        _position = position;
    }

    public NCalcParserException(string message, Exception exception) : base(message, exception)
    {
    }

    public NCalcParserException(string message, Exception exception, Parlot.TextPosition position) : base(message, exception)
    {
        _position = position;
    }
}