using NCalc.Exceptions;

namespace NCalc.Helpers;

public static class BuiltInFunctionHelper
{
    public static async Task<object?> Evaluate(string functionName, ExpressionBase[] arguments, ExpressionContextBase context, Func<ExpressionBase, Task<object?>> evaluate)
    {
        var caseInsensitive = context.Options.HasFlag(ExpressionOptions.IgnoreCaseAtBuiltInFunctions);
        var comparison = caseInsensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

        if (functionName.Equals("Abs", comparison))
        {
            if (arguments.Length != 1)
                throw new NCalcEvaluationException("Abs() takes exactly 1 argument");

            return MathHelper.Abs(await evaluate(arguments[0]), context);
        }

        if (functionName.Equals("Acos", comparison))
        {
            if (arguments.Length != 1)
                throw new NCalcEvaluationException("Acos() takes exactly 1 argument");
            return MathHelper.Acos(await evaluate(arguments[0]), context);
        }
        if (functionName.Equals("Asin", comparison))
        {
            if (arguments.Length != 1)
                throw new NCalcEvaluationException("Asin() takes exactly 1 argument");
            return MathHelper.Asin(await evaluate(arguments[0]), context);
        }
        if (functionName.Equals("Atan", comparison))
        {
            if (arguments.Length != 1)
                throw new NCalcEvaluationException("Atan() takes exactly 1 argument");
            return MathHelper.Atan(await evaluate(arguments[0]), context);
        }
        if (functionName.Equals("Atan2", comparison))
        {
            if (arguments.Length != 2)
                throw new NCalcEvaluationException("Atan2() takes exactly 2 arguments");
            return MathHelper.Atan2(await evaluate(arguments[0]), await evaluate(arguments[1]), context);
        }
        if (functionName.Equals("Ceiling", comparison))
        {
            if (arguments.Length != 1)
                throw new NCalcEvaluationException("Ceiling() takes exactly 1 argument");
            return MathHelper.Ceiling(await evaluate(arguments[0]), context);
        }
        if (functionName.Equals("Cos", comparison))
        {
            if (arguments.Length != 1)
                throw new NCalcEvaluationException("Cos() takes exactly 1 argument");
            return MathHelper.Cos(await evaluate(arguments[0]), context);
        }
        if (functionName.Equals("Exp", comparison))
        {
            if (arguments.Length != 1)
                throw new NCalcEvaluationException("Exp() takes exactly 1 argument");
            return MathHelper.Exp(await evaluate(arguments[0]), context);
        }
        if (functionName.Equals("Floor", comparison))
        {
            if (arguments.Length != 1)
                throw new NCalcEvaluationException("Floor() takes exactly 1 argument");
            return MathHelper.Floor(await evaluate(arguments[0]), context);
        }
        if (functionName.Equals("IEEERemainder", comparison))
        {
            if (arguments.Length != 2)
                throw new NCalcEvaluationException("IEEERemainder() takes exactly 2 arguments");
            return MathHelper.IEEERemainder(await evaluate(arguments[0]), await evaluate(arguments[1]), context);
        }
        if (functionName.Equals("Ln", comparison))
        {
            if (arguments.Length != 1)
                throw new NCalcEvaluationException("Ln() takes exactly 1 argument");
            return MathHelper.Ln(await evaluate(arguments[0]), context);
        }
        if (functionName.Equals("Log", comparison))
        {
            if (arguments.Length != 2)
                throw new NCalcEvaluationException("Log() takes exactly 2 arguments");
            return MathHelper.Log(await evaluate(arguments[0]), await evaluate(arguments[1]), context);
        }
        if (functionName.Equals("Log10", comparison))
        {
            if (arguments.Length != 1)
                throw new NCalcEvaluationException("Log10() takes exactly 1 argument");
            return MathHelper.Log10(await evaluate(arguments[0]), context);
        }
        if (functionName.Equals("Pow", comparison))
        {
            if (arguments.Length != 2)
                throw new NCalcEvaluationException("Pow() takes exactly 2 arguments");
            return MathHelper.Pow(await evaluate(arguments[0]), await evaluate(arguments[1]), context);
        }
        if (functionName.Equals("Round", comparison))
        {
            if (arguments.Length != 2)
                throw new NCalcEvaluationException("Round() takes exactly 2 arguments");
            var rounding = context.Options.HasFlag(ExpressionOptions.RoundAwayFromZero)
                ? MidpointRounding.AwayFromZero
                : MidpointRounding.ToEven;
            return MathHelper.Round(await evaluate(arguments[0]), await evaluate(arguments[1]), rounding, context);
        }
        if (functionName.Equals("Sign", comparison))
        {
            if (arguments.Length != 1)
                throw new NCalcEvaluationException("Sign() takes exactly 1 argument");
            return MathHelper.Sign(await evaluate(arguments[0]), context);
        }
        if (functionName.Equals("Sin", comparison))
        {
            if (arguments.Length != 1)
                throw new NCalcEvaluationException("Sin() takes exactly 1 argument");
            return MathHelper.Sin(await evaluate(arguments[0]), context);
        }
        if (functionName.Equals("Sqrt", comparison))
        {
            if (arguments.Length != 1)
                throw new NCalcEvaluationException("Sqrt() takes exactly 1 argument");
            return MathHelper.Sqrt(await evaluate(arguments[0]), context);
        }
        if (functionName.Equals("Tan", comparison))
        {
            if (arguments.Length != 1)
                throw new NCalcEvaluationException("Tan() takes exactly 1 argument");
            return MathHelper.Tan(await evaluate(arguments[0]), context);
        }
        if (functionName.Equals("Truncate", comparison))
        {
            if (arguments.Length != 1)
                throw new NCalcEvaluationException("Truncate() takes exactly 1 argument");
            return MathHelper.Truncate(await evaluate(arguments[0]), context);
        }
        if (functionName.Equals("Max", comparison))
        {
            if (arguments.Length != 2)
                throw new NCalcEvaluationException("Max() takes exactly 2 arguments");
            return MathHelper.Max(await evaluate(arguments[0]), await evaluate(arguments[1]), context);
        }
        if (functionName.Equals("Min", comparison))
        {
            if (arguments.Length != 2)
                throw new NCalcEvaluationException("Min() takes exactly 2 arguments");
            return MathHelper.Min(await evaluate(arguments[0]), await evaluate(arguments[1]), context);
        }
        if (functionName.Equals("ifs", comparison))
        {
            if (arguments.Length < 3 || arguments.Length % 2 != 1)
                throw new NCalcEvaluationException(
                    "ifs() takes at least 3 arguments, or an odd number of arguments");
            foreach (var argument in arguments.Where((_, i) => i % 2 == 0))
            {
                var index = Array.IndexOf(arguments, argument);
                if (index == arguments.Length - 1)
                    return await evaluate(argument);

                var tf = Convert.ToBoolean(await evaluate(argument), context.CultureInfo);
                if (tf)
                    return await evaluate(arguments[index + 1]);
            }

            return null;
        }
        if (functionName.Equals("if", comparison))
        {
            if (arguments.Length != 3)
                throw new NCalcEvaluationException("if() takes exactly 3 arguments");

            var cond = Convert.ToBoolean(await evaluate(arguments[0]), context.CultureInfo);
            return cond ? await evaluate(arguments[1]) : await evaluate(arguments[2]);
        }
        if (functionName.Equals("in", comparison))
        {
            if (arguments.Length < 2)
                throw new NCalcEvaluationException("in() takes at least 2 arguments");
            var parameter = await evaluate(arguments[0]);
            var evaluation = false;
            for (var i = 1; i < arguments.Length; i++)
            {
                if (TypeHelper.CompareUsingMostPreciseType(parameter, await evaluate(arguments[i]), context) != 0) continue;
                evaluation = true;
                break;
            }

            return evaluation;
        }

        throw new NCalcFunctionNotFoundException(functionName);
    }
}
