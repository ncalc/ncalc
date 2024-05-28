namespace NCalc.Visitors;

/// <summary>
/// ILogicalExpressionVisitor dedicated to extract parameters from an expression.
/// </summary>
public interface IParameterExtractionVisitor : ILogicalExpressionVisitor
{
    List<string> Parameters { get; }
}