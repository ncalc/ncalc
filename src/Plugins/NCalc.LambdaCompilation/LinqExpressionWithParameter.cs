using System.Linq.Expressions;

using LinqExpression = System.Linq.Expressions.Expression;

namespace NCalc.LambdaCompilation;

public readonly struct LinqExpressionWithParameter(LinqExpression expression, ParameterExpression? parameter)
{
    public LinqExpression Expression { get; init; } = expression;
    public ParameterExpression? Parameter { get; init; } = parameter;
}