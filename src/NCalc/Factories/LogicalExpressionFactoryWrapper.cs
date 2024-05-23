using NCalc.Domain;

namespace NCalc.Factories;

/// <summary>
/// Default LogicalExpressionFactory implementation.
/// </summary>
internal sealed class LogicalExpressionFactoryWrapper : ILogicalExpressionFactory
{
    private static LogicalExpressionFactoryWrapper? _instance;
    private LogicalExpressionFactoryWrapper()
    {
        
    }

    public static LogicalExpressionFactoryWrapper GetInstance()
    {
        return _instance ??= new LogicalExpressionFactoryWrapper();
    }
    
    public LogicalExpression Create(string expression, ExpressionOptions expressionOptions)
    {
        return LogicalExpressionFactory.Create(expression, expressionOptions);
    }
}