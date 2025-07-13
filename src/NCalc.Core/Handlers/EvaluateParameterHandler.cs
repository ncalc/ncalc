namespace NCalc.Handlers;

public delegate ValueTask EvaluateParameterHandler(string name, ParameterArgs args);