namespace NCalc;

public delegate ValueTask<object?> AsyncExpressionFunction(AsyncExpression[] arguments, AsyncExpressionContext context);