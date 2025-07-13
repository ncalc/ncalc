namespace NCalc;

public delegate ValueTask<object?> ExpressionAsyncFunction(ExpressionFunctionData data);