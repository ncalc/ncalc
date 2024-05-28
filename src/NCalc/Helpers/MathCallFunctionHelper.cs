#nullable disable

using System.Collections.Frozen;
using System.Reflection;

namespace NCalc.Helpers;

internal static class MathCallFunctionHelper
{
    public struct MathCallFunction
    {
        public MethodInfo MathMethodInfo;
        public int ArgumentCount;
    }

    private static MathCallFunction GetMathCallFunctionHelper(string method, int argCount) => new()
    {
        MathMethodInfo = typeof(Math).GetMethod(method, Enumerable.Repeat(typeof(double), argCount).ToArray()),
        ArgumentCount = argCount
    };

    public static readonly FrozenDictionary<string, MathCallFunction> Functions =
        new Dictionary<string, MathCallFunction>
        {
            { "ABS", GetMathCallFunctionHelper(nameof(Math.Abs), 1) },
            { "ACOS", GetMathCallFunctionHelper(nameof(Math.Acos), 1) },
            { "ASIN", GetMathCallFunctionHelper(nameof(Math.Asin), 1) },
            { "ATAN", GetMathCallFunctionHelper(nameof(Math.Atan), 1) },
            { "ATAN2", GetMathCallFunctionHelper(nameof(Math.Atan2), 2) },
            { "CEILING", GetMathCallFunctionHelper(nameof(Math.Ceiling), 1) },
            { "COS", GetMathCallFunctionHelper(nameof(Math.Cos), 1) },
            { "COSH", GetMathCallFunctionHelper(nameof(Math.Cosh), 1) },
            { "EXP", GetMathCallFunctionHelper(nameof(Math.Exp), 1) },
            { "FLOOR", GetMathCallFunctionHelper(nameof(Math.Floor), 1) },
            { "IEEEREMAINDER", GetMathCallFunctionHelper(nameof(Math.IEEERemainder), 2) },
            { "LOG", GetMathCallFunctionHelper(nameof(Math.Log), 2) },
            { "LOG10", GetMathCallFunctionHelper(nameof(Math.Log10), 1) },
            { "SIGN", GetMathCallFunctionHelper(nameof(Math.Sign), 1) },
            { "SIN", GetMathCallFunctionHelper(nameof(Math.Sin), 1) },
            { "SINH", GetMathCallFunctionHelper(nameof(Math.Sinh), 1) },
            { "SQRT", GetMathCallFunctionHelper(nameof(Math.Sqrt), 1) },
            { "TAN", GetMathCallFunctionHelper(nameof(Math.Tan), 1) },
            { "TANH", GetMathCallFunctionHelper(nameof(Math.Tanh), 1) },
            { "TRUNCATE", GetMathCallFunctionHelper(nameof(Math.Truncate), 1) },

            // Exceptional handling
            {
                "ROUND", new MathCallFunction
                {
                    MathMethodInfo = typeof(Math).GetMethod(nameof(Math.Round),
                        [typeof(double), typeof(int), typeof(MidpointRounding)]),
                    ArgumentCount = 2
                }
            }
        }.ToFrozenDictionary();
}