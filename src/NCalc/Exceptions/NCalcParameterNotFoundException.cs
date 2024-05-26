namespace NCalc.Exceptions;

public sealed class NCalcParameterNotFoundException(string parameterName)
    : NCalcEvaluationException($"Parameter {parameterName} not found.")
{
    public string ParameterName { get; } = parameterName;
}