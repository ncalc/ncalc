using System.Collections.ObjectModel;
using NCalc.Exceptions;
using NCalc.Handlers;

namespace NCalc.Helpers;

public static class BuiltInFunctionHelper
{
    private static readonly IReadOnlyList<string> _builtInFunctionNames =
        new ReadOnlyCollection<string>([
            "abs",
            "acos",
            "asin",
            "atan",
            "atan2",
            "ceiling",
            "cos",
            "exp",
            "floor",
            "ieeeremainder",
            "ln",
            "log",
            "log10",
            "pow",
            "round",
            "sign",
            "sqrt",
            "tan",
            "truncate",
            "max",
            "min",
            "if",
            "in",
            "ifs"
        ]);
    public static IReadOnlyList<string> GetBuiltInFunctionNames() => _builtInFunctionNames;

    public static object? Evaluate(
        string functionName,
        FunctionData functionData)
    {
        var context = functionData.Context;
        var caseInsensitive = context.Options.HasFlag(ExpressionOptions.IgnoreCaseAtBuiltInFunctions);
        var comparison = caseInsensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

        if (functionName.Equals("if", comparison))
        {
            if (functionData.Count != 3)
                throw new NCalcEvaluationException("if() takes exactly 3 arguments");

            var cond = Convert.ToBoolean(functionData.Evaluate(0), context.CultureInfo);
            return cond ? functionData.Evaluate(1) : functionData.Evaluate(2);
        }

        if (functionName.Equals("in", comparison))
        {
            if (functionData.Count < 2)
                throw new NCalcEvaluationException("in() takes at least 2 arguments");

            var parameter = functionData.Evaluate(0);
            for (var i = 1; i < functionData.Count; i++)
            {
                if (TypeHelper.CompareUsingMostPreciseType(parameter, functionData.Evaluate(i), context) == ComparisonResult.Equal)
                    return true;
            }

            return false;
        }

        if (functionName.Equals("Abs", comparison))
        {
            if (functionData.Count != 1)
                throw new NCalcEvaluationException("Abs() takes exactly 1 argument");
            return MathHelper.Abs(functionData.Evaluate(0), context);
        }

        if (functionName.Equals("Acos", comparison))
        {
            if (functionData.Count != 1)
                throw new NCalcEvaluationException("Acos() takes exactly 1 argument");
            return MathHelper.Acos(functionData.Evaluate(0), context);
        }

        if (functionName.Equals("Asin", comparison))
        {
            if (functionData.Count != 1)
                throw new NCalcEvaluationException("Asin() takes exactly 1 argument");
            return MathHelper.Asin(functionData.Evaluate(0), context);
        }

        if (functionName.Equals("Atan", comparison))
        {
            if (functionData.Count != 1)
                throw new NCalcEvaluationException("Atan() takes exactly 1 argument");
            return MathHelper.Atan(functionData.Evaluate(0), context);
        }

        if (functionName.Equals("Atan2", comparison))
        {
            if (functionData.Count != 2)
                throw new NCalcEvaluationException("Atan2() takes exactly 2 arguments");
            return MathHelper.Atan2(functionData.Evaluate(0), functionData.Evaluate(1), context);
        }

        if (functionName.Equals("Ceiling", comparison))
        {
            if (functionData.Count != 1)
                throw new NCalcEvaluationException("Ceiling() takes exactly 1 argument");
            return MathHelper.Ceiling(functionData.Evaluate(0), context);
        }

        if (functionName.Equals("Cos", comparison))
        {
            if (functionData.Count != 1)
                throw new NCalcEvaluationException("Cos() takes exactly 1 argument");
            return MathHelper.Cos(functionData.Evaluate(0), context);
        }

        if (functionName.Equals("Exp", comparison))
        {
            if (functionData.Count != 1)
                throw new NCalcEvaluationException("Exp() takes exactly 1 argument");
            return MathHelper.Exp(functionData.Evaluate(0), context);
        }

        if (functionName.Equals("Floor", comparison))
        {
            if (functionData.Count != 1)
                throw new NCalcEvaluationException("Floor() takes exactly 1 argument");
            return MathHelper.Floor(functionData.Evaluate(0), context);
        }

        if (functionName.Equals("IEEERemainder", comparison))
        {
            if (functionData.Count != 2)
                throw new NCalcEvaluationException("IEEERemainder() takes exactly 2 arguments");
            return MathHelper.IEEERemainder(functionData.Evaluate(0), functionData.Evaluate(1), context);
        }

        if (functionName.Equals("Ln", comparison))
        {
            if (functionData.Count != 1)
                throw new NCalcEvaluationException("Ln() takes exactly 1 argument");
            return MathHelper.Ln(functionData.Evaluate(0), context);
        }

        if (functionName.Equals("Log", comparison))
        {
            if (functionData.Count != 2)
                throw new NCalcEvaluationException("Log() takes exactly 2 arguments");
            return MathHelper.Log(functionData.Evaluate(0), functionData.Evaluate(1), context);
        }

        if (functionName.Equals("Log10", comparison))
        {
            if (functionData.Count != 1)
                throw new NCalcEvaluationException("Log10() takes exactly 1 argument");
            return MathHelper.Log10(functionData.Evaluate(0), context);
        }

        if (functionName.Equals("Pow", comparison))
        {
            if (functionData.Count != 2)
                throw new NCalcEvaluationException("Pow() takes exactly 2 arguments");
            return MathHelper.Pow(functionData.Evaluate(0), functionData.Evaluate(1), context);
        }

        if (functionName.Equals("Round", comparison))
        {
            if (functionData.Count != 2)
                throw new NCalcEvaluationException("Round() takes exactly 2 arguments");
            var rounding = context.Options.HasFlag(ExpressionOptions.RoundAwayFromZero)
                ? MidpointRounding.AwayFromZero
                : MidpointRounding.ToEven;
            return MathHelper.Round(functionData.Evaluate(0), functionData.Evaluate(1), rounding, context);
        }

        if (functionName.Equals("Sign", comparison))
        {
            if (functionData.Count != 1)
                throw new NCalcEvaluationException("Sign() takes exactly 1 argument");
            return MathHelper.Sign(functionData.Evaluate(0), context);
        }

        if (functionName.Equals("Sin", comparison))
        {
            if (functionData.Count != 1)
                throw new NCalcEvaluationException("Sin() takes exactly 1 argument");
            return MathHelper.Sin(functionData.Evaluate(0), context);
        }

        if (functionName.Equals("Sqrt", comparison))
        {
            if (functionData.Count != 1)
                throw new NCalcEvaluationException("Sqrt() takes exactly 1 argument");
            return MathHelper.Sqrt(functionData.Evaluate(0), context);
        }

        if (functionName.Equals("Tan", comparison))
        {
            if (functionData.Count != 1)
                throw new NCalcEvaluationException("Tan() takes exactly 1 argument");
            return MathHelper.Tan(functionData.Evaluate(0), context);
        }

        if (functionName.Equals("Truncate", comparison))
        {
            if (functionData.Count != 1)
                throw new NCalcEvaluationException("Truncate() takes exactly 1 argument");
            return MathHelper.Truncate(functionData.Evaluate(0), context);
        }

        if (functionName.Equals("Max", comparison))
        {
            if (functionData.Count != 2)
                throw new NCalcEvaluationException("Max() takes exactly 2 arguments");
            return MathHelper.Max(functionData.Evaluate(0), functionData.Evaluate(1), context);
        }

        if (functionName.Equals("Min", comparison))
        {
            if (functionData.Count != 2)
                throw new NCalcEvaluationException("Min() takes exactly 2 arguments");
            return MathHelper.Min(functionData.Evaluate(0), functionData.Evaluate(1), context);
        }

        if (functionName.Equals("ifs", comparison))
        {
            if (functionData.Count < 3 || functionData.Count % 2 != 1)
                throw new NCalcEvaluationException("ifs() takes at least 3 arguments, or an odd number of arguments");

            for (var i = 0; i < functionData.Count; i += 2)
            {
                if (i == functionData.Count - 1)
                    return functionData.Evaluate(i);

                var tf = Convert.ToBoolean(functionData.Evaluate(i), context.CultureInfo);
                if (tf)
                    return functionData.Evaluate(i + 1);
            }

            return null;
        }
        
        throw new NCalcFunctionNotFoundException(functionName);
    }

    public static async ValueTask<object?> EvaluateAsync(
        string functionName,
        FunctionData functionData)
    {
        var context = functionData.Context;
        var caseInsensitive = context.Options.HasFlag(ExpressionOptions.IgnoreCaseAtBuiltInFunctions);
        var comparison = caseInsensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

        if (functionName.Equals("if", comparison))
        {
            if (functionData.Count != 3)
                throw new NCalcEvaluationException("if() takes exactly 3 arguments");

            var cond = Convert.ToBoolean(await functionData.EvaluateAsync(0), context.CultureInfo);
            return cond ? await functionData.EvaluateAsync(1) : await functionData.EvaluateAsync(2);
        }

        if (functionName.Equals("in", comparison))
        {
            if (functionData.Count < 2)
                throw new NCalcEvaluationException("in() takes at least 2 arguments");

            var parameter = await functionData.EvaluateAsync(0);
            for (var i = 1; i < functionData.Count; i++)
            {
                if (TypeHelper.CompareUsingMostPreciseType(parameter, await functionData.EvaluateAsync(i), context) == ComparisonResult.Equal)
                    return true;
            }

            return false;
        }

        if (functionName.Equals("Abs", comparison))
        {
            if (functionData.Count != 1)
                throw new NCalcEvaluationException("Abs() takes exactly 1 argument");
            return MathHelper.Abs(await functionData.EvaluateAsync(0), context);
        }

        if (functionName.Equals("Acos", comparison))
        {
            if (functionData.Count != 1)
                throw new NCalcEvaluationException("Acos() takes exactly 1 argument");
            return MathHelper.Acos(await functionData.EvaluateAsync(0), context);
        }

        if (functionName.Equals("Asin", comparison))
        {
            if (functionData.Count != 1)
                throw new NCalcEvaluationException("Asin() takes exactly 1 argument");
            return MathHelper.Asin(await functionData.EvaluateAsync(0), context);
        }

        if (functionName.Equals("Atan", comparison))
        {
            if (functionData.Count != 1)
                throw new NCalcEvaluationException("Atan() takes exactly 1 argument");
            return MathHelper.Atan(await functionData.EvaluateAsync(0), context);
        }

        if (functionName.Equals("Atan2", comparison))
        {
            if (functionData.Count != 2)
                throw new NCalcEvaluationException("Atan2() takes exactly 2 arguments");
            return MathHelper.Atan2(await functionData.EvaluateAsync(0), await functionData.EvaluateAsync(1), context);
        }

        if (functionName.Equals("Ceiling", comparison))
        {
            if (functionData.Count != 1)
                throw new NCalcEvaluationException("Ceiling() takes exactly 1 argument");
            return MathHelper.Ceiling(await functionData.EvaluateAsync(0), context);
        }

        if (functionName.Equals("Cos", comparison))
        {
            if (functionData.Count != 1)
                throw new NCalcEvaluationException("Cos() takes exactly 1 argument");
            return MathHelper.Cos(await functionData.EvaluateAsync(0), context);
        }

        if (functionName.Equals("Exp", comparison))
        {
            if (functionData.Count != 1)
                throw new NCalcEvaluationException("Exp() takes exactly 1 argument");
            return MathHelper.Exp(await functionData.EvaluateAsync(0), context);
        }

        if (functionName.Equals("Floor", comparison))
        {
            if (functionData.Count != 1)
                throw new NCalcEvaluationException("Floor() takes exactly 1 argument");
            return MathHelper.Floor(await functionData.EvaluateAsync(0), context);
        }

        if (functionName.Equals("IEEERemainder", comparison))
        {
            if (functionData.Count != 2)
                throw new NCalcEvaluationException("IEEERemainder() takes exactly 2 arguments");
            return MathHelper.IEEERemainder(await functionData.EvaluateAsync(0), await functionData.EvaluateAsync(1), context);
        }

        if (functionName.Equals("Ln", comparison))
        {
            if (functionData.Count != 1)
                throw new NCalcEvaluationException("Ln() takes exactly 1 argument");
            return MathHelper.Ln(await functionData.EvaluateAsync(0), context);
        }

        if (functionName.Equals("Log", comparison))
        {
            if (functionData.Count != 2)
                throw new NCalcEvaluationException("Log() takes exactly 2 arguments");
            return MathHelper.Log(await functionData.EvaluateAsync(0), await functionData.EvaluateAsync(1), context);
        }

        if (functionName.Equals("Log10", comparison))
        {
            if (functionData.Count != 1)
                throw new NCalcEvaluationException("Log10() takes exactly 1 argument");
            return MathHelper.Log10(await functionData.EvaluateAsync(0), context);
        }

        if (functionName.Equals("Pow", comparison))
        {
            if (functionData.Count != 2)
                throw new NCalcEvaluationException("Pow() takes exactly 2 arguments");
            return MathHelper.Pow(await functionData.EvaluateAsync(0), await functionData.EvaluateAsync(1), context);
        }

        if (functionName.Equals("Round", comparison))
        {
            if (functionData.Count != 2)
                throw new NCalcEvaluationException("Round() takes exactly 2 arguments");
            var rounding = context.Options.HasFlag(ExpressionOptions.RoundAwayFromZero)
                ? MidpointRounding.AwayFromZero
                : MidpointRounding.ToEven;
            return MathHelper.Round(await functionData.EvaluateAsync(0), await functionData.EvaluateAsync(1), rounding, context);
        }

        if (functionName.Equals("Sign", comparison))
        {
            if (functionData.Count != 1)
                throw new NCalcEvaluationException("Sign() takes exactly 1 argument");
            return MathHelper.Sign(await functionData.EvaluateAsync(0), context);
        }

        if (functionName.Equals("Sin", comparison))
        {
            if (functionData.Count != 1)
                throw new NCalcEvaluationException("Sin() takes exactly 1 argument");
            return MathHelper.Sin(await functionData.EvaluateAsync(0), context);
        }

        if (functionName.Equals("Sqrt", comparison))
        {
            if (functionData.Count != 1)
                throw new NCalcEvaluationException("Sqrt() takes exactly 1 argument");
            return MathHelper.Sqrt(await functionData.EvaluateAsync(0), context);
        }

        if (functionName.Equals("Tan", comparison))
        {
            if (functionData.Count != 1)
                throw new NCalcEvaluationException("Tan() takes exactly 1 argument");
            return MathHelper.Tan(await functionData.EvaluateAsync(0), context);
        }

        if (functionName.Equals("Truncate", comparison))
        {
            if (functionData.Count != 1)
                throw new NCalcEvaluationException("Truncate() takes exactly 1 argument");
            return MathHelper.Truncate(await functionData.EvaluateAsync(0), context);
        }

        if (functionName.Equals("Max", comparison))
        {
            if (functionData.Count != 2)
                throw new NCalcEvaluationException("Max() takes exactly 2 arguments");
            return MathHelper.Max(await functionData.EvaluateAsync(0), await functionData.EvaluateAsync(1), context);
        }

        if (functionName.Equals("Min", comparison))
        {
            if (functionData.Count != 2)
                throw new NCalcEvaluationException("Min() takes exactly 2 arguments");
            return MathHelper.Min(await functionData.EvaluateAsync(0), await functionData.EvaluateAsync(1), context);
        }

        if (functionName.Equals("ifs", comparison))
        {
            if (functionData.Count < 3 || functionData.Count % 2 != 1)
                throw new NCalcEvaluationException("ifs() takes at least 3 arguments, or an odd number of arguments");

            for (var i = 0; i < functionData.Count; i += 2)
            {
                if (i == functionData.Count - 1)
                    return await functionData.EvaluateAsync(i);

                var tf = Convert.ToBoolean(await functionData.EvaluateAsync(i), context.CultureInfo);
                if (tf)
                    return await functionData.EvaluateAsync(i + 1);
            }

            return null;
        }

        throw new NCalcFunctionNotFoundException(functionName);
    }
}
