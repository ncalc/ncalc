namespace NCalc.Exceptions;

public sealed class NCalcEvaluationException : NCalcException
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