using System.Linq.Expressions;
using NCalc.Domain;
using NCalc.Exceptions;
using NCalc.Extensions;
using NCalc.Handlers;
using NCalc.Helpers;
using BinaryExpression = NCalc.Domain.BinaryExpression;
using UnaryExpression = NCalc.Domain.UnaryExpression;

namespace NCalc.Visitors;

public class AsyncEvaluationVisitor : EvaluationVisitorBase, IAsyncEvaluationVisitor
{
    public event AsyncEvaluateFunctionHandler? EvaluateFunctionAsync;
    public event AsyncEvaluateParameterHandler? EvaluateParameterAsync;

    public async Task VisitAsync(TernaryExpression expression)
    {
        // Evaluates the left expression and saves the value
        await expression.LeftExpression.AcceptAsync(this);
        var left = Convert.ToBoolean(Result, CultureInfo);

        if (left)
        {
            await expression.MiddleExpression.AcceptAsync(this);
        }
        else
        {
            await expression.RightExpression.AcceptAsync(this);
        }
    }

    public async Task VisitAsync(BinaryExpression expression)
    {
        var leftValue = new Lazy<Task<object?>>(() => EvaluateAsync(expression.LeftExpression));
        var rightValue = new Lazy<Task<object?>>(() => EvaluateAsync(expression.RightExpression));

        switch (expression.Type)
        {
            case BinaryExpressionType.And:
                Result = Convert.ToBoolean(await leftValue.Value, CultureInfo) &&
                         Convert.ToBoolean(await rightValue.Value, CultureInfo);
                break;

            case BinaryExpressionType.Or:
                Result = Convert.ToBoolean(await leftValue.Value, CultureInfo) ||
                         Convert.ToBoolean(await rightValue.Value, CultureInfo);
                break;

            case BinaryExpressionType.Div:
                Result = TypeHelper.IsReal(await leftValue.Value) || TypeHelper.IsReal(await rightValue.Value)
                    ? MathHelper.Divide(await leftValue.Value, await rightValue.Value, MathHelperOptions)
                    : MathHelper.Divide(Convert.ToDouble(await leftValue.Value, CultureInfo), await rightValue.Value,
                        MathHelperOptions);
                break;

            case BinaryExpressionType.Equal:
                Result = CompareUsingMostPreciseType(await leftValue.Value, await rightValue.Value) == 0;
                break;

            case BinaryExpressionType.Greater:
                Result = CompareUsingMostPreciseType(await leftValue.Value, await rightValue.Value) > 0;
                break;

            case BinaryExpressionType.GreaterOrEqual:
                Result = CompareUsingMostPreciseType(await leftValue.Value, await rightValue.Value) >= 0;
                break;

            case BinaryExpressionType.Lesser:
                Result = CompareUsingMostPreciseType(await leftValue.Value, await rightValue.Value) < 0;
                break;

            case BinaryExpressionType.LesserOrEqual:
                Result = CompareUsingMostPreciseType(await leftValue.Value, await rightValue.Value) <= 0;
                break;

            case BinaryExpressionType.Minus:
                Result = MathHelper.Subtract(await leftValue.Value, await rightValue.Value, MathHelperOptions);
                break;

            case BinaryExpressionType.Modulo:
                Result = MathHelper.Modulo(await leftValue.Value, await rightValue.Value, MathHelperOptions);
                break;

            case BinaryExpressionType.NotEqual:
                Result = CompareUsingMostPreciseType(await leftValue.Value, await rightValue.Value) != 0;
                break;

            case BinaryExpressionType.Plus:
                if (await leftValue.Value is string)
                {
                    Result = string.Concat(await leftValue.Value, await rightValue.Value);
                }
                else
                {
                    Result = MathHelper.Add(await leftValue.Value, await rightValue.Value, MathHelperOptions);
                }

                break;

            case BinaryExpressionType.Times:
                Result = MathHelper.Multiply(await leftValue.Value, await rightValue.Value, MathHelperOptions);
                break;

            case BinaryExpressionType.BitwiseAnd:
                Result = Convert.ToUInt16(await leftValue.Value, CultureInfo) &
                         Convert.ToUInt16(await rightValue.Value, CultureInfo);
                break;

            case BinaryExpressionType.BitwiseOr:
                Result = Convert.ToUInt16(await leftValue.Value, CultureInfo) |
                         Convert.ToUInt16(await rightValue.Value, CultureInfo);
                break;

            case BinaryExpressionType.BitwiseXOr:
                Result = Convert.ToUInt16(await leftValue.Value, CultureInfo) ^
                         Convert.ToUInt16(await rightValue.Value, CultureInfo);
                break;

            case BinaryExpressionType.LeftShift:
                Result = Convert.ToUInt16(await leftValue.Value, CultureInfo) <<
                         Convert.ToUInt16(await rightValue.Value, CultureInfo);
                break;

            case BinaryExpressionType.RightShift:
                Result = Convert.ToUInt16(await leftValue.Value, CultureInfo) >>
                         Convert.ToUInt16(await rightValue.Value, CultureInfo);
                break;

            case BinaryExpressionType.Exponentiation:
                Result = Math.Pow(Convert.ToDouble(await leftValue.Value, CultureInfo),
                    Convert.ToDouble(await rightValue.Value, CultureInfo));
                break;
        }
    }

    public async Task VisitAsync(UnaryExpression expression)
    {
        // Recursively evaluates the underlying expression
        await expression.Expression.AcceptAsync(this);

        ExecuteUnaryExpression(expression);
    }

    public async Task VisitAsync(Function function)
    {
        var args = new AsyncFunctionArgs
        {
            Parameters = new AsyncExpression[function.Expressions.Length]
        };

        // Don't call parameters right now, instead let the function do it as needed.
        // Some parameters shouldn't be called, for instance, in a if(), the "not" value might be a division by zero
        // Evaluating every value could produce unexpected behaviour
        for (var i = 0; i < function.Expressions.Length; i++)
        {
            args.Parameters[i] = new AsyncExpression(function.Expressions[i], Options, CultureInfo);
            args.Parameters[i].EvaluateFunctionAsync += EvaluateFunctionAsync;
            args.Parameters[i].EvaluateParameterAsync += EvaluateParameterAsync;

            // Assign the parameters of the Expression to the arguments so that custom Functions and Parameters can use them
            args.Parameters[i].Parameters = Parameters;
        }

        // Calls external implementation
        await OnEvaluateFunctionAsync(function.Identifier.Name, args);

        // If an external implementation was found get the result back
        if (args.HasResult)
        {
            Result = args.Result;
            return;
        }

        Result = await ExecuteBuiltInFunctionAsync(function);
    }

    private async Task<object?> ExecuteBuiltInFunctionAsync(Function function)
    {
        var functionName = function.Identifier.Name.ToUpperInvariant();


        switch (functionName)
        {
            case "ABS":
                CheckCase("Abs", function.Identifier.Name);
                if (function.Expressions.Length != 1)
                    throw new NCalcEvaluationException("Abs() takes exactly 1 argument");
                return MathHelper.Abs(await EvaluateAsync(function.Expressions[0]), MathHelperOptions);

            case "ACOS":
                CheckCase("Acos", function.Identifier.Name);
                if (function.Expressions.Length != 1)
                    throw new NCalcEvaluationException("Acos() takes exactly 1 argument");
                return MathHelper.Acos(await EvaluateAsync(function.Expressions[0]), MathHelperOptions);
            case "ASIN":
                CheckCase("Asin", function.Identifier.Name);
                if (function.Expressions.Length != 1)
                    throw new NCalcEvaluationException("Asin() takes exactly 1 argument");
                return MathHelper.Asin(await EvaluateAsync(function.Expressions[0]), MathHelperOptions);
            case "ATAN":
                CheckCase("Atan", function.Identifier.Name);
                if (function.Expressions.Length != 1)
                    throw new NCalcEvaluationException("Atan() takes exactly 1 argument");
                return MathHelper.Atan(await EvaluateAsync(function.Expressions[0]), MathHelperOptions);
            case "ATAN2":
                CheckCase("Atan2", function.Identifier.Name);
                if (function.Expressions.Length != 2)
                    throw new NCalcEvaluationException("Atan2() takes exactly 2 arguments");
                return MathHelper.Atan2(await EvaluateAsync(function.Expressions[0]),
                    await EvaluateAsync(function.Expressions[1]),
                    MathHelperOptions);
            case "CEILING":
                CheckCase("Ceiling", function.Identifier.Name);
                if (function.Expressions.Length != 1)
                    throw new NCalcEvaluationException("Ceiling() takes exactly 1 argument");
                return MathHelper.Ceiling(await EvaluateAsync(function.Expressions[0]), MathHelperOptions);
            case "COS":
                CheckCase("Cos", function.Identifier.Name);
                if (function.Expressions.Length != 1)
                    throw new NCalcEvaluationException("Cos() takes exactly 1 argument");
                return MathHelper.Cos(await EvaluateAsync(function.Expressions[0]), MathHelperOptions);
            case "EXP":
                CheckCase("Exp", function.Identifier.Name);
                if (function.Expressions.Length != 1)
                    throw new NCalcEvaluationException("Exp() takes exactly 1 argument");
                return MathHelper.Exp(await EvaluateAsync(function.Expressions[0]), MathHelperOptions);
            case "FLOOR":
                CheckCase("Floor", function.Identifier.Name);
                if (function.Expressions.Length != 1)
                    throw new NCalcEvaluationException("Floor() takes exactly 1 argument");
                return MathHelper.Floor(await EvaluateAsync(function.Expressions[0]), MathHelperOptions);

            case "IEEEREMAINDER":
                CheckCase("IEEERemainder", function.Identifier.Name);
                if (function.Expressions.Length != 2)
                    throw new NCalcEvaluationException("IEEERemainder() takes exactly 2 arguments");
                return MathHelper.IEEERemainder(await EvaluateAsync(function.Expressions[0]),
                    await EvaluateAsync(function.Expressions[1]),
                    MathHelperOptions);

            case "LN":
                CheckCase("Ln", function.Identifier.Name);
                if (function.Expressions.Length != 1)
                    throw new NCalcEvaluationException("Ln() takes exactly 1 argument");
                return MathHelper.Ln(await EvaluateAsync(function.Expressions[0]), MathHelperOptions);

            case "LOG":
                CheckCase("Log", function.Identifier.Name);
                if (function.Expressions.Length != 2)
                    throw new NCalcEvaluationException("Log() takes exactly 2 arguments");
                return MathHelper.Log(await EvaluateAsync(function.Expressions[0]),
                    await EvaluateAsync(function.Expressions[1]),
                    MathHelperOptions);
            case "LOG10":
                CheckCase("Log10", function.Identifier.Name);
                if (function.Expressions.Length != 1)
                    throw new NCalcEvaluationException("Log10() takes exactly 1 argument");
                return MathHelper.Log10(await EvaluateAsync(function.Expressions[0]), MathHelperOptions);
            case "POW":
                CheckCase("Pow", function.Identifier.Name);
                if (function.Expressions.Length != 2)
                    throw new NCalcEvaluationException("Pow() takes exactly 2 arguments");
                return MathHelper.Pow(await EvaluateAsync(function.Expressions[0]),
                    await EvaluateAsync(function.Expressions[1]),
                    MathHelperOptions);
            case "ROUND":
                CheckCase("Round", function.Identifier.Name);
                if (function.Expressions.Length != 2)
                    throw new NCalcEvaluationException("Round() takes exactly 2 arguments");

                var rounding = Options.HasFlag(ExpressionOptions.RoundAwayFromZero)
                    ? MidpointRounding.AwayFromZero
                    : MidpointRounding.ToEven;

                return MathHelper.Round(await EvaluateAsync(function.Expressions[0]),
                    await EvaluateAsync(function.Expressions[1]),
                    rounding, MathHelperOptions);
            case "SIGN":
                CheckCase("Sign", function.Identifier.Name);
                if (function.Expressions.Length != 1)
                    throw new NCalcEvaluationException("Sign() takes exactly 1 argument");
                return MathHelper.Sign(await EvaluateAsync(function.Expressions[0]), MathHelperOptions);
            case "SIN":
                CheckCase("Sin", function.Identifier.Name);
                if (function.Expressions.Length != 1)
                    throw new NCalcEvaluationException("Sin() takes exactly 1 argument");
                return MathHelper.Sin(await EvaluateAsync(function.Expressions[0]), MathHelperOptions);
            case "SQRT":
                CheckCase("Sqrt", function.Identifier.Name);
                if (function.Expressions.Length != 1)
                    throw new NCalcEvaluationException("Sqrt() takes exactly 1 argument");
                return MathHelper.Sqrt(await EvaluateAsync(function.Expressions[0]), MathHelperOptions);
            case "TAN":
                CheckCase("Tan", function.Identifier.Name);
                if (function.Expressions.Length != 1)
                    throw new NCalcEvaluationException("Tan() takes exactly 1 argument");
                return MathHelper.Tan(await EvaluateAsync(function.Expressions[0]), MathHelperOptions);
            case "TRUNCATE":
                CheckCase("Truncate", function.Identifier.Name);
                if (function.Expressions.Length != 1)
                    throw new NCalcEvaluationException("Truncate() takes exactly 1 argument");
                return MathHelper.Truncate(await EvaluateAsync(function.Expressions[0]), MathHelperOptions);
            case "MAX":
                CheckCase("Max", function.Identifier.Name);
                if (function.Expressions.Length != 2)
                    throw new NCalcEvaluationException("Max() takes exactly 2 arguments");
                var maxleft = await EvaluateAsync(function.Expressions[0]);
                var maxright = await EvaluateAsync(function.Expressions[1]);
                return MathHelper.Max(maxleft, maxright, MathHelperOptions);

            case "MIN":
                CheckCase("Min", function.Identifier.Name);
                if (function.Expressions.Length != 2)
                    throw new NCalcEvaluationException("Min() takes exactly 2 arguments");
                var minleft = await EvaluateAsync(function.Expressions[0]);
                var minright = await EvaluateAsync(function.Expressions[1]);
                return MathHelper.Min(minleft, minright, MathHelperOptions);

            case "IFS":
                CheckCase("ifs", function.Identifier.Name);
                if (function.Expressions.Length < 3 || function.Expressions.Length % 2 != 1)
                    throw new NCalcEvaluationException(
                        "ifs() takes at least 3 arguments, or an odd number of arguments");
                foreach (var eval in function.Expressions.Where((_, i) => i % 2 == 0))
                {
                    var index = Array.IndexOf(function.Expressions, eval);
                    var tf = Convert.ToBoolean(await EvaluateAsync(eval), CultureInfo);
                    if (index == function.Expressions.Length - 1)
                        return await EvaluateAsync(eval);
                    else if (tf)
                    {
                        return await EvaluateAsync(function.Expressions[index + 1]);
                    }
                }

                break;

            case "IF":
                CheckCase("if", function.Identifier.Name);
                if (function.Expressions.Length != 3)
                    throw new NCalcEvaluationException("if() takes exactly 3 arguments");
                var cond = Convert.ToBoolean(await EvaluateAsync(function.Expressions[0]), CultureInfo);
                return cond
                    ? await EvaluateAsync(function.Expressions[1])
                    : await EvaluateAsync(function.Expressions[2]);
            case "IN":
                CheckCase("in", function.Identifier.Name);
                if (function.Expressions.Length < 2)
                    throw new NCalcEvaluationException("in() takes at least 2 arguments");
                var parameter = EvaluateAsync(function.Expressions[0]);
                var evaluation = false;
                for (var i = 1; i < function.Expressions.Length; i++)
                {
                    var argument = await EvaluateAsync(function.Expressions[i]);
                    if (CompareUsingMostPreciseType(parameter, argument) == 0)
                    {
                        evaluation = true;
                        break;
                    }
                }

                return evaluation;
            default:
                throw new NCalcFunctionNotFoundException($"Function {function.Identifier.Name} not found",
                    function.Identifier.Name);
        }

        throw new InvalidOperationException();
    }

    private async Task<object?> EvaluateAsync(LogicalExpression expression)
    {
        await expression.AcceptAsync(this);
        return Result;
    }

    public async Task VisitAsync(Identifier identifier)
    {
        if (Parameters.TryGetValue(identifier.Name, out var parameterValue))
        {
            // The parameter is defined in the hashtable
            if (parameterValue is AsyncExpression expression)
            {
                // Overloads parameters 
                foreach (var p in Parameters)
                {
                    if (expression.Parameters != null)
                        expression.Parameters[p.Key] = p.Value;
                }

                expression.EvaluateFunctionAsync += EvaluateFunctionAsync;
                expression.EvaluateParameterAsync += EvaluateParameterAsync;

                Result = await expression.EvaluateAsync();
            }
            else
                Result = parameterValue;
        }
        else
        {
            // The parameter should be defined in a call back method
            var args = new AsyncParameterArgs();

            // Calls external implementation
            await OnEvaluateParameterAsync(identifier.Name, args);

            if (!args.HasResult)
                throw new NCalcParameterNotDefinedException(identifier.Name);

            Result = args.Result;
        }
    }

    public Task VisitAsync(ValueExpression expression)
    {
        Result = expression.Value;
        return Task.CompletedTask;
    }

    protected Task OnEvaluateFunctionAsync(string name, AsyncFunctionArgs args)
    {
        if (EvaluateFunctionAsync is not null)
        {
            return EvaluateFunctionAsync(name, args);
        }

        return Task.CompletedTask;
    }

    protected Task OnEvaluateParameterAsync(string name, AsyncParameterArgs args)
    {
        if (EvaluateParameterAsync is not null)
        {
            return EvaluateParameterAsync(name, args);
        }

        return Task.CompletedTask;
    }
}