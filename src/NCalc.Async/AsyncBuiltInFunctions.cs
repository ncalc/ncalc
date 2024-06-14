using System.Collections.Frozen;
using NCalc.Exceptions;
using NCalc.Helpers;


namespace NCalc;

public static class AsyncExpressionBuiltInFunctions
{
    public static readonly FrozenDictionary<string, AsyncExpressionFunction> Values;

    static AsyncExpressionBuiltInFunctions()
    {
        var builtInFunctions = new Dictionary<string, AsyncExpressionFunction>();
        builtInFunctions.Add("Abs", async (arguments, context) =>
        {
            if (arguments.Length != 1) throw new NCalcEvaluationException("Abs() takes exactly 1 argument");
            return MathHelper.Abs(await arguments[0].EvaluateAsync(), context);
        });

        builtInFunctions.Add("Acos", async (arguments, context) =>
        {
            if (arguments.Length != 1) throw new NCalcEvaluationException("Acos() takes exactly 1 argument");
            return MathHelper.Acos(await arguments[0].EvaluateAsync(), context);
        });

        builtInFunctions.Add("Asin", async (arguments, context) =>
        {
            if (arguments.Length != 1) throw new NCalcEvaluationException("Asin() takes exactly 1 argument");
            return MathHelper.Asin(await arguments[0].EvaluateAsync(), context);
        });

        builtInFunctions.Add("Atan", async (arguments, context) =>
        {
            if (arguments.Length != 1) throw new NCalcEvaluationException("Atan() takes exactly 1 argument");
            return MathHelper.Atan(await arguments[0].EvaluateAsync(), context);
        });

        builtInFunctions.Add("Atan2", async (arguments, context) =>
        {
            if (arguments.Length != 2) throw new NCalcEvaluationException("Atan2() takes exactly 2 arguments");
            return MathHelper.Atan2(await arguments[0].EvaluateAsync(), await arguments[1].EvaluateAsync(), context);
        });

        builtInFunctions.Add("Ceiling", async (arguments, context) =>
        {
            if (arguments.Length != 1) throw new NCalcEvaluationException("Ceiling() takes exactly 1 argument");
            return MathHelper.Ceiling(await arguments[0].EvaluateAsync(), context);
        });

        builtInFunctions.Add("Cos", async (arguments, context) =>
        {
            if (arguments.Length != 1)
                throw new NCalcEvaluationException("Cos() takes exactly 1 argument");
            return MathHelper.Cos(await arguments[0].EvaluateAsync(), context);
        });

        builtInFunctions.Add("Exp", async (arguments, context) =>
        {
            if (arguments.Length != 1) throw new NCalcEvaluationException("Exp() takes exactly 1 argument");
            return MathHelper.Exp(await arguments[0].EvaluateAsync(), context);
        });

        builtInFunctions.Add("Floor", async (arguments, context) =>
        {
            if (arguments.Length != 1) throw new NCalcEvaluationException("Floor() takes exactly 1 argument");
            return MathHelper.Floor(await arguments[0].EvaluateAsync(), context);
        });

        builtInFunctions.Add("IEEERemainder", async (arguments, context) =>
        {
            if (arguments.Length != 2) throw new NCalcEvaluationException("IEEERemainder() takes exactly 2 arguments");
            return MathHelper.IEEERemainder(await arguments[0].EvaluateAsync(), await arguments[1].EvaluateAsync(), context);
        });

        builtInFunctions.Add("Ln", async (arguments, context) =>
        {
            if (arguments.Length != 1) throw new NCalcEvaluationException("Ln() takes exactly 1 argument");
            return MathHelper.Ln(await arguments[0].EvaluateAsync(), context);
        });

        builtInFunctions.Add("Log", async (arguments, context) =>
        {
            if (arguments.Length != 2) throw new NCalcEvaluationException("Log() takes exactly 2 arguments");
            return MathHelper.Log(await arguments[0].EvaluateAsync(), await arguments[1].EvaluateAsync(), context);
        });

        builtInFunctions.Add("Log10", async (arguments, context) =>
        {
            if (arguments.Length != 1) throw new NCalcEvaluationException("Log10() takes exactly 1 argument");
            return MathHelper.Log10(await arguments[0].EvaluateAsync(), context);
        });

        builtInFunctions.Add("Pow", async (arguments, context) =>
        {
            if (arguments.Length != 2) throw new NCalcEvaluationException("Pow() takes exactly 2 arguments");
            return MathHelper.Pow(await arguments[0].EvaluateAsync(), await arguments[1].EvaluateAsync(), context);
        });

        builtInFunctions.Add("Round", async (arguments, context) =>
        {
            if (arguments.Length != 2) throw new NCalcEvaluationException("Round() takes exactly 2 arguments");
            var rounding = context.Options.HasFlag(ExpressionOptions.RoundAwayFromZero)
                ? MidpointRounding.AwayFromZero
                : MidpointRounding.ToEven;
            return MathHelper.Round(await arguments[0].EvaluateAsync(), await arguments[1].EvaluateAsync(), rounding,
                context);
        });

        builtInFunctions.Add("Sign", async (arguments, context) =>
        {
            if (arguments.Length != 1) throw new NCalcEvaluationException("Sign() takes exactly 1 argument");
            return MathHelper.Sign(await arguments[0].EvaluateAsync(), context);
        });

        builtInFunctions.Add("Sin", async (arguments, context) =>
        {
            if (arguments.Length != 1) throw new NCalcEvaluationException("Sin() takes exactly 1 argument");
            return MathHelper.Sin(await arguments[0].EvaluateAsync(), context);
        });

        builtInFunctions.Add("Sqrt", async (arguments, context) =>
        {
            if (arguments.Length != 1) throw new NCalcEvaluationException("Sqrt() takes exactly 1 argument");
            return MathHelper.Sqrt(await arguments[0].EvaluateAsync(), context);
        });

        builtInFunctions.Add("Tan", async (arguments, context) =>
        {
            if (arguments.Length != 1) throw new NCalcEvaluationException("Tan() takes exactly 1 argument");
            return MathHelper.Tan(await arguments[0].EvaluateAsync(), context);
        });

        builtInFunctions.Add("Truncate", async (arguments, context) =>
        {
            if (arguments.Length != 1) throw new NCalcEvaluationException("Truncate() takes exactly 1 argument");
            return MathHelper.Truncate(await arguments[0].EvaluateAsync(), context);
        });

        builtInFunctions.Add("Max", async (arguments, context) =>
        {
            if (arguments.Length != 2) throw new NCalcEvaluationException("Max() takes exactly 2 arguments");
            return MathHelper.Max(await arguments[0].EvaluateAsync(), await arguments[1].EvaluateAsync(), context);
        });

        builtInFunctions.Add("Min", async (arguments, context) =>
        {
            if (arguments.Length != 2) throw new NCalcEvaluationException("Min() takes exactly 2 arguments");
            return MathHelper.Min(await arguments[0].EvaluateAsync(), await arguments[1].EvaluateAsync(), context);
        });

        builtInFunctions.Add("ifs", async (arguments, context) =>
        {
            if (arguments.Length < 3 || arguments.Length % 2 != 1)
                throw new NCalcEvaluationException("ifs() takes at least 3 arguments, or an odd number of arguments");
            foreach (var argument in arguments.Where((_, i) => i % 2 == 0))
            {
                var index = Array.IndexOf(arguments, argument);
                var tf = Convert.ToBoolean(await argument.EvaluateAsync(), context.CultureInfo);
                if (index == arguments.Length - 1) return await argument.EvaluateAsync();
                if (tf) return await arguments[index + 1].EvaluateAsync();
            }

            return null;
        });

        builtInFunctions.Add("if", async (arguments, context) =>
        {
            if (arguments.Length != 3) throw new NCalcEvaluationException("if() takes exactly 3 arguments");
            var cond = Convert.ToBoolean(await arguments[0].EvaluateAsync(), context.CultureInfo);
            return cond ? arguments[1].EvaluateAsync() : await arguments[2].EvaluateAsync();
        });

        builtInFunctions.Add("in", async (arguments, context) =>
        {
            if (arguments.Length < 2) throw new NCalcEvaluationException("in() takes at least 2 arguments");
            var parameter = await arguments[0].EvaluateAsync();
            var evaluation = false;
            for (var i = 1; i < arguments.Length; i++)
            {
                if (TypeHelper.CompareUsingMostPreciseType(parameter, arguments[i].EvaluateAsync(), new(
                        context.CultureInfo,
                        context.Options.HasFlag(ExpressionOptions.CaseInsensitiveStringComparer),
                        context.Options.HasFlag(ExpressionOptions.OrdinalStringComparer))) == 0)
                {
                    evaluation = true;
                    break;
                }
            }

            return evaluation;
        });
        Values = builtInFunctions.ToFrozenDictionary();
    }
}