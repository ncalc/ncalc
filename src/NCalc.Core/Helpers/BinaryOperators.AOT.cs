namespace NCalc.Helpers;

/// <summary>
/// Binary operators designed specifically for AOT execution
/// </summary>
static class BinaryOperatorsAOT
{
    // unchecked
    public static object AddFunc(object a, object b)
    {
        switch(a)
        {
            case byte aa when b is byte bb: return aa + bb;
            case sbyte aa when b is sbyte bb: return aa + bb;
            case short aa when b is short bb: return aa + bb;
            case ushort aa when b is ushort bb: return aa + bb;
            case int aa when b is int bb: return aa + bb;
            case uint aa when b is uint bb: return aa + bb;
            case long aa when b is long bb: return aa + bb;
            case ulong aa when b is ulong bb: return aa + bb;
            case float aa when b is float bb: return aa + bb;
            case double aa when b is double bb: return aa + bb;
            case decimal aa when b is decimal bb: return aa + bb;
            default: throw new InvalidOperationException(FormatError(a,"+", b));
        }        
    }

    public static object SubtractFunc(object a, object b)
    {
        switch (a)
        {
            case byte aa when b is byte bb: return aa - bb;
            case sbyte aa when b is sbyte bb: return aa - bb;
            case short aa when b is short bb: return aa - bb;
            case ushort aa when b is ushort bb: return aa - bb;
            case int aa when b is int bb: return aa - bb;
            case uint aa when b is uint bb: return aa - bb;
            case long aa when b is long bb: return aa - bb;
            case ulong aa when b is ulong bb: return aa - bb;
            case float aa when b is float bb: return aa - bb;
            case double aa when b is double bb: return aa - bb;
            case decimal aa when b is decimal bb: return aa - bb;
            default: throw new InvalidOperationException(FormatError(a,"-", b));
        }
    }

    public static object MultiplyFunc(object a, object b)
    {
        switch (a)
        {
            case byte aa when b is byte bb: return aa * bb;
            case sbyte aa when b is sbyte bb: return aa * bb;
            case short aa when b is short bb: return aa * bb;
            case ushort aa when b is ushort bb: return aa * bb;
            case int aa when b is int bb: return aa * bb;
            case uint aa when b is uint bb: return aa * bb;
            case long aa when b is long bb: return aa * bb;
            case ulong aa when b is ulong bb: return aa * bb;
            case float aa when b is float bb: return aa * bb;
            case double aa when b is double bb: return aa * bb;
            case decimal aa when b is decimal bb: return aa * bb;
            default: throw new InvalidOperationException(FormatError(a,"*", b));
        }
    }

    public static object DivideFunc(object a, object b)
    {
        switch (a)
        {
            case byte aa when b is byte bb: return aa / bb;
            case sbyte aa when b is sbyte bb: return aa / bb;
            case short aa when b is short bb: return aa / bb;
            case ushort aa when b is ushort bb: return aa / bb;
            case int aa when b is int bb: return aa / bb;
            case uint aa when b is uint bb: return aa / bb;
            case long aa when b is long bb: return aa / bb;
            case ulong aa when b is ulong bb: return aa / bb;
            case float aa when b is float bb: return aa / bb;
            case double aa when b is double bb: return aa / bb;
            case decimal aa when b is decimal bb: return aa / bb;
            default: throw new InvalidOperationException(FormatError(a,"/", b));
        }
    }

    public static object ModuloFunc(object a, object b)
    {
        switch (a)
        {
            case byte aa when b is byte bb: return aa % bb;
            case sbyte aa when b is sbyte bb: return aa % bb;
            case short aa when b is short bb: return aa % bb;
            case ushort aa when b is ushort bb: return aa % bb;
            case int aa when b is int bb: return aa % bb;
            case uint aa when b is uint bb: return aa % bb;
            case long aa when b is long bb: return aa % bb;
            case ulong aa when b is ulong bb: return aa % bb;
            case float aa when b is float bb: return aa % bb;
            case double aa when b is double bb: return aa % bb;
            case decimal aa when b is decimal bb: return aa % bb;
            default: throw new InvalidOperationException(FormatError(a, "%", b));
        }
    }

   

    // checked
    public static readonly Func<object, object, object> AddFuncChecked = (a, b) =>
    {
        switch (a)
        {
            case byte aa when b is byte bb: return CheckOverflow(checked(aa + bb));
            case sbyte aa when b is sbyte bb: return CheckOverflow(checked(aa + bb));
            case short aa when b is short bb: return CheckOverflow(checked(aa + bb));
            case ushort aa when b is ushort bb: return CheckOverflow(checked(aa + bb));
            case int aa when b is int bb: return CheckOverflow(checked(aa + bb));
            case uint aa when b is uint bb: return CheckOverflow(checked(aa + bb));
            case long aa when b is long bb: return CheckOverflow(checked(aa + bb));
            case ulong aa when b is ulong bb: return CheckOverflow(checked(aa + bb));
            case float aa when b is float bb: return CheckOverflow(checked(aa + bb));
            case double aa when b is double bb: return CheckOverflow(checked(aa + bb));
            case decimal aa when b is decimal bb: return CheckOverflow(checked(aa + bb));
            default: throw new InvalidOperationException(FormatError(a, "+", b));
        }
    };

    public static readonly Func<object, object, object> SubtractFuncChecked = (a, b) =>
    {
        switch (a)
        {
            case byte aa when b is byte bb: return CheckOverflow(checked(aa - bb));
            case sbyte aa when b is sbyte bb: return CheckOverflow(checked(aa - bb));
            case short aa when b is short bb: return CheckOverflow(checked(aa - bb));
            case ushort aa when b is ushort bb: return CheckOverflow(checked(aa - bb));
            case int aa when b is int bb: return CheckOverflow(checked(aa - bb));
            case uint aa when b is uint bb: return CheckOverflow(checked(aa - bb));
            case long aa when b is long bb: return CheckOverflow(checked(aa - bb));
            case ulong aa when b is ulong bb: return CheckOverflow(checked(aa - bb));
            case float aa when b is float bb: return CheckOverflow(checked(aa - bb));
            case double aa when b is double bb: return CheckOverflow(checked(aa - bb));
            case decimal aa when b is decimal bb: return CheckOverflow(checked(aa - bb));
            default: throw new InvalidOperationException(FormatError(a, "-", b));
        }
    };

    public static readonly Func<object, object, object> MultiplyFuncChecked = (a, b) =>
    {
        switch (a)
        {
            case byte aa when b is byte bb: return CheckOverflow(checked(aa * bb));
            case sbyte aa when b is sbyte bb: return CheckOverflow(checked(aa * bb));
            case short aa when b is short bb: return CheckOverflow(checked(aa * bb));
            case ushort aa when b is ushort bb: return CheckOverflow(checked(aa * bb));
            case int aa when b is int bb: return CheckOverflow(checked(aa * bb));
            case uint aa when b is uint bb: return CheckOverflow(checked(aa * bb));
            case long aa when b is long bb: return CheckOverflow(checked(aa * bb));
            case ulong aa when b is ulong bb: return CheckOverflow(checked(aa * bb));
            case float aa when b is float bb: return CheckOverflow(checked(aa * bb));
            case double aa when b is double bb: return CheckOverflow(checked(aa * bb));
            case decimal aa when b is decimal bb: return CheckOverflow(checked(aa * bb));
            default: throw new InvalidOperationException(FormatError(a, "*", b));
        }
    };

    public static readonly Func<object, object, object> DivideFuncChecked = (a, b) =>
    {
        switch (a)
        {
            case byte aa when b is byte bb: return CheckOverflow(checked(aa / bb));
            case sbyte aa when b is sbyte bb: return CheckOverflow(checked(aa / bb));
            case short aa when b is short bb: return CheckOverflow(checked(aa / bb));
            case ushort aa when b is ushort bb: return CheckOverflow(checked(aa / bb));
            case int aa when b is int bb: return CheckOverflow(checked(aa / bb));
            case uint aa when b is uint bb: return CheckOverflow(checked(aa / bb));
            case long aa when b is long bb: return CheckOverflow(checked(aa / bb));
            case ulong aa when b is ulong bb: return CheckOverflow(checked(aa / bb));
            case float aa when b is float bb: return CheckOverflow(checked(aa / bb));
            case double aa when b is double bb: return CheckOverflow(checked(aa / bb));
            case decimal aa when b is decimal bb: return CheckOverflow(checked(aa / bb));
            default: throw new InvalidOperationException(FormatError(a, "/", b));
        }
    };

    private static string FormatError(object a, string op, object b)
    {
        var aType = a?.GetType()?.Name;
        var bType = b?.GetType()?.Name;

        return $"{a} {op} {b}: {aType}, {bType}";
    }

    private static T CheckOverflow<T>(T value)
    {
        switch (value)
        {
            case double doubleVal when double.IsInfinity(doubleVal):
            case float floatValue when float.IsInfinity(floatValue):
                throw new OverflowException("Arithmetic operation resulted in an overflow");
        }

        return value;
    }
}