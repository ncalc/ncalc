using System.Collections.Frozen;
using NCalc.Exceptions;
using NCalc.Helpers;

namespace NCalc;

public static class ExpressionBuiltInFunctions
{
    public static readonly FrozenDictionary<string, ExpressionFunction> Values;

    static ExpressionBuiltInFunctions()
    {
        var builtInFunctions = new Dictionary<string, ExpressionFunction>();
        builtInFunctions.Add("Abs", (arguments, context) =>
        {
            if (arguments.Length != 1) throw new NCalcEvaluationException("Abs() takes exactly 1 argument");
            return MathHelper.Abs(arguments[0].Evaluate(), context);
        });

        builtInFunctions.Add("Acos", (arguments, context) =>
        {
            if (arguments.Length != 1) throw new NCalcEvaluationException("Acos() takes exactly 1 argument");
            return MathHelper.Acos(arguments[0].Evaluate(), context);
        });

        builtInFunctions.Add("Asin", (arguments, context) =>
        {
            if (arguments.Length != 1) throw new NCalcEvaluationException("Asin() takes exactly 1 argument");
            return MathHelper.Asin(arguments[0].Evaluate(), context);
        });

        builtInFunctions.Add("Atan", (arguments, context) =>
        {
            if (arguments.Length != 1) throw new NCalcEvaluationException("Atan() takes exactly 1 argument");
            return MathHelper.Atan(arguments[0].Evaluate(), context);
        });

        builtInFunctions.Add("Atan2", (arguments, context) =>
        {
            if (arguments.Length != 2) throw new NCalcEvaluationException("Atan2() takes exactly 2 arguments");
            return MathHelper.Atan2(arguments[0].Evaluate(), arguments[1].Evaluate(), context);
        });

        builtInFunctions.Add("Ceiling", (arguments, context) =>
        {
            if (arguments.Length != 1) throw new NCalcEvaluationException("Ceiling() takes exactly 1 argument");
            return MathHelper.Ceiling(arguments[0].Evaluate(), context);
        });

        builtInFunctions.Add("Cos", (arguments, context) =>
        {
            if (arguments.Length != 1) throw new NCalcEvaluationException("Cos() takes exactly 1 argument");
            return MathHelper.Cos(arguments[0].Evaluate(), context);
        });

        builtInFunctions.Add("Exp", (arguments, context) =>
        {
            if (arguments.Length != 1) throw new NCalcEvaluationException("Exp() takes exactly 1 argument");
            return MathHelper.Exp(arguments[0].Evaluate(), context);
        });

        builtInFunctions.Add("Floor", (arguments, context) =>
        {
            if (arguments.Length != 1) throw new NCalcEvaluationException("Floor() takes exactly 1 argument");
            return MathHelper.Floor(arguments[0].Evaluate(), context);
        });

        builtInFunctions.Add("IEEERemainder", (arguments, context) =>
        {
            if (arguments.Length != 2) throw new NCalcEvaluationException("IEEERemainder() takes exactly 2 arguments");
            return MathHelper.IEEERemainder(arguments[0].Evaluate(), arguments[1].Evaluate(), context);
        });

        builtInFunctions.Add("Ln", (arguments, context) =>
        {
            if (arguments.Length != 1) throw new NCalcEvaluationException("Ln() takes exactly 1 argument");
            return MathHelper.Ln(arguments[0].Evaluate(), context);
        });

        builtInFunctions.Add("Log", (arguments, context) =>
        {
            if (arguments.Length != 2) throw new NCalcEvaluationException("Log() takes exactly 2 arguments");
            return MathHelper.Log(arguments[0].Evaluate(), arguments[1].Evaluate(), context);
        });

        builtInFunctions.Add("Log10", (arguments, context) =>
        {
            if (arguments.Length != 1) throw new NCalcEvaluationException("Log10() takes exactly 1 argument");
            return MathHelper.Log10(arguments[0].Evaluate(), context);
        });

        builtInFunctions.Add("Pow", (arguments, context) =>
        {
            if (arguments.Length != 2) throw new NCalcEvaluationException("Pow() takes exactly 2 arguments");
            return MathHelper.Pow(arguments[0].Evaluate(), arguments[1].Evaluate(), context);
        });

        builtInFunctions.Add("Round", (arguments, context) =>
        {
            if (arguments.Length != 2) throw new NCalcEvaluationException("Round() takes exactly 2 arguments");
            var rounding = context.Options.HasFlag(ExpressionOptions.RoundAwayFromZero)
                ? MidpointRounding.AwayFromZero
                : MidpointRounding.ToEven;
            return MathHelper.Round(arguments[0].Evaluate(), arguments[1].Evaluate(), rounding, context);
        });

        builtInFunctions.Add("Sign", (arguments, context) =>
        {
            if (arguments.Length != 1) throw new NCalcEvaluationException("Sign() takes exactly 1 argument");
            return MathHelper.Sign(arguments[0].Evaluate(), context);
        });

        builtInFunctions.Add("Sin", (arguments, context) =>
        {
            if (arguments.Length != 1) throw new NCalcEvaluationException("Sin() takes exactly 1 argument");
            return MathHelper.Sin(arguments[0].Evaluate(), context);
        });

        builtInFunctions.Add("Sqrt", (arguments, context) =>
        {
            if (arguments.Length != 1) throw new NCalcEvaluationException("Sqrt() takes exactly 1 argument");
            return MathHelper.Sqrt(arguments[0].Evaluate(), context);
        });

        builtInFunctions.Add("Tan", (arguments, context) =>
        {
            if (arguments.Length != 1) throw new NCalcEvaluationException("Tan() takes exactly 1 argument");
            return MathHelper.Tan(arguments[0].Evaluate(), context);
        });

        builtInFunctions.Add("Truncate", (arguments, context) =>
        {
            if (arguments.Length != 1) throw new NCalcEvaluationException("Truncate() takes exactly 1 argument");
            return MathHelper.Truncate(arguments[0].Evaluate(), context);
        });

        builtInFunctions.Add("Max", (arguments, context) =>
        {
            if (arguments.Length != 2) throw new NCalcEvaluationException("Max() takes exactly 2 arguments");
            return MathHelper.Max(arguments[0].Evaluate(), arguments[1].Evaluate(), context);
        });

        builtInFunctions.Add("Min", (arguments, context) =>
        {
            if (arguments.Length != 2) throw new NCalcEvaluationException("Min() takes exactly 2 arguments");
            return MathHelper.Min(arguments[0].Evaluate(), arguments[1].Evaluate(), context);
        });

        builtInFunctions.Add("ifs", (arguments, context) =>
        {
            if (arguments.Length < 3 || arguments.Length % 2 != 1)
                throw new NCalcEvaluationException("ifs() takes at least 3 arguments, or an odd number of arguments");
            foreach (var argument in arguments.Where((_, i) => i % 2 == 0))
            {
                var index = Array.IndexOf(arguments, argument);
                var tf = Convert.ToBoolean(argument.Evaluate(), context.CultureInfo);
                if (index == arguments.Length - 1) return argument.Evaluate();
                if (tf) return arguments[index + 1].Evaluate();
            }

            return null;
        });

        builtInFunctions.Add("if", (arguments, context) =>
        {
            if (arguments.Length != 3) throw new NCalcEvaluationException("if() takes exactly 3 arguments");
            var cond = Convert.ToBoolean(arguments[0].Evaluate(), context.CultureInfo);
            return cond ? arguments[1].Evaluate() : arguments[2].Evaluate();
        });

        builtInFunctions.Add("in", (arguments, context) =>
        {
            if (arguments.Length < 2) throw new NCalcEvaluationException("in() takes at least 2 arguments");
            var parameter = arguments[0].Evaluate();
            var evaluation = false;
            for (var i = 1; i < arguments.Length; i++)
            {
                if (TypeHelper.CompareUsingMostPreciseType(parameter, arguments[i].Evaluate(), new(context.CultureInfo,
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
