namespace NCalc.Exceptions;

public sealed class NCalcFunctionNotFoundException(string message, string functionName) 
    : NCalcEvaluationException(message)
{
    public string FunctionName { get;} = functionName;
}