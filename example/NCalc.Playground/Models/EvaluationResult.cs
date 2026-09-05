using System;
using NCalc.Playground.Helpers;

namespace NCalc.Playground.Models;

public sealed class EvaluationResult
{
    public object? Value { get; init; }
    public Type Type { get; init; }
    public Exception? Exception { get; init; }
    public bool HasError => Exception is not null;
    public string ExpressionString { get; set; } = string.Empty;

    private EvaluationResult(object? value,
        Type type,
        Exception? exception)
    {
        Value = value;
        Type = type;
        Exception = exception;
    }

    public static EvaluationResult Success(object? value)
    {
        return new EvaluationResult(value,
            value?.GetType() ?? typeof(void),
            null);
    }

    public static EvaluationResult Failure(Exception exception)
    {
        return new EvaluationResult(exception.Message,
            exception.GetType(),
            exception);
    }
}
