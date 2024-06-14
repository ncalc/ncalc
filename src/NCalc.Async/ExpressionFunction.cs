namespace NCalc;

public delegate Task<object?> AsyncExpressionFunction(AsyncExpression[] arguments, AsyncExpressionContext context);