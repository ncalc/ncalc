namespace NCalc.Helpers
{
    public static class AsyncBuiltInFunctionHelper
    {
        public static async Task<object?> EvaluateAsync(string functionName, ExpressionBase[] arguments, ExpressionContextBase context)
        {
            return await BuiltInFunctionHelper.Evaluate(functionName, arguments, context, EvaluateAsyncInt);
        }

        private static async Task<object?> EvaluateAsyncInt(ExpressionBase expressionBase)
        {
            return await expressionBase.EvaluateAsync();
        }
    }
}
