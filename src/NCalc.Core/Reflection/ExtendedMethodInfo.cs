using System.Reflection;

namespace NCalc.Reflection;

public sealed class ExtendedMethodInfo<T> where T: class
{
    public required MethodInfo MethodInfo { get; init; }
    public required T[] PreparedArguments { get; init; }
    public int Score { get; init; }
}