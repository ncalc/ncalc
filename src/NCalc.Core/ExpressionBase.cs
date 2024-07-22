namespace NCalc
{
    public abstract class ExpressionBase
    {
        public virtual Task<object?> EvaluateAsync()
        {
            return Task.FromResult<object?>(null);
        }

        public virtual object? Evaluate()
        {
            return null;
        }
    }
}
