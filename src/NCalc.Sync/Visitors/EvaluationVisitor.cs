using NCalc.Domain;
using NCalc.Exceptions;
using NCalc.Handlers;
using NCalc.Helpers;

namespace NCalc.Visitors;

public class EvaluationVisitor : ExpressionEvaluator, IEvaluationVisitor
{
    public event EvaluateFunctionHandler? EvaluateFunction;
    public event EvaluateParameterHandler? EvaluateParameter;

    public EvaluationVisitor()
    {
    }

    public EvaluationVisitor(ExpressionOptions options, CultureInfo cultureInfo)
    {
        Context = new(options, cultureInfo);
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
        var leftValue = new Lazy<object?>(() => Evaluate(expression.LeftExpression));
        var rightValue = new Lazy<object?>(() => Evaluate(expression.RightExpression));

        switch (expression.Type)
        {
            case BinaryExpressionType.And:
                Result = Convert.ToBoolean(leftValue.Value, CultureInfo) &&
                         Convert.ToBoolean(rightValue.Value, CultureInfo);
                break;

            case BinaryExpressionType.Or:
                Result = Convert.ToBoolean(leftValue.Value, CultureInfo) ||
                         Convert.ToBoolean(rightValue.Value, CultureInfo);
                break;

            case BinaryExpressionType.Div:
                Result = TypeHelper.IsReal(leftValue.Value) || TypeHelper.IsReal(rightValue.Value)
                    ? MathHelper.Divide(leftValue.Value, rightValue.Value, MathHelperOptions)
                    : MathHelper.Divide(Convert.ToDouble(leftValue.Value, CultureInfo), rightValue.Value,
                        MathHelperOptions);
                break;

            case BinaryExpressionType.Equal:
                Result = CompareUsingMostPreciseType(leftValue.Value, rightValue.Value) == 0;
                break;

            case BinaryExpressionType.Greater:
                Result = CompareUsingMostPreciseType(leftValue.Value, rightValue.Value) > 0;
                break;

            case BinaryExpressionType.GreaterOrEqual:
                Result = CompareUsingMostPreciseType(leftValue.Value, rightValue.Value) >= 0;
                break;

            case BinaryExpressionType.Lesser:
                Result = CompareUsingMostPreciseType(leftValue.Value, rightValue.Value) < 0;
                break;

            case BinaryExpressionType.LesserOrEqual:
                Result = CompareUsingMostPreciseType(leftValue.Value, rightValue.Value) <= 0;
                break;

            case BinaryExpressionType.Minus:
                Result = MathHelper.Subtract(leftValue.Value, rightValue.Value, MathHelperOptions);
                break;

            case BinaryExpressionType.Modulo:
                Result = MathHelper.Modulo(leftValue.Value, rightValue.Value, MathHelperOptions);
                break;

            case BinaryExpressionType.NotEqual:
                Result = CompareUsingMostPreciseType(leftValue.Value, rightValue.Value) != 0;
                break;

            case BinaryExpressionType.Plus:
                if (leftValue.Value is string)
                {
                    Result = string.Concat(leftValue.Value, rightValue.Value);
                }
                else
                {
                    Result = MathHelper.Add(leftValue.Value, rightValue.Value, MathHelperOptions);
                }

                break;

            case BinaryExpressionType.Times:
                Result = MathHelper.Multiply(leftValue.Value, rightValue.Value, MathHelperOptions);
                break;

            case BinaryExpressionType.BitwiseAnd:
                Result = Convert.ToUInt64(Left(), CultureInfo) &
						 Convert.ToUInt64(Right(), CultureInfo);
                break;

            case BinaryExpressionType.BitwiseOr:
                Result = Convert.ToUInt64(Left(), CultureInfo) |
						 Convert.ToUInt64(Right(), CultureInfo);
                break;

            case BinaryExpressionType.BitwiseXOr:
                Result = Convert.ToUInt64(Left(), CultureInfo) ^
						 Convert.ToUInt64(Right(), CultureInfo);
                break;

            case BinaryExpressionType.LeftShift:
                Result = Convert.ToUInt64(Left(), CultureInfo) <<
						 Convert.ToInt32(Right(), CultureInfo);
                break;

            case BinaryExpressionType.RightShift:
                Result = Convert.ToUInt64(Left(), CultureInfo) >>
						 Convert.ToInt32(Right(), CultureInfo);

            case BinaryExpressionType.Exponentiation:
                Result = Math.Pow(Convert.ToDouble(leftValue.Value, CultureInfo),
                    Convert.ToDouble(rightValue.Value, CultureInfo));
                break;
        }
    }
    
    public void Visit(UnaryExpression expression)
    {
        // Recursively evaluates the underlying expression
        expression.Expression.Accept(this);

        ExecuteUnaryExpression(expression);
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

        Result = ExecuteBuiltInFunction(function);
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
    
    private object? ExecuteBuiltInFunction(Function function)
    {
        var functionName = function.Identifier.Name.ToUpperInvariant();
        object? result = null;

        switch (functionName)
        {
            case "ABS":
                CheckCase("Abs", function.Identifier.Name);
                if (function.Expressions.Length != 1)
                    throw new NCalcEvaluationException("Abs() takes exactly 1 argument");
                result = MathHelper.Abs(Evaluate(function.Expressions[0]), MathHelperOptions);
                break;

            case "ACOS":
                CheckCase("Acos", function.Identifier.Name);
                if (function.Expressions.Length != 1)
                    throw new NCalcEvaluationException("Acos() takes exactly 1 argument");
                result = MathHelper.Acos(Evaluate(function.Expressions[0]), MathHelperOptions);
                break;

            case "ASIN":
                CheckCase("Asin", function.Identifier.Name);
                if (function.Expressions.Length != 1)
                    throw new NCalcEvaluationException("Asin() takes exactly 1 argument");
                result = MathHelper.Asin(Evaluate(function.Expressions[0]), MathHelperOptions);
                break;

            case "ATAN":
                CheckCase("Atan", function.Identifier.Name);
                if (function.Expressions.Length != 1)
                    throw new NCalcEvaluationException("Atan() takes exactly 1 argument");
                result = MathHelper.Atan(Evaluate(function.Expressions[0]), MathHelperOptions);
                break;

            case "ATAN2":
                CheckCase("Atan2", function.Identifier.Name);
                if (function.Expressions.Length != 2)
                    throw new NCalcEvaluationException("Atan2() takes exactly 2 arguments");
                result = MathHelper.Atan2(Evaluate(function.Expressions[0]), Evaluate(function.Expressions[1]),
                    MathHelperOptions);
                break;

            case "CEILING":
                CheckCase("Ceiling", function.Identifier.Name);
                if (function.Expressions.Length != 1)
                    throw new NCalcEvaluationException("Ceiling() takes exactly 1 argument");
                result = MathHelper.Ceiling(Evaluate(function.Expressions[0]), MathHelperOptions);
                break;

            case "COS":
                CheckCase("Cos", function.Identifier.Name);
                if (function.Expressions.Length != 1)
                    throw new NCalcEvaluationException("Cos() takes exactly 1 argument");
                result = MathHelper.Cos(Evaluate(function.Expressions[0]), MathHelperOptions);
                break;

            case "EXP":
                CheckCase("Exp", function.Identifier.Name);
                if (function.Expressions.Length != 1)
                    throw new NCalcEvaluationException("Exp() takes exactly 1 argument");
                result = MathHelper.Exp(Evaluate(function.Expressions[0]), MathHelperOptions);
                break;

            case "FLOOR":
                CheckCase("Floor", function.Identifier.Name);
                if (function.Expressions.Length != 1)
                    throw new NCalcEvaluationException("Floor() takes exactly 1 argument");
                result = MathHelper.Floor(Evaluate(function.Expressions[0]), MathHelperOptions);
                break;

            case "IEEEREMAINDER":
                CheckCase("IEEERemainder", function.Identifier.Name);
                if (function.Expressions.Length != 2)
                    throw new NCalcEvaluationException("IEEERemainder() takes exactly 2 arguments");
                result = MathHelper.IEEERemainder(Evaluate(function.Expressions[0]), Evaluate(function.Expressions[1]),
                    MathHelperOptions);
                break;

            case "LN":
                CheckCase("Ln", function.Identifier.Name);
                if (function.Expressions.Length != 1)
                    throw new NCalcEvaluationException("Ln() takes exactly 1 argument");
                result = MathHelper.Ln(Evaluate(function.Expressions[0]), MathHelperOptions);
                break;

            case "LOG":
                CheckCase("Log", function.Identifier.Name);
                if (function.Expressions.Length != 2)
                    throw new NCalcEvaluationException("Log() takes exactly 2 arguments");
                result = MathHelper.Log(Evaluate(function.Expressions[0]), Evaluate(function.Expressions[1]),
                    MathHelperOptions);
                break;

            case "LOG10":
                CheckCase("Log10", function.Identifier.Name);
                if (function.Expressions.Length != 1)
                    throw new NCalcEvaluationException("Log10() takes exactly 1 argument");
                result = MathHelper.Log10(Evaluate(function.Expressions[0]), MathHelperOptions);
                break;

            case "POW":
                CheckCase("Pow", function.Identifier.Name);
                if (function.Expressions.Length != 2)
                    throw new NCalcEvaluationException("Pow() takes exactly 2 arguments");
                result = MathHelper.Pow(Evaluate(function.Expressions[0]), Evaluate(function.Expressions[1]),
                    MathHelperOptions);
                break;

            case "ROUND":
                CheckCase("Round", function.Identifier.Name);
                if (function.Expressions.Length != 2)
                    throw new NCalcEvaluationException("Round() takes exactly 2 arguments");

                var rounding = Options.HasFlag(ExpressionOptions.RoundAwayFromZero)
                    ? MidpointRounding.AwayFromZero
                    : MidpointRounding.ToEven;

                result = MathHelper.Round(Evaluate(function.Expressions[0]), Evaluate(function.Expressions[1]),
                    rounding, MathHelperOptions);
                break;

            case "SIGN":
                CheckCase("Sign", function.Identifier.Name);
                if (function.Expressions.Length != 1)
                    throw new NCalcEvaluationException("Sign() takes exactly 1 argument");
                result = MathHelper.Sign(Evaluate(function.Expressions[0]), MathHelperOptions);
                break;

            case "SIN":
                CheckCase("Sin", function.Identifier.Name);
                if (function.Expressions.Length != 1)
                    throw new NCalcEvaluationException("Sin() takes exactly 1 argument");
                result = MathHelper.Sin(Evaluate(function.Expressions[0]), MathHelperOptions);
                break;

            case "SQRT":
                CheckCase("Sqrt", function.Identifier.Name);
                if (function.Expressions.Length != 1)
                    throw new NCalcEvaluationException("Sqrt() takes exactly 1 argument");
                result = MathHelper.Sqrt(Evaluate(function.Expressions[0]), MathHelperOptions);
                break;

            case "TAN":
                CheckCase("Tan", function.Identifier.Name);
                if (function.Expressions.Length != 1)
                    throw new NCalcEvaluationException("Tan() takes exactly 1 argument");
                result = MathHelper.Tan(Evaluate(function.Expressions[0]), MathHelperOptions);
                break;

            case "TRUNCATE":
                CheckCase("Truncate", function.Identifier.Name);
                if (function.Expressions.Length != 1)
                    throw new NCalcEvaluationException("Truncate() takes exactly 1 argument");
                result = MathHelper.Truncate(Evaluate(function.Expressions[0]), MathHelperOptions);
                break;

            case "MAX":
                CheckCase("Max", function.Identifier.Name);
                if (function.Expressions.Length != 2)
                    throw new NCalcEvaluationException("Max() takes exactly 2 arguments");
                var maxleft = Evaluate(function.Expressions[0]);
                var maxright = Evaluate(function.Expressions[1]);
                result = MathHelper.Max(maxleft, maxright, MathHelperOptions);
                break;

            case "MIN":
                CheckCase("Min", function.Identifier.Name);
                if (function.Expressions.Length != 2)
                    throw new NCalcEvaluationException("Min() takes exactly 2 arguments");
                var minleft = Evaluate(function.Expressions[0]);
                var minright = Evaluate(function.Expressions[1]);
                result = MathHelper.Min(minleft, minright, MathHelperOptions);
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
                        result = Evaluate(eval);
                    else if (tf)
                    {
                        result = Evaluate(function.Expressions[index + 1]);
                        break;
                    }
                }

                break;

            case "IF":
                CheckCase("if", function.Identifier.Name);
                if (function.Expressions.Length != 3)
                    throw new NCalcEvaluationException("if() takes exactly 3 arguments");
                var cond = Convert.ToBoolean(Evaluate(function.Expressions[0]), CultureInfo);
                result = cond ? Evaluate(function.Expressions[1]) : Evaluate(function.Expressions[2]);
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

                result = evaluation;
                break;

            default:
                throw new NCalcFunctionNotFoundException($"Function {function.Identifier.Name} not found",
                    function.Identifier.Name);
        }

        return result;
    }
    
    private object? Evaluate(LogicalExpression expression)
    {
        expression.Accept(this);
        return Result;
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