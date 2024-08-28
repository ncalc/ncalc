using NCalc.Exceptions;

namespace NCalc.Helpers;

public static class BuiltInFunctionHelper
{
    public static object? Evaluate(string functionName, Expression[] arguments, ExpressionContext context)
    {
        var caseInsensitive = context.Options.HasFlag(ExpressionOptions.IgnoreCaseAtBuiltInFunctions);
        var comparison = caseInsensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

        if (functionName.Equals("Abs", comparison))
        {
            if (arguments.Length != 1)
                throw new NCalcEvaluationException("Abs() takes exactly 1 argument");
            return MathHelper.Abs(arguments[0].Evaluate(), context);
        }
        if (functionName.Equals("Acos", comparison))
        {
            if (arguments.Length != 1)
                throw new NCalcEvaluationException("Acos() takes exactly 1 argument");
            return MathHelper.Acos(arguments[0].Evaluate(), context);
        }
        if (functionName.Equals("Asin", comparison))
        {
            if (arguments.Length != 1)
                throw new NCalcEvaluationException("Asin() takes exactly 1 argument");
            return MathHelper.Asin(arguments[0].Evaluate(), context);
        }
        if (functionName.Equals("Atan", comparison))
        {
            if (arguments.Length != 1)
                throw new NCalcEvaluationException("Atan() takes exactly 1 argument");
            return MathHelper.Atan(arguments[0].Evaluate(), context);
        }
        if (functionName.Equals("Atan2", comparison))
        {
            if (arguments.Length != 2)
                throw new NCalcEvaluationException("Atan2() takes exactly 2 arguments");
            return MathHelper.Atan2(arguments[0].Evaluate(), arguments[1].Evaluate(), context);
        }
        if (functionName.Equals("Ceiling", comparison))
        {
            if (arguments.Length != 1)
                throw new NCalcEvaluationException("Ceiling() takes exactly 1 argument");
            return MathHelper.Ceiling(arguments[0].Evaluate(), context);
        }
        if (functionName.Equals("Cos", comparison))
        {
            if (arguments.Length != 1)
                throw new NCalcEvaluationException("Cos() takes exactly 1 argument");
            return MathHelper.Cos(arguments[0].Evaluate(), context);
        }
        if (functionName.Equals("Exp", comparison))
        {
            if (arguments.Length != 1)
                throw new NCalcEvaluationException("Exp() takes exactly 1 argument");
            return MathHelper.Exp(arguments[0].Evaluate(), context);
        }
        if (functionName.Equals("Floor", comparison))
        {
            if (arguments.Length != 1)
                throw new NCalcEvaluationException("Floor() takes exactly 1 argument");
            return MathHelper.Floor(arguments[0].Evaluate(), context);
        }
        if (functionName.Equals("IEEERemainder", comparison))
        {
            if (arguments.Length != 2)
                throw new NCalcEvaluationException("IEEERemainder() takes exactly 2 arguments");
            return MathHelper.IEEERemainder(arguments[0].Evaluate(), arguments[1].Evaluate(), context);
        }
        if (functionName.Equals("Ln", comparison))
        {
            if (arguments.Length != 1)
                throw new NCalcEvaluationException("Ln() takes exactly 1 argument");
            return MathHelper.Ln(arguments[0].Evaluate(), context);
        }
        if (functionName.Equals("Log", comparison))
        {
            if (arguments.Length != 2)
                throw new NCalcEvaluationException("Log() takes exactly 2 arguments");
            return MathHelper.Log(arguments[0].Evaluate(), arguments[1].Evaluate(), context);
        }
        if (functionName.Equals("Log10", comparison))
        {
            if (arguments.Length != 1)
                throw new NCalcEvaluationException("Log10() takes exactly 1 argument");
            return MathHelper.Log10(arguments[0].Evaluate(), context);
        }
        if (functionName.Equals("Pow", comparison))
        {
            if (arguments.Length != 2)
                throw new NCalcEvaluationException("Pow() takes exactly 2 arguments");
            return MathHelper.Pow(arguments[0].Evaluate(), arguments[1].Evaluate(), context);
        }
        if (functionName.Equals("Round", comparison))
        {
            if (arguments.Length != 2)
                throw new NCalcEvaluationException("Round() takes exactly 2 arguments");
            var rounding = context.Options.HasFlag(ExpressionOptions.RoundAwayFromZero)
                ? MidpointRounding.AwayFromZero
                : MidpointRounding.ToEven;
            return MathHelper.Round(arguments[0].Evaluate(), arguments[1].Evaluate(), rounding, context);
        }
        if (functionName.Equals("Sign", comparison))
        {
            if (arguments.Length != 1)
                throw new NCalcEvaluationException("Sign() takes exactly 1 argument");
            return MathHelper.Sign(arguments[0].Evaluate(), context);
        }
        if (functionName.Equals("Sin", comparison))
        {
            if (arguments.Length != 1)
                throw new NCalcEvaluationException("Sin() takes exactly 1 argument");
            return MathHelper.Sin(arguments[0].Evaluate(), context);
        }
        if (functionName.Equals("Sqrt", comparison))
        {
            if (arguments.Length != 1)
                throw new NCalcEvaluationException("Sqrt() takes exactly 1 argument");
            return MathHelper.Sqrt(arguments[0].Evaluate(), context);
        }
        if (functionName.Equals("Tan", comparison))
        {
            if (arguments.Length != 1)
                throw new NCalcEvaluationException("Tan() takes exactly 1 argument");
            return MathHelper.Tan(arguments[0].Evaluate(), context);
        }
        if (functionName.Equals("Truncate", comparison))
        {
            if (arguments.Length != 1)
                throw new NCalcEvaluationException("Truncate() takes exactly 1 argument");
            return MathHelper.Truncate(arguments[0].Evaluate(), context);
        }
        if (functionName.Equals("Max", comparison))
        {
            if (arguments.Length != 2)
                throw new NCalcEvaluationException("Max() takes exactly 2 arguments");
            return MathHelper.Max(arguments[0].Evaluate(), arguments[1].Evaluate(), context);
        }
        if (functionName.Equals("Min", comparison))
        {
            if (arguments.Length != 2)
                throw new NCalcEvaluationException("Min() takes exactly 2 arguments");
            return MathHelper.Min(arguments[0].Evaluate(), arguments[1].Evaluate(), context);
        }
        if (functionName.Equals("ifs", comparison))
        {
            if (arguments.Length < 3 || arguments.Length % 2 != 1)
            {
                throw new NCalcEvaluationException(
                    "ifs() takes at least 3 arguments, or an odd number of arguments");
            }

            foreach (var argument in arguments.Where((_, i) => i % 2 == 0))
            {
                var index = Array.IndexOf(arguments, argument);
                if (index == arguments.Length - 1)
                    return argument.Evaluate();

                var tf = Convert.ToBoolean(argument.Evaluate(), context.CultureInfo);
                if (tf)
                    return arguments[index + 1].Evaluate();
            }

            return null;
        }
        if (functionName.Equals("if", comparison))
        {
            if (arguments.Length != 3)
                throw new NCalcEvaluationException("if() takes exactly 3 arguments");
            var cond = Convert.ToBoolean(arguments[0].Evaluate(), context.CultureInfo);
            return cond ? arguments[1].Evaluate() : arguments[2].Evaluate();
        }
        if (functionName.Equals("in", comparison))
        {
            if (arguments.Length < 2)
                throw new NCalcEvaluationException("in() takes at least 2 arguments");
            var parameter = arguments[0].Evaluate();
            var evaluation = false;
            for (var i = 1; i < arguments.Length; i++)
            {
                if (TypeHelper.CompareUsingMostPreciseType(parameter, arguments[i].Evaluate(), context) != 0) continue;
                evaluation = true;
                break;
            }

            return evaluation;
        }

        throw new NCalcFunctionNotFoundException(functionName);
    }
}
