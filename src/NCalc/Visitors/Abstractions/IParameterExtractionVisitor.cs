namespace NCalc.Visitors;

public interface IParameterExtractionVisitor : ILogicalExpressionVisitor
{
    List<string> Parameters { get; }
}