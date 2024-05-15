using System;

namespace NCalc.Domain;

public class FunctionArgs : EventArgs
{
    private object? _result;

    public object? Result
    {
        get => _result;
        set
        {
            _result = value;
            HasResult = true;
        }
    }

    public bool HasResult { get; set; }

    public Expression[] Parameters { get; init; } = [];

    public object?[] EvaluateParameters()
    {
        var values = new object?[Parameters.Length];
        for (var i = 0; i < values.Length; i++)
        {
            values[i] = Parameters[i].Evaluate();
        }

        return values;
    }
}