using System;

namespace NCalc.Exceptions;

public class NCalcException : Exception
{
    protected NCalcException(string message, Exception exception) : base(message, exception)
    {
     
    }
}