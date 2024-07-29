#nullable disable

using System.Reflection;
using NCalc.Domain;
using NCalc.Helpers;
using NCalc.Reflection;
using Linq = System.Linq.Expressions;
using LinqExpression = System.Linq.Expressions.Expression;
using LinqParameterExpression = System.Linq.Expressions.ParameterExpression;

namespace NCalc.Visitors;

public sealed class LambdaExpressionVisitor : ILogicalExpressionVisitor<LinqExpression>
{
    private readonly IDictionary<string, object> _parameters;
    private readonly LinqExpression _context;
    private readonly ExpressionOptions _options;
    private readonly bool _ordinalStringComparer;
    private readonly bool _caseInsensitiveStringComparer;
    private readonly bool _checked;

    private LambdaExpressionVisitor(ExpressionOptions options)
    {
        _options = options;
        _ordinalStringComparer = _options.HasFlag(ExpressionOptions.OrdinalStringComparer);
        _checked = _options.HasFlag(ExpressionOptions.OverflowProtection);
        _caseInsensitiveStringComparer = _options.HasFlag(ExpressionOptions.CaseInsensitiveStringComparer);
    }

    public LambdaExpressionVisitor(IDictionary<string, object> parameters, ExpressionOptions options) : this(options)
    {
        _parameters = parameters;
    }

    public LambdaExpressionVisitor(LinqParameterExpression context, ExpressionOptions options) : this(options)
    {
        _context = context;
    }

    public LinqExpression Visit(TernaryExpression expression)
    {
        var conditional = expression.LeftExpression.Accept(this);
        var ifTrue = expression.MiddleExpression.Accept(this);
        var ifFalse = expression.RightExpression.Accept(this);

        return LinqExpression.Condition(conditional, ifTrue, ifFalse);
    }

    public LinqExpression Visit(BinaryExpression expression)
    {
        var left = expression.LeftExpression.Accept(this);
        var right = expression.RightExpression.Accept(this);

        return expression.Type switch
        {
            BinaryExpressionType.And => LinqExpression.AndAlso(left, right),
            BinaryExpressionType.Or => LinqExpression.OrElse(left, right),
            BinaryExpressionType.NotEqual => WithCommonNumericType(left, right, LinqExpression.NotEqual, expression.Type),
            BinaryExpressionType.LesserOrEqual => WithCommonNumericType(left, right, LinqExpression.LessThanOrEqual, expression.Type),
            BinaryExpressionType.GreaterOrEqual => WithCommonNumericType(left, right, LinqExpression.GreaterThanOrEqual, expression.Type),
            BinaryExpressionType.Lesser => WithCommonNumericType(left, right, LinqExpression.LessThan, expression.Type),
            BinaryExpressionType.Greater => WithCommonNumericType(left, right, LinqExpression.GreaterThan, expression.Type),
            BinaryExpressionType.Equal => WithCommonNumericType(left, right, LinqExpression.Equal, expression.Type),
            BinaryExpressionType.Minus => _checked ? WithCommonNumericType(left, right, LinqExpression.SubtractChecked) : WithCommonNumericType(left, right, LinqExpression.Subtract),
            BinaryExpressionType.Plus => _checked ? WithCommonNumericType(left, right, LinqExpression.AddChecked) : WithCommonNumericType(left, right, LinqExpression.Add),
            BinaryExpressionType.Modulo => WithCommonNumericType(left, right, LinqExpression.Modulo),
            BinaryExpressionType.Div => WithCommonNumericType(left, right, LinqExpression.Divide),
            BinaryExpressionType.Times => _checked ? WithCommonNumericType(left, right, LinqExpression.MultiplyChecked) : WithCommonNumericType(left, right, LinqExpression.Multiply),
            BinaryExpressionType.BitwiseOr => LinqExpression.Or(left, right),
            BinaryExpressionType.BitwiseAnd => LinqExpression.And(left, right),
            BinaryExpressionType.BitwiseXOr => LinqExpression.ExclusiveOr(left, right),
            BinaryExpressionType.LeftShift => LinqExpression.LeftShift(left, right),
            BinaryExpressionType.RightShift => LinqExpression.RightShift(left, right),
            BinaryExpressionType.Exponentiation => LinqExpression.Power(left, right),
            BinaryExpressionType.Unknown => throw new ArgumentOutOfRangeException(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public LinqExpression Visit(UnaryExpression expression)
    {
        var operand = expression.Expression.Accept(this);

        return expression.Type switch
        {
            UnaryExpressionType.Not => LinqExpression.Not(operand),
            UnaryExpressionType.Negate => LinqExpression.Negate(operand),
            UnaryExpressionType.BitwiseNot => LinqExpression.Not(operand),
            UnaryExpressionType.Positive => operand,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public LinqExpression Visit(ValueExpression expression)
    {
        return LinqExpression.Constant(expression.Value);
    }

    public LinqExpression Visit(Function function)
    {
        var args = new LinqExpression[function.Parameters.Count];
        for (var i = 0; i < function.Parameters.Count; i++)
        {
            args[i] = function.Parameters[i].Accept(this);
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

            return LinqExpression.Condition(args[0], args[1], args[2]);
        }

        if (functionName == "IN")
        {
            var items = LinqExpression.NewArrayInit(args[0].Type,
                new ArraySegment<LinqExpression>(args, 1, args.Length - 1));
            var smi = typeof(Array).GetRuntimeMethod("IndexOf", [typeof(Array), typeof(object)]);
            var r = LinqExpression.Call(smi, LinqExpression.Convert(items, typeof(Array)),
                LinqExpression.Convert(args[0], typeof(object)));
            return LinqExpression.GreaterThanOrEqual(r, LinqExpression.Constant(0));
        }

        //Context methods take precedence over built-in functions because they're user-customisable.
        var mi = FindMethod(function.Identifier.Name, args);
        if (mi != null)
        {
            return LinqExpression.Call(_context, mi.MethodInfo, mi.PreparedArguments);
        }

        Linq.UnaryExpression arg0;
        Linq.UnaryExpression arg1;

        var actualNumArgs = function.Parameters.Count;

        switch (functionName)
        {
            // Exceptional handling
            case "MAX":
                CheckArgumentsLengthForFunction(functionName, function.Parameters.Count, 2);
                arg0 = LinqExpression.Convert(args[0], typeof(double));
                arg1 = LinqExpression.Convert(args[1], typeof(double));
                return LinqExpression.Condition(LinqExpression.GreaterThan(arg0, arg1), arg0, arg1);
            case "MIN":
                CheckArgumentsLengthForFunction(functionName, function.Parameters.Count, 2);
                arg0 = LinqExpression.Convert(args[0], typeof(double));
                arg1 = LinqExpression.Convert(args[1], typeof(double));
                return LinqExpression.Condition(LinqExpression.LessThan(arg0, arg1), arg0, arg1);
            case "POW":
                CheckArgumentsLengthForFunction(functionName, function.Parameters.Count, 2);
                arg0 = LinqExpression.Convert(args[0], typeof(double));
                arg1 = LinqExpression.Convert(args[1], typeof(double));
                return LinqExpression.Power(arg0, arg1);
            case "ROUND":
                CheckArgumentsLengthForFunction(functionName, function.Parameters.Count, 2);
                arg0 = LinqExpression.Convert(args[0], typeof(double));
                arg1 = LinqExpression.Convert(args[1], typeof(int));
                var rounding = (_options & ExpressionOptions.RoundAwayFromZero) == ExpressionOptions.RoundAwayFromZero
                    ? MidpointRounding.AwayFromZero
                    : MidpointRounding.ToEven;
                return LinqExpression.Call(MathFunctionHelper.Functions["ROUND"].MethodInfo, arg0, arg1,
                    LinqExpression.Constant(rounding));

            default:
                // Regular handling
                if (MathFunctionHelper.Functions.TryGetValue(functionName, out var func))
                {
                    return MakeMathCallExpression(func, actualNumArgs);
                }

                throw new MissingMethodException($"method not found: {functionName}");
        }

        static void CheckArgumentsLengthForFunction(string funcStr, int argsNum, int argsNeed)
        {
            if (argsNum != argsNeed)
                throw new ArgumentException($"{funcStr} takes exactly {argsNeed} argument");
        }

        LinqExpression MakeMathCallExpression(MathMethodInfo mathMethod, int argsNumActual)
        {
            CheckArgumentsLengthForFunction(mathMethod.MethodInfo.Name, argsNumActual, mathMethod.ArgumentCount);

            return LinqExpression.Call(mathMethod.MethodInfo,
                Enumerable.Range(0, argsNumActual).Select(i => LinqExpression.Convert(args[i], typeof(double))));
        }
    }

    public LinqExpression Visit(Identifier function)
    {
        if (_context == null)
        {
            return LinqExpression.Constant(_parameters[function.Name]);
        }

        return LinqExpression.PropertyOrField(_context, function.Name);
    }

    public LinqExpression Visit(LogicalExpressionList list)
    {
        throw new NotSupportedException("Collections are not supported for Lambda expressions yet. Please open a issue at https://www.github.com/ncalc/ncalc if you want this support.");
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
        if (_caseInsensitiveStringComparer)
        {
            comparer = LinqExpression.Property(null, typeof(StringComparer),
                _ordinalStringComparer ? "OrdinalIgnoreCase" : "CurrentCultureIgnoreCase");
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