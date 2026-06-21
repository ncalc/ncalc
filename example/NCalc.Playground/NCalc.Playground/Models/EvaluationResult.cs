using System;
using NCalc.Playground.Helpers;

namespace NCalc.Playground.Models;

public sealed class EvaluationResult
{
    public string Value { get; init; }
    public string ValueType { get; init; }
    public Exception? Exception { get; init; }
    public bool HasError => Exception is not null;
    public string ExpressionString { get; set; } = string.Empty;

    private EvaluationResult(string value,
        string valueType,
        Exception? exception)
    {
        Value = value;
        ValueType = valueType;
        Exception = exception;
    }

    public static EvaluationResult Success(object? value)
    {
        return new EvaluationResult(ValueFormatter.Format(value),
            value?.GetType().Name ?? "null",
            null);
    }

    public static EvaluationResult Failure(Exception exception)
    {
        return new EvaluationResult(exception.Message,
            exception.GetType().Name,
            exception);
    }


}
