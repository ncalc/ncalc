namespace NCalc.Helpers
{
    public static class SyncBuiltInFunctionHelper
    {
        public static object? Evaluate(string functionName, ExpressionBase[] arguments, ExpressionContextBase context)
        {
            var result = BuiltInFunctionHelper.Evaluate(functionName, arguments, context, EvaluateInt);
            return result.GetAwaiter().GetResult();
        }

        private static async Task<object?> EvaluateInt(ExpressionBase expressionBase)
        {
            return await Task.FromResult(expressionBase.Evaluate());
        }
    }
}
