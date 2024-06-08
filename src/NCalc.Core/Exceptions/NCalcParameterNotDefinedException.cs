namespace NCalc.Exceptions;

public sealed class NCalcParameterNotDefinedException(string parameterName)
    : NCalcEvaluationException($"Parameter {parameterName} not defined.")
{
    public string ParameterName { get; } = parameterName;
}