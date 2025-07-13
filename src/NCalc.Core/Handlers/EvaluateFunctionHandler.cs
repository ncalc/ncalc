namespace NCalc.Handlers;

public delegate ValueTask EvaluateFunctionHandler(string name, FunctionArgs args);