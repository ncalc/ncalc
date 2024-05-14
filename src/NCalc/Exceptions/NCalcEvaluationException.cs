using System;

namespace NCalc.Exceptions;

public class NCalcEvaluationException : ApplicationException
{
    public NCalcEvaluationException(string message)
        : base(message)
    {
    }

    public NCalcEvaluationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}