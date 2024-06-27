namespace NCalc;

public delegate ValueTask<object?> AsyncExpressionFunction(AsyncExpressionFunctionData data);