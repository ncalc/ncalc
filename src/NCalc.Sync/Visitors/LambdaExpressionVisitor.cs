#nullable disable

using System.Reflection;
using NCalc.Domain;
using NCalc.Helpers;
using NCalc.Reflection;
using Linq = System.Linq.Expressions;
using LinqExpression = System.Linq.Expressions.Expression;
using LinqParameterExpression = System.Linq.Expressions.ParameterExpression;

namespace NCalc.Visitors;

public sealed class LambdaExpressionVistor : ILogicalExpressionVisitor
{
    private readonly Dictionary<string, object> _parameters;
    private readonly LinqExpression _context;
    private readonly ExpressionOptions _options;

    private bool OrdinalStringComparer => _options.HasFlag(ExpressionOptions.OrdinalStringComparer);
    private bool CaseInsensitiveStringComparer => _options.HasFlag(ExpressionOptions.CaseInsensitiveStringComparer);

    //TODO:
    private static bool Checked =>
        false; //{ get //{ return (_options & ExpressionOptions.OverflowProtection) == ExpressionOptions.OverflowProtection; } }

    // ReSharper disable once ConvertToPrimaryConstructor
    public LambdaExpressionVistor(Dictionary<string, object> parameters, ExpressionOptions options)
    {
        _parameters = parameters;
        _options = options;
    }

    // ReSharper disable once SuggestBaseTypeForParameterInConstructor
    public LambdaExpressionVistor(LinqParameterExpression context, ExpressionOptions options)
    {
        _context = context;
        _options = options;
    }

    public LinqExpression Result { get; private set; }

    public void Visit(TernaryExpression expression)
    {
        expression.LeftExpression.Accept(this);
        var conditional = Result;

        expression.MiddleExpression.Accept(this);
        var ifTrue = Result;

        expression.RightExpression.Accept(this);
        var ifFalse = Result;

        Result = LinqExpression.Condition(conditional, ifTrue, ifFalse);
    }

    public void Visit(BinaryExpression expression)
    {
        expression.LeftExpression.Accept(this);
        var left = Result;

        expression.RightExpression.Accept(this);
        var right = Result;

        switch (expression.Type)
        {
            case BinaryExpressionType.And:
                Result = LinqExpression.AndAlso(left, right);
                break;
            case BinaryExpressionType.Or:
                Result = LinqExpression.OrElse(left, right);
                break;
            case BinaryExpressionType.NotEqual:
                Result = WithCommonNumericType(left, right, LinqExpression.NotEqual, expression.Type);
                break;
            case BinaryExpressionType.LesserOrEqual:
                Result = WithCommonNumericType(left, right, LinqExpression.LessThanOrEqual, expression.Type);
                break;
            case BinaryExpressionType.GreaterOrEqual:
                Result = WithCommonNumericType(left, right, LinqExpression.GreaterThanOrEqual, expression.Type);
                break;
            case BinaryExpressionType.Lesser:
                Result = WithCommonNumericType(left, right, LinqExpression.LessThan, expression.Type);
                break;
            case BinaryExpressionType.Greater:
                Result = WithCommonNumericType(left, right, LinqExpression.GreaterThan, expression.Type);
                break;
            case BinaryExpressionType.Equal:
                Result = WithCommonNumericType(left, right, LinqExpression.Equal, expression.Type);
                break;
            case BinaryExpressionType.Minus:
                if (Checked) Result = WithCommonNumericType(left, right, LinqExpression.SubtractChecked);
                else Result = WithCommonNumericType(left, right, LinqExpression.Subtract);
                break;
            case BinaryExpressionType.Plus:
                if (Checked) Result = WithCommonNumericType(left, right, LinqExpression.AddChecked);
                else Result = WithCommonNumericType(left, right, LinqExpression.Add);
                break;
            case BinaryExpressionType.Modulo:
                Result = WithCommonNumericType(left, right, LinqExpression.Modulo);
                break;
            case BinaryExpressionType.Div:
                Result = WithCommonNumericType(left, right, LinqExpression.Divide);
                break;
            case BinaryExpressionType.Times:
                if (Checked) Result = WithCommonNumericType(left, right, LinqExpression.MultiplyChecked);
                else Result = WithCommonNumericType(left, right, LinqExpression.Multiply);
                break;
            case BinaryExpressionType.BitwiseOr:
                Result = LinqExpression.Or(left, right);
                break;
            case BinaryExpressionType.BitwiseAnd:
                Result = LinqExpression.And(left, right);
                break;
            case BinaryExpressionType.BitwiseXOr:
                Result = LinqExpression.ExclusiveOr(left, right);
                break;
            case BinaryExpressionType.LeftShift:
                Result = LinqExpression.LeftShift(left, right);
                break;
            case BinaryExpressionType.RightShift:
                Result = LinqExpression.RightShift(left, right);
                break;
            case BinaryExpressionType.Exponentiation:
                Result = LinqExpression.Power(left, right);
                break;
            case BinaryExpressionType.Unknown:
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void Visit(UnaryExpression expression)
    {
        expression.Expression.Accept(this);
        Result = expression.Type switch
        {
            UnaryExpressionType.Not => LinqExpression.Not(Result),
            UnaryExpressionType.Negate => LinqExpression.Negate(Result),
            UnaryExpressionType.BitwiseNot => LinqExpression.Not(Result),
            UnaryExpressionType.Positive => Result,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public void Visit(ValueExpression expression)
    {
        Result = LinqExpression.Constant(expression.Value);
    }

    public void Visit(Function function)
    {
        var args = new LinqExpression[function.Expressions.Length];
        for (var i = 0; i < function.Expressions.Length; i++)
        {
            function.Expressions[i].Accept(this);
            args[i] = Result;
        }

        var functionName = function.Identifier.Name.ToUpperInvariant();
        if (functionName == "IF")
        {
            var numberTypePriority = new[] { typeof(double), typeof(float), typeof(long), typeof(int), typeof(short) };
            var index1 = Array.IndexOf(numberTypePriority, args[1].Type);
            var index2 = Array.IndexOf(numberTypePriority, args[2].Type);
            if (index1 >= 0 && index2 >= 0 && index1 != index2)
            {
                args[1] = LinqExpression.Convert(args[1], numberTypePriority[Math.Min(index1, index2)]);
                args[2] = LinqExpression.Convert(args[2], numberTypePriority[Math.Min(index1, index2)]);
            }

            Result = LinqExpression.Condition(args[0], args[1], args[2]);
            return;
        }

        if (functionName == "IN")
        {
            var items = LinqExpression.NewArrayInit(args[0].Type,
                new ArraySegment<LinqExpression>(args, 1, args.Length - 1));
            var smi = typeof(Array).GetRuntimeMethod("IndexOf", [typeof(Array), typeof(object)]);
            var r = LinqExpression.Call(smi, LinqExpression.Convert(items, typeof(Array)),
                LinqExpression.Convert(args[0], typeof(object)));
            Result = LinqExpression.GreaterThanOrEqual(r, LinqExpression.Constant(0));
            return;
        }

        //Context methods take precedence over built-in functions because they're user-customisable.
        var mi = FindMethod(function.Identifier.Name, args);
        if (mi != null)
        {
            Result = LinqExpression.Call(_context, mi.MethodInfo, mi.PreparedArguments);
            return;
        }

        Linq.UnaryExpression arg0;
        Linq.UnaryExpression arg1;

        var actualNumArgs = function.Expressions.Length;

        switch (functionName)
        {
            // Exceptional handling
            case "MAX":
                CheckArgumentsLengthForFunction(functionName, function.Expressions.Length, 2);
                arg0 = LinqExpression.Convert(args[0], typeof(double));
                arg1 = LinqExpression.Convert(args[1], typeof(double));
                Result = LinqExpression.Condition(LinqExpression.GreaterThan(arg0, arg1), arg0, arg1);
                break;
            case "MIN":
                CheckArgumentsLengthForFunction(functionName, function.Expressions.Length, 2);
                arg0 = LinqExpression.Convert(args[0], typeof(double));
                arg1 = LinqExpression.Convert(args[1], typeof(double));
                Result = LinqExpression.Condition(LinqExpression.LessThan(arg0, arg1), arg0, arg1);
                break;
            case "POW":
                CheckArgumentsLengthForFunction(functionName, function.Expressions.Length, 2);
                arg0 = LinqExpression.Convert(args[0], typeof(double));
                arg1 = LinqExpression.Convert(args[1], typeof(double));
                Result = LinqExpression.Power(arg0, arg1);
                break;
            case "ROUND":
                CheckArgumentsLengthForFunction(functionName, function.Expressions.Length, 2);
                arg0 = LinqExpression.Convert(args[0], typeof(double));
                arg1 = LinqExpression.Convert(args[1], typeof(int));
                var rounding = (_options & ExpressionOptions.RoundAwayFromZero) == ExpressionOptions.RoundAwayFromZero
                    ? MidpointRounding.AwayFromZero
                    : MidpointRounding.ToEven;
                Result = LinqExpression.Call(MathFunctionHelper.Functions["ROUND"].MethodInfo, arg0, arg1,
                    LinqExpression.Constant(rounding));
                break;
            default:
                // Regular handling
                if (MathFunctionHelper.Functions.TryGetValue(functionName, out var func))
                {
                    MakeMathCallExpression(func, actualNumArgs);
                }
                else
                {
                    throw new MissingMethodException($"method not found: {functionName}");
                }

                break;
        }

        return;

        static void CheckArgumentsLengthForFunction(string funcStr, int argsNum, int argsNeed)
        {
            if (argsNum != argsNeed)
                throw new ArgumentException($"{funcStr} takes exactly {argsNeed} argument");
        }

        void MakeMathCallExpression(MathFunctionHelper.MathMethodInfo mathMethod, int argsNumActual)
        {
            CheckArgumentsLengthForFunction(mathMethod.MethodInfo.Name, argsNumActual, mathMethod.ArgumentCount);

            Result = LinqExpression.Call(mathMethod.MethodInfo,
                Enumerable.Range(0, argsNumActual).Select(i => LinqExpression.Convert(args[i], typeof(double))));
        }
    }

    public void Visit(Identifier function)
    {
        if (_context == null)
        {
            Result = LinqExpression.Constant(_parameters[function.Name]);
        }
        else
        {
            Result = LinqExpression.PropertyOrField(_context, function.Name);
        }
    }

    private ExtendedMethodInfo FindMethod(string methodName, LinqExpression[] methodArgs)
    {
        if (_context == null)
            return null;

        var contextTypeInfo = _context.Type.GetTypeInfo();
        var objectTypeInfo = typeof(object).GetTypeInfo();
        do
        {
            var methods = contextTypeInfo.DeclaredMethods.Where(m =>
                m.Name.Equals(methodName, StringComparison.OrdinalIgnoreCase) && m.IsPublic && !m.IsStatic);
            var candidates = new List<ExtendedMethodInfo>();
            foreach (var potentialMethod in methods)
            {
                var methodParams = potentialMethod.GetParameters();
                var preparedArguments = LinqUtils.PrepareMethodArgumentsIfValid(methodParams, methodArgs);

                if (preparedArguments == null)
                    continue;

                var candidate = new ExtendedMethodInfo
                {
                    MethodInfo = potentialMethod,
                    PreparedArguments = preparedArguments.Item2,
                    Score = preparedArguments.Item1
                };

                if (candidate.Score == 0)
                    return candidate;
                candidates.Add(candidate);
            }

            if (candidates.Count != 0)
                return candidates.OrderBy(method => method.Score).First();
            contextTypeInfo = contextTypeInfo.BaseType.GetTypeInfo();
        } while (contextTypeInfo != objectTypeInfo);

        return null;
    }

    private LinqExpression WithCommonNumericType(LinqExpression left, LinqExpression right,
        Func<LinqExpression, LinqExpression, LinqExpression> action,
        BinaryExpressionType expressiontype = BinaryExpressionType.Unknown)
    {
        left = LinqUtils.UnwrapNullable(left);
        right = LinqUtils.UnwrapNullable(right);

        if (_options.HasFlag(ExpressionOptions.AllowBooleanCalculation))
        {
            if (left.Type == typeof(bool))
            {
                left = LinqExpression.Condition(left, LinqExpression.Constant(1.0), LinqExpression.Constant(0.0));
            }

            if (right.Type == typeof(bool))
            {
                right = LinqExpression.Condition(right, LinqExpression.Constant(1.0), LinqExpression.Constant(0.0));
            }
        }

        var type = TypeHelper.GetMostPreciseNumberType(left.Type, right.Type);
        if (type != null)
        {
            if (left.Type != type)
            {
                left = LinqExpression.Convert(left, type);
            }

            if (right.Type != type)
            {
                right = LinqExpression.Convert(right, type);
            }
        }

        LinqExpression comparer;
        if (CaseInsensitiveStringComparer)
        {
            comparer = LinqExpression.Property(null, typeof(StringComparer),
                OrdinalStringComparer ? "OrdinalIgnoreCase" : "CurrentCultureIgnoreCase");
        }
        else
            comparer = LinqExpression.Property(null, typeof(StringComparer), "Ordinal");

        if (typeof(string) != left.Type && typeof(string) != right.Type)
            return action(left, right);
        
        switch (expressiontype)
        {
            case BinaryExpressionType.Equal:
                return LinqExpression.Call(comparer,
                    typeof(StringComparer).GetRuntimeMethod("Equals", [typeof(string), typeof(string)]),
                    [left, right]);
            case BinaryExpressionType.NotEqual:
                return LinqExpression.Not(LinqExpression.Call(comparer,
                    typeof(StringComparer).GetRuntimeMethod("Equals", [typeof(string), typeof(string)]),
                    [left, right]));
            case BinaryExpressionType.GreaterOrEqual:
                return LinqExpression.GreaterThanOrEqual(
                    LinqExpression.Call(comparer,
                        typeof(StringComparer).GetRuntimeMethod("Compare",
                            [typeof(string), typeof(string)]), [left, right]),
                    LinqExpression.Constant(0));
            case BinaryExpressionType.LesserOrEqual:
                return LinqExpression.LessThanOrEqual(
                    LinqExpression.Call(comparer,
                        typeof(StringComparer).GetRuntimeMethod("Compare",
                            [typeof(string), typeof(string)]), [left, right]),
                    LinqExpression.Constant(0));
            case BinaryExpressionType.Greater:
                return LinqExpression.GreaterThan(
                    LinqExpression.Call(comparer,
                        typeof(StringComparer).GetRuntimeMethod("Compare",
                            [typeof(string), typeof(string)]), [left, right]),
                    LinqExpression.Constant(0));
            case BinaryExpressionType.Lesser:
                return LinqExpression.LessThan(
                    LinqExpression.Call(comparer,
                        typeof(StringComparer).GetRuntimeMethod("Compare",
                            [typeof(string), typeof(string)]), [left, right]),
                    LinqExpression.Constant(0));
        }

        return action(left, right);
    }
}