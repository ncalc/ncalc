using System;

namespace NCalc.Exceptions;

public class NCalcParserException : NCalcException
{
    public NCalcParserException(string message, Exception exception) : base(message, exception)
    {

    }
}