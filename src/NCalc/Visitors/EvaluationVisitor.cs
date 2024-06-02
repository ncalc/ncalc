using NCalc.Domain;
using NCalc.Exceptions;
using NCalc.Handlers;
using NCalc.Helpers;

namespace NCalc.Visitors;

public class EvaluationVisitor : IEvaluationVisitor
{
    public event EvaluateFunctionHandler? EvaluateFunction;
    public event EvaluateParameterHandler? EvaluateParameter;

    public ExpressionOptions Options
    {
        get => Context.Options;
        set => Context.Options = value;
    }

    public CultureInfo CultureInfo
    {
        get => Context.CultureInfo;
        set => Context.CultureInfo = value;
    }

    public Dictionary<string, object?> Parameters { get; set; } = new();

    public ExpressionContext Context { get; set; } = new();

    public object? Result { get; set; }

    private MathHelperOptions MathHelperOptions => new(Context.CultureInfo,
        Context.Options.HasFlag(ExpressionOptions.AllowBooleanCalculation), Context.Options.HasFlag(ExpressionOptions.DecimalAsDefault));

    public EvaluationVisitor()
    {
    }

    public EvaluationVisitor(ExpressionOptions options, CultureInfo cultureInfo)
    {
        Context = new(options, cultureInfo);
    }

    private object? Evaluate(LogicalExpression expression)
    {
        expression.Accept(this);
        return Result;
    }

    public void Visit(TernaryExpression expression)
    {
        // Evaluates the left expression and saves the value
        expression.LeftExpression.Accept(this);
        var left = Convert.ToBoolean(Result, CultureInfo);

        if (left)
        {
            expression.MiddleExpression.Accept(this);
        }
        else
        {
            expression.RightExpression.Accept(this);
        }
    }

    public void Visit(BinaryExpression expression)
    {
        // simulate Lazy<Func<>> behavior for late evaluation
        object? leftValue = null;

        // simulate Lazy<Func<>> behavior for late evaluations
        object? rightValue = null;

        switch (expression.Type)
        {
            case BinaryExpressionType.And:
                Result = Convert.ToBoolean(Left(), CultureInfo) && Convert.ToBoolean(Right(), CultureInfo);
                break;

            case BinaryExpressionType.Or:
                Result = Convert.ToBoolean(Left(), CultureInfo) || Convert.ToBoolean(Right(), CultureInfo);
                break;

            case BinaryExpressionType.Div:
                Result = TypeHelper.IsReal(Left()) || TypeHelper.IsReal(Right())
                    ? MathHelper.Divide(Left(), Right(), MathHelperOptions)
                    : MathHelper.Divide(Convert.ToDouble(Left(), CultureInfo), Right(), MathHelperOptions);
                break;

            case BinaryExpressionType.Equal:
                // Use the type of the left operand to make the comparison
                Result = CompareUsingMostPreciseType(Left(), Right()) == 0;
                break;

            case BinaryExpressionType.Greater:
                // Use the type of the left operand to make the comparison
                Result = CompareUsingMostPreciseType(Left(), Right()) > 0;
                break;

            case BinaryExpressionType.GreaterOrEqual:
                // Use the type of the left operand to make the comparison
                Result = CompareUsingMostPreciseType(Left(), Right()) >= 0;
                break;

            case BinaryExpressionType.Lesser:
                // Use the type of the left operand to make the comparison
                Result = CompareUsingMostPreciseType(Left(), Right()) < 0;
                break;

            case BinaryExpressionType.LesserOrEqual:
                // Use the type of the left operand to make the comparison
                Result = CompareUsingMostPreciseType(Left(), Right()) <= 0;
                break;

            case BinaryExpressionType.Minus:
                Result = MathHelper.Subtract(Left(), Right(), MathHelperOptions);
                break;

            case BinaryExpressionType.Modulo:
                Result = MathHelper.Modulo(Left(), Right(), MathHelperOptions);
                break;

            case BinaryExpressionType.NotEqual:
                // Use the type of the left operand to make the comparison
                Result = CompareUsingMostPreciseType(Left(), Right()) != 0;
                break;

            case BinaryExpressionType.Plus:
                if (Left() is string)
                {
                    Result = string.Concat(Left(), Right());
                }
                else
                {
                    Result = MathHelper.Add(Left(), Right(), MathHelperOptions);
                }

                break;

            case BinaryExpressionType.Times:
                Result = MathHelper.Multiply(Left(), Right(), MathHelperOptions);
                break;

            case BinaryExpressionType.BitwiseAnd:
                Result = Convert.ToUInt16(Left(), CultureInfo) & Convert.ToUInt16(Right(), CultureInfo);
                break;

            case BinaryExpressionType.BitwiseOr:
                Result = Convert.ToUInt16(Left(), CultureInfo) | Convert.ToUInt16(Right(), CultureInfo);
                break;

            case BinaryExpressionType.BitwiseXOr:
                Result = Convert.ToUInt16(Left(), CultureInfo) ^ Convert.ToUInt16(Right(), CultureInfo);
                break;

            case BinaryExpressionType.LeftShift:
                Result = Convert.ToUInt16(Left(), CultureInfo) << Convert.ToUInt16(Right(), CultureInfo);
                break;

            case BinaryExpressionType.RightShift:
                Result = Convert.ToUInt16(Left(), CultureInfo) >> Convert.ToUInt16(Right(), CultureInfo);
                break;

            case BinaryExpressionType.Exponentiation:
                Result = Math.Pow(Convert.ToDouble(Left(), CultureInfo), Convert.ToDouble(Right(), CultureInfo));
                break;
        }

        return;

        object? Left()
        {
            if (leftValue != null)
                return leftValue;

            expression.LeftExpression.Accept(this);
            leftValue = Result;

            return leftValue;
        }

        object? Right()
        {
            if (rightValue != null)
                return rightValue;

            expression.RightExpression.Accept(this);
            rightValue = Result;

            return rightValue;
        }
    }

    public void Visit(UnaryExpression expression)
    {
        // Recursively evaluates the underlying expression
        expression.Expression.Accept(this);

        switch (expression.Type)
        {
            case UnaryExpressionType.Not:
                Result = !Convert.ToBoolean(Result, CultureInfo);
                break;

            case UnaryExpressionType.Negate:
                Result = MathHelper.Subtract(0, Result, MathHelperOptions);
                break;

            case UnaryExpressionType.BitwiseNot:
                Result = ~Convert.ToUInt16(Result, CultureInfo);
                break;

            case UnaryExpressionType.Positive:
                // No-op
                break;
        }
    }

    public void Visit(ValueExpression expression)
    {
        Result = expression.Value;
    }

    public void Visit(Function function)
    {
        var args = new FunctionArgs
        {
            Parameters = new Expression[function.Expressions.Length]
        };

        // Don't call parameters right now, instead let the function do it as needed.
        // Some parameters shouldn't be called, for instance, in a if(), the "not" value might be a division by zero
        // Evaluating every value could produce unexpected behaviour
        for (var i = 0; i < function.Expressions.Length; i++)
        {
            args.Parameters[i] = new Expression(function.Expressions[i], Options, CultureInfo);
            args.Parameters[i].EvaluateFunction += EvaluateFunction;
            args.Parameters[i].EvaluateParameter += EvaluateParameter;

            // Assign the parameters of the Expression to the arguments so that custom Functions and Parameters can use them
            args.Parameters[i].Parameters = Parameters;
        }

        // Calls external implementation
        OnEvaluateFunction(function.Identifier.Name, args);

        // If an external implementation was found get the result back
        if (args.HasResult)
        {
            Result = args.Result;
            return;
        }

        ExecuteBuiltInFunction(function);
    }



    private void ExecuteBuiltInFunction(Function function)
    {
        var functionName = function.Identifier.Name.ToUpperInvariant();

        switch (functionName)
        {
            case "ABS":
                CheckCase("Abs", function.Identifier.Name);
                if (function.Expressions.Length != 1)
                    throw new NCalcEvaluationException("Abs() takes exactly 1 argument");
                Result = MathHelper.Abs(Evaluate(function.Expressions[0]), MathHelperOptions);
                break;

            case "ACOS":
                CheckCase("Acos", function.Identifier.Name);
                if (function.Expressions.Length != 1)
                    throw new NCalcEvaluationException("Acos() takes exactly 1 argument");
                Result = MathHelper.Acos(Evaluate(function.Expressions[0]), MathHelperOptions);
                break;

            case "ASIN":
                CheckCase("Asin", function.Identifier.Name);
                if (function.Expressions.Length != 1)
                    throw new NCalcEvaluationException("Asin() takes exactly 1 argument");
                Result = MathHelper.Asin(Evaluate(function.Expressions[0]), MathHelperOptions);
                break;

            case "ATAN":
                CheckCase("Atan", function.Identifier.Name);
                if (function.Expressions.Length != 1)
                    throw new NCalcEvaluationException("Atan() takes exactly 1 argument");
                Result = MathHelper.Atan(Evaluate(function.Expressions[0]), MathHelperOptions);
                break;

            case "ATAN2":
                CheckCase("Atan2", function.Identifier.Name);
                if (function.Expressions.Length != 2)
                    throw new NCalcEvaluationException("Atan2() takes exactly 2 arguments");
                Result = MathHelper.Atan2(Evaluate(function.Expressions[0]), Evaluate(function.Expressions[1]),
                    MathHelperOptions);
                break;

            case "CEILING":
                CheckCase("Ceiling", function.Identifier.Name);
                if (function.Expressions.Length != 1)
                    throw new NCalcEvaluationException("Ceiling() takes exactly 1 argument");
                Result = MathHelper.Ceiling(Evaluate(function.Expressions[0]), MathHelperOptions);
                break;

            case "COS":
                CheckCase("Cos", function.Identifier.Name);
                if (function.Expressions.Length != 1)
                    throw new NCalcEvaluationException("Cos() takes exactly 1 argument");
                Result = MathHelper.Cos(Evaluate(function.Expressions[0]), MathHelperOptions);
                break;

            case "EXP":
                CheckCase("Exp", function.Identifier.Name);
                if (function.Expressions.Length != 1)
                    throw new NCalcEvaluationException("Exp() takes exactly 1 argument");
                Result = MathHelper.Exp(Evaluate(function.Expressions[0]), MathHelperOptions);
                break;

            case "FLOOR":
                CheckCase("Floor", function.Identifier.Name);
                if (function.Expressions.Length != 1)
                    throw new NCalcEvaluationException("Floor() takes exactly 1 argument");
                Result = MathHelper.Floor(Evaluate(function.Expressions[0]), MathHelperOptions);
                break;

            case "IEEEREMAINDER":
                CheckCase("IEEERemainder", function.Identifier.Name);
                if (function.Expressions.Length != 2)
                    throw new NCalcEvaluationException("IEEERemainder() takes exactly 2 arguments");
                Result = MathHelper.IEEERemainder(Evaluate(function.Expressions[0]), Evaluate(function.Expressions[1]),
                    MathHelperOptions);
                break;

            case "LN":
                CheckCase("Ln", function.Identifier.Name);
                if (function.Expressions.Length != 1)
                    throw new NCalcEvaluationException("Ln() takes exactly 1 argument");
                Result = MathHelper.Ln(Evaluate(function.Expressions[0]), MathHelperOptions);
                break;

            case "LOG":
                CheckCase("Log", function.Identifier.Name);
                if (function.Expressions.Length != 2)
                    throw new NCalcEvaluationException("Log() takes exactly 2 arguments");
                Result = MathHelper.Log(Evaluate(function.Expressions[0]), Evaluate(function.Expressions[1]),
                    MathHelperOptions);
                break;

            case "LOG10":
                CheckCase("Log10", function.Identifier.Name);
                if (function.Expressions.Length != 1)
                    throw new NCalcEvaluationException("Log10() takes exactly 1 argument");
                Result = MathHelper.Log10(Evaluate(function.Expressions[0]), MathHelperOptions);
                break;

            case "POW":
                CheckCase("Pow", function.Identifier.Name);
                if (function.Expressions.Length != 2)
                    throw new NCalcEvaluationException("Pow() takes exactly 2 arguments");
                Result = MathHelper.Pow(Evaluate(function.Expressions[0]), Evaluate(function.Expressions[1]),
                    MathHelperOptions);
                break;

            case "ROUND":
                CheckCase("Round", function.Identifier.Name);
                if (function.Expressions.Length != 2)
                    throw new NCalcEvaluationException("Round() takes exactly 2 arguments");

                var rounding = (Options & ExpressionOptions.RoundAwayFromZero) == ExpressionOptions.RoundAwayFromZero
                    ? MidpointRounding.AwayFromZero
                    : MidpointRounding.ToEven;

                Result = MathHelper.Round(Evaluate(function.Expressions[0]), Evaluate(function.Expressions[1]),
                    rounding, MathHelperOptions);
                break;

            case "SIGN":
                CheckCase("Sign", function.Identifier.Name);
                if (function.Expressions.Length != 1)
                    throw new NCalcEvaluationException("Sign() takes exactly 1 argument");
                Result = MathHelper.Sign(Evaluate(function.Expressions[0]), MathHelperOptions);
                break;

            case "SIN":
                CheckCase("Sin", function.Identifier.Name);
                if (function.Expressions.Length != 1)
                    throw new NCalcEvaluationException("Sin() takes exactly 1 argument");
                Result = MathHelper.Sin(Evaluate(function.Expressions[0]), MathHelperOptions);
                break;

            case "SQRT":
                CheckCase("Sqrt", function.Identifier.Name);
                if (function.Expressions.Length != 1)
                    throw new NCalcEvaluationException("Sqrt() takes exactly 1 argument");
                Result = MathHelper.Sqrt(Evaluate(function.Expressions[0]), MathHelperOptions);
                break;

            case "TAN":
                CheckCase("Tan", function.Identifier.Name);
                if (function.Expressions.Length != 1)
                    throw new NCalcEvaluationException("Tan() takes exactly 1 argument");
                Result = MathHelper.Tan(Evaluate(function.Expressions[0]), MathHelperOptions);
                break;

            case "TRUNCATE":
                CheckCase("Truncate", function.Identifier.Name);
                if (function.Expressions.Length != 1)
                    throw new NCalcEvaluationException("Truncate() takes exactly 1 argument");
                Result = MathHelper.Truncate(Evaluate(function.Expressions[0]), MathHelperOptions);
                break;

            case "MAX":
                CheckCase("Max", function.Identifier.Name);
                if (function.Expressions.Length != 2)
                    throw new NCalcEvaluationException("Max() takes exactly 2 arguments");
                var maxleft = Evaluate(function.Expressions[0]);
                var maxright = Evaluate(function.Expressions[1]);
                Result = MathHelper.Max(maxleft, maxright, MathHelperOptions);
                break;

            case "MIN":
                CheckCase("Min", function.Identifier.Name);
                if (function.Expressions.Length != 2)
                    throw new NCalcEvaluationException("Min() takes exactly 2 arguments");
                var minleft = Evaluate(function.Expressions[0]);
                var minright = Evaluate(function.Expressions[1]);
                Result = MathHelper.Min(minleft, minright, MathHelperOptions);
                break;

            case "IFS":
                CheckCase("ifs", function.Identifier.Name);
                if (function.Expressions.Length < 3 || function.Expressions.Length % 2 != 1)
                    throw new NCalcEvaluationException(
                        "ifs() takes at least 3 arguments, or an odd number of arguments");
                foreach (var eval in function.Expressions.Where((_, i) => i % 2 == 0))
                {
                    var index = Array.IndexOf(function.Expressions, eval);
                    var tf = Convert.ToBoolean(Evaluate(eval), CultureInfo);
                    if (index == function.Expressions.Length - 1)
                        Result = Evaluate(eval);
                    else if (tf)
                    {
                        Result = Evaluate(function.Expressions[index + 1]);
                        break;
                    }
                }

                break;

            case "IF":
                CheckCase("if", function.Identifier.Name);
                if (function.Expressions.Length != 3)
                    throw new NCalcEvaluationException("if() takes exactly 3 arguments");
                var cond = Convert.ToBoolean(Evaluate(function.Expressions[0]), CultureInfo);
                Result = cond ? Evaluate(function.Expressions[1]) : Evaluate(function.Expressions[2]);
                break;

            case "IN":
                CheckCase("in", function.Identifier.Name);
                if (function.Expressions.Length < 2)
                    throw new NCalcEvaluationException("in() takes at least 2 arguments");
                var parameter = Evaluate(function.Expressions[0]);
                var evaluation = false;
                for (var i = 1; i < function.Expressions.Length; i++)
                {
                    var argument = Evaluate(function.Expressions[i]);
                    if (CompareUsingMostPreciseType(parameter, argument) == 0)
                    {
                        evaluation = true;
                        break;
                    }
                }

                Result = evaluation;
                break;

            default:
                throw new NCalcFunctionNotFoundException($"Function {function.Identifier.Name} not found",
                    function.Identifier.Name);
        }
    }

    public int CompareUsingMostPreciseType(object? a, object? b)
    {
        return TypeHelper.CompareUsingMostPreciseType(a,
            b, new(CultureInfo,
                Options.HasFlag(ExpressionOptions.CaseInsensitiveStringComparer),
                Options.HasFlag(ExpressionOptions.OrdinalStringComparer)));
    }

    private void CheckCase(string function, string called)
    {
        bool ignoreCase = Options.HasFlag(ExpressionOptions.IgnoreCase);

        if (!ignoreCase && function != called)
            throw new NCalcFunctionNotFoundException($"Function {called} not found. Try {function} instead.", called);
    }

    public void Visit(Identifier identifier)
    {
        if (Parameters.TryGetValue(identifier.Name, out var parameterValue))
        {
            // The parameter is defined in the hashtable
            if (parameterValue is Expression expression)
            {
                // Overloads parameters 
                foreach (var p in Parameters)
                {
                    if (expression.Parameters != null)
                        expression.Parameters[p.Key] = p.Value;
                }

                expression.EvaluateFunction += EvaluateFunction;
                expression.EvaluateParameter += EvaluateParameter;

                Result = expression.Evaluate();
            }
            else
                Result = parameterValue;
        }
        else
        {
            // The parameter should be defined in a call back method
            var args = new ParameterArgs();

            // Calls external implementation
            OnEvaluateParameter(identifier.Name, args);

            if (!args.HasResult)
                throw new NCalcParameterNotDefinedException(identifier.Name);

            Result = args.Result;
        }
    }

    protected void OnEvaluateFunction(string name, FunctionArgs args)
    {
        EvaluateFunction?.Invoke(name, args);
    }

    protected void OnEvaluateParameter(string name, ParameterArgs args)
    {
        EvaluateParameter?.Invoke(name, args);
    }
}