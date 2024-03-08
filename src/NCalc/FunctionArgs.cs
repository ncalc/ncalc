using System;

namespace NCalc;

public class FunctionArgs : EventArgs
{
    private object _result;

    public object Result
    {
        get => _result;
        set
        {
            _result = value;
            HasResult = true;
        }
    }

    public bool HasResult { get; set; }

    private Expression[] _parameters = [];

    public Expression[] Parameters
    {
        get => _parameters;
        set => _parameters = value;
    }

    public object[] EvaluateParameters()
    {
        var values = new object[_parameters.Length];
        for (var i = 0; i < values.Length; i++)
        {
            values[i] = _parameters[i].Evaluate();
        }

        return values;
    }
}