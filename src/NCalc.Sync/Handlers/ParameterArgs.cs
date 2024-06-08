﻿namespace NCalc.Handlers;

public class ParameterArgs : EventArgs
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

    public bool HasResult { get; private set; }
}