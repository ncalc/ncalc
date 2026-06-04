using System.Diagnostics.CodeAnalysis;

namespace NCalc.Helpers;

#pragma warning disable IL2026 // Using dynamic types might cause types or members to be removed by trimmer.
#pragma warning disable IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.

/// <summary>
/// Binary operators designed for JIT execution
/// </summary>
#if NET8_0_OR_GREATER
[RequiresUnreferencedCode("dynamic code")]
[RequiresDynamicCode("dynamic code")]
#endif
static class BinaryOperatorsJIT
{
    // unchecked
    public static readonly Func<dynamic, dynamic, object> AddFunc = (a, b) => a + b;
    public static readonly Func<dynamic, dynamic, object> SubtractFunc = (a, b) => a - b;
    public static readonly Func<dynamic, dynamic, object> MultiplyFunc = (a, b) => a * b;
    public static readonly Func<dynamic, dynamic, object> DivideFunc = (a, b) => a / b;
    public static readonly Func<dynamic, dynamic, object> ModuloFunc = (a, b) => a % b;

    // checked
    public static readonly Func<dynamic, dynamic, object> AddFuncChecked = (a, b) =>
    {
        var res = checked(a + b);
        CheckOverflow(res);

        return res;
    };

    public static readonly Func<dynamic, dynamic, object> SubtractFuncChecked = (a, b) =>
    {
        var res = checked(a - b);
        CheckOverflow(res);

        return res;
    };

    public static readonly Func<dynamic, dynamic, object> MultiplyFuncChecked = (a, b) =>
    {
        var res = checked(a * b);
        CheckOverflow(res);

        return res;
    };

    public static readonly Func<dynamic, dynamic, object> DivideFuncChecked = (a, b) =>
    {
        var res = checked(a / b);
        CheckOverflow(res);

        return res;
    };

    private static void CheckOverflow(dynamic value)
    {
        switch (value)
        {
            case double doubleVal when double.IsInfinity(doubleVal):
            case float floatValue when float.IsInfinity(floatValue):
                throw new OverflowException("Arithmetic operation resulted in an overflow");
        }
    }
}

#pragma warning restore IL2026 // Using dynamic types might cause types or members to be removed by trimmer.
#pragma warning restore IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.