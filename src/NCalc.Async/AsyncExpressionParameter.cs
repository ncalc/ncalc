namespace NCalc;

public delegate ValueTask<object?> AsyncExpressionParameter(AsyncExpressionParameterData data);