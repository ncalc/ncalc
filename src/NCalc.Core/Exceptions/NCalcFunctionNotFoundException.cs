namespace NCalc.Exceptions;

public sealed class NCalcFunctionNotFoundException(string functionName)
    : NCalcEvaluationException($"Function not found. Name: {functionName}")
{
    public string FunctionName { get; } = functionName;
}