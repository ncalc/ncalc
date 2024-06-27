#nullable disable

using System.Reflection;
using LinqExpression = System.Linq.Expressions.Expression;

namespace NCalc.Reflection;

public sealed class ExtendedMethodInfo
{
    public MethodInfo MethodInfo { get; init; }
    public LinqExpression[] PreparedArguments { get; init; }
    public int Score { get; init; }
}