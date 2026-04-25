using System.Collections.Frozen;

namespace NCalc.LambdaCompilation.Reflection;

public static class MathFunctionHelper
{
    private static List<MathMethodInfo> GetMathMethodInfo(string method, int argCount, bool supportDecimal = false)
    {
        var methodInfo = new List<MathMethodInfo>();

        if (supportDecimal)
        {
            methodInfo.Add(new MathMethodInfo
            {
                MethodInfo = typeof(Math).GetMethod(method, Enumerable.Repeat(typeof(decimal), argCount).ToArray())!,
                ArgumentCount = argCount,
                DecimalSupport = true
            });
        }

        methodInfo.Add(new MathMethodInfo
        {
            MethodInfo = typeof(Math).GetMethod(method, Enumerable.Repeat(typeof(double), argCount).ToArray())!,
            ArgumentCount = argCount,
            DecimalSupport = false
        });

        return methodInfo;
    }

    public static readonly FrozenDictionary<string, List<MathMethodInfo>> Functions =
        new Dictionary<string, List<MathMethodInfo>>
        {
            { "Abs", GetMathMethodInfo(nameof(Math.Abs), 1, true) },
            { "Acos", GetMathMethodInfo(nameof(Math.Acos), 1) },
            { "Asin", GetMathMethodInfo(nameof(Math.Asin), 1) },
            { "Atan", GetMathMethodInfo(nameof(Math.Atan), 1) },
            { "Atan2", GetMathMethodInfo(nameof(Math.Atan2), 2) },
            { "Ceiling", GetMathMethodInfo(nameof(Math.Ceiling), 1, true) },
            { "Cos", GetMathMethodInfo(nameof(Math.Cos), 1) },
            { "Cosh", GetMathMethodInfo(nameof(Math.Cosh), 1) },
            { "Exp", GetMathMethodInfo(nameof(Math.Exp), 1) },
            { "Floor", GetMathMethodInfo(nameof(Math.Floor), 1, true) },
            { "IEEERemainder", GetMathMethodInfo(nameof(Math.IEEERemainder), 2) },
            { "Log", GetMathMethodInfo(nameof(Math.Log), 2) },
            { "Log10", GetMathMethodInfo(nameof(Math.Log10), 1) },
            { "Sign", GetMathMethodInfo(nameof(Math.Sign), 1, true) },
            { "Sin", GetMathMethodInfo(nameof(Math.Sin), 1) },
            { "Sinh", GetMathMethodInfo(nameof(Math.Sinh), 1) },
            { "Sqrt", GetMathMethodInfo(nameof(Math.Sqrt), 1) },
            { "Tan", GetMathMethodInfo(nameof(Math.Tan), 1) },
            { "Tanh", GetMathMethodInfo(nameof(Math.Tanh), 1) },
            { "Truncate", GetMathMethodInfo(nameof(Math.Truncate), 1, true) },

            // Exceptional handling
            {
                "Round",
                [
                    new MathMethodInfo
                    {
                        MethodInfo = typeof(Math).GetMethod(nameof(Math.Round),
                            [typeof(double), typeof(int), typeof(MidpointRounding)])!,
                        ArgumentCount = 2,
                        DecimalSupport = false
                    }
                ]
            }
        }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
}