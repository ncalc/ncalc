namespace NCalc.Exceptions;

public class NCalcEvaluationException : NCalcException
{
    public NCalcEvaluationException(string message) : base(message)
    {
    }

    public NCalcEvaluationException(string message, Exception exception) : base(message, exception)
    {
    }
}
