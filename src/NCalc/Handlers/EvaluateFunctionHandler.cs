using NCalc.Domain;

namespace NCalc.Handlers;

public delegate void EvaluateFunctionHandler(string name, FunctionArgs args, string? id = null);