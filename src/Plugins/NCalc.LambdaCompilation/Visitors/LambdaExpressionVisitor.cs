using System.Numerics;
using System.Reflection;
using System.Text.RegularExpressions;
using ExtendedNumerics;
using NCalc.Domain;
using NCalc.Exceptions;
using NCalc.Helpers;
using NCalc.LambdaCompilation.Reflection;
using NCalc.Visitors;
using Linq = System.Linq.Expressions;
using LinqExpression = System.Linq.Expressions.Expression;
using LinqParameterExpression = System.Linq.Expressions.ParameterExpression;

namespace NCalc.LambdaCompilation.Visitors;

public sealed class LambdaExpressionVisitor : ILogicalExpressionVisitor<LinqExpression>
{
    private readonly IDictionary<string, object?>? _parameters;
    private readonly LinqExpression? _context;
    private readonly ExpressionOptions _options;
    private readonly bool _ordinalStringComparer;
    private readonly bool _caseInsensitiveStringComparer;
    private readonly bool _ignoreCaseAtBuiltInFunctions;
    private readonly bool _checked;

    private static readonly MethodInfo StringComparerEqualsMethod =
        typeof(StringComparer).GetMethod("Equals", [typeof(string), typeof(string)])!;

    private static readonly MethodInfo StringComparerCompareMethod =
        typeof(StringComparer).GetMethod("Compare", [typeof(string), typeof(string)])!;

    private LambdaExpressionVisitor(ExpressionOptions options)
    {
        _options = options;
        _ordinalStringComparer = _options.HasFlag(ExpressionOptions.OrdinalStringComparer);
        _checked = _options.HasFlag(ExpressionOptions.OverflowProtection);
        _caseInsensitiveStringComparer = _options.HasFlag(ExpressionOptions.CaseInsensitiveStringComparer);
        _ignoreCaseAtBuiltInFunctions = _options.HasFlag(ExpressionOptions.IgnoreCaseAtBuiltInFunctions);
    }

    public LambdaExpressionVisitor(IDictionary<string, object?> parameters, ExpressionOptions options) : this(options)
    {
        _parameters = parameters;
    }

    public LambdaExpressionVisitor(LinqParameterExpression context, ExpressionOptions options) : this(options)
    {
        _context = context;
    }

    public LinqExpression Visit(TernaryExpression expression, CancellationToken ct = default)
    {
        var conditional = expression.LeftExpression.Accept(this, ct);
        var ifTrue = expression.MiddleExpression.Accept(this, ct);
        var ifFalse = expression.RightExpression.Accept(this, ct);

        return LinqExpression.Condition(conditional, ifTrue, ifFalse);
    }

    public LinqExpression Visit(BinaryExpression expression, CancellationToken ct = default)
    {
        var left = expression.LeftExpression.Accept(this, ct);
        var right = expression.RightExpression.Accept(this, ct);

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
            BinaryExpressionType.Like => LikeOperator(left, right),
            BinaryExpressionType.NotLike => LinqExpression.Not(LikeOperator(left, right)),
            BinaryExpressionType.In => InOperator(left, right),
            BinaryExpressionType.NotIn => LinqExpression.Not(InOperator(left, right)),
            BinaryExpressionType.Unknown => throw new ArgumentOutOfRangeException(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public LinqExpression Visit(UnaryExpression expression, CancellationToken ct = default)
    {
        var operand = expression.Expression.Accept(this, ct);

        return expression.Type switch
        {
            UnaryExpressionType.Not => LinqExpression.Not(operand),
            UnaryExpressionType.Negate => LinqExpression.Negate(operand),
            UnaryExpressionType.BitwiseNot => LinqExpression.Not(operand),
            UnaryExpressionType.Positive => operand,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public LinqExpression Visit(ValueExpression expression, CancellationToken ct = default)
    {
        return LinqExpression.Constant(expression.Value);
    }

    public LinqExpression Visit(Function function, CancellationToken ct = default)
    {
        var args = new LinqExpression[function.Parameters.Count];
        for (var i = 0; i < function.Parameters.Count; i++)
        {
            args[i] = function.Parameters[i].Accept(this, ct);
        }

        // Context methods take precedence over built-in functions because they're user-customizable.
        var mi = FindMethod(function.Identifier.Name, args);
        if (mi != null)
            return LinqExpression.Call(_context, mi.MethodInfo, mi.PreparedArguments);

        Linq.UnaryExpression arg0;
        Linq.UnaryExpression arg1;

        var comparisonType = _ignoreCaseAtBuiltInFunctions ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        var functionName = function.Identifier.Name;

        switch (functionName)
        {
            // Exceptional handling
            case var s when string.Equals(s, "Max", comparisonType):
                CheckArgumentsLengthForFunction(functionName, function.Parameters.Count, 2);
                arg0 = LinqExpression.Convert(args[0], typeof(double));
                arg1 = LinqExpression.Convert(args[1], typeof(double));
                return LinqExpression.Condition(LinqExpression.GreaterThan(arg0, arg1), arg0, arg1);
            case var s when string.Equals(s, "Min", comparisonType):
                CheckArgumentsLengthForFunction(functionName, function.Parameters.Count, 2);
                arg0 = LinqExpression.Convert(args[0], typeof(double));
                arg1 = LinqExpression.Convert(args[1], typeof(double));
                return LinqExpression.Condition(LinqExpression.LessThan(arg0, arg1), arg0, arg1);
            case var s when string.Equals(s, "Pow", comparisonType):
                CheckArgumentsLengthForFunction(functionName, function.Parameters.Count, 2);

                if (_options == ExpressionOptions.DecimalAsDefault)
                {
                    arg0 = LinqExpression.Convert(args[0], typeof(decimal));
                    arg1 = LinqExpression.Convert(args[1], typeof(decimal));

                    var @base = LinqExpression.Convert(arg0, typeof(BigDecimal));
                    var exponent = LinqExpression.Convert(arg1, typeof(BigInteger));

                    var methodInfo = typeof(BigDecimal).GetMethod("Pow", [typeof(BigDecimal), typeof(BigInteger)]);
                    if (methodInfo != null)
                    {
                        var result = LinqExpression.Call(methodInfo, @base, exponent);
                        return LinqExpression.Convert(result, typeof(decimal));
                    }
                }

                arg0 = LinqExpression.Convert(args[0], typeof(double));
                arg1 = LinqExpression.Convert(args[1], typeof(double));

                return LinqExpression.Power(arg0, arg1);
            case var s when string.Equals(s, "Round", comparisonType):
                CheckArgumentsLengthForFunction(functionName, function.Parameters.Count, 2);

                if (_options == ExpressionOptions.DecimalAsDefault)
                    arg0 = LinqExpression.Convert(args[0], typeof(decimal));
                else
                    arg0 = LinqExpression.Convert(args[0], typeof(double));

                arg1 = LinqExpression.Convert(args[1], typeof(int));

                var rounding = (_options & ExpressionOptions.RoundAwayFromZero) == ExpressionOptions.RoundAwayFromZero
                    ? MidpointRounding.AwayFromZero
                    : MidpointRounding.ToEven;
                return LinqExpression.Call(MathFunctionHelper.Functions["Round"].First().MethodInfo, arg0, arg1,
                    LinqExpression.Constant(rounding));
            case var s when string.Equals(s, "if", comparisonType):
                var numberTypePriority = new[] { typeof(double), typeof(float), typeof(long), typeof(int), typeof(short) };
                var index1 = Array.IndexOf(numberTypePriority, args[1].Type);
                var index2 = Array.IndexOf(numberTypePriority, args[2].Type);
                if (index1 >= 0 && index2 >= 0 && index1 != index2)
                {
                    args[1] = LinqExpression.Convert(args[1], numberTypePriority[Math.Min(index1, index2)]);
                    args[2] = LinqExpression.Convert(args[2], numberTypePriority[Math.Min(index1, index2)]);
                }

                return LinqExpression.Condition(args[0], args[1], args[2]);

            case var s when string.Equals(s, "in", comparisonType):
                var items = LinqExpression.NewArrayInit(args[0].Type,
                    new ArraySegment<LinqExpression>(args, 1, args.Length - 1));
                var smi = typeof(Array).GetMethod("IndexOf", [typeof(Array), typeof(object)]);
                var r = LinqExpression.Call(smi!, LinqExpression.Convert(items, typeof(Array)),
                    LinqExpression.Convert(args[0], typeof(object)));
                return LinqExpression.GreaterThanOrEqual(r, LinqExpression.Constant(0));

            default:
                // Regular handling
                var kvp = MathFunctionHelper.Functions
                    .FirstOrDefault(_ => string.Equals(functionName, _.Key, comparisonType));

                if (kvp.Key != null)
                {
                    MathMethodInfo func;
                    var f = kvp.Value;

                    if (_options == ExpressionOptions.DecimalAsDefault && f.Any(_ => _.DecimalSupport))
                        func = f.First(_ => _.DecimalSupport);
                    else
                        func = f.First(_ => !_.DecimalSupport);

                    CheckArgumentsLengthForFunction(functionName, args.Length, func.ArgumentCount);

                    var arguments = new List<LinqExpression>();

                    var parameters = func.MethodInfo.GetParameters();
                    for (int i = 0; i < parameters.Length; i++)
                        arguments.Add(LinqExpression.Convert(args[i], parameters[i].ParameterType));

                    return LinqExpression.Call(func.MethodInfo, arguments);
                }

                throw new MissingMethodException($"method not found: {functionName}");
        }

        static void CheckArgumentsLengthForFunction(string funcStr, int argsNum, int argsNeed)
        {
            if (argsNum != argsNeed)
                throw new ArgumentException($"{funcStr} takes exactly {argsNeed} argument");
        }
    }

    public LinqExpression Visit(Identifier identifier, CancellationToken ct = default)
    {
        var identifierName = identifier.Name;

        if (_context == null)
        {
            if (_parameters != null && _parameters.TryGetValue(identifierName, out var param))
                return LinqExpression.Constant(param);

            throw new NCalcParameterNotDefinedException(identifierName);
        }

        return LinqExpression.PropertyOrField(_context, identifierName);
    }

    public LinqExpression Visit(LogicalExpressionList list, CancellationToken ct = default)
    {
        var newList = LinqExpression.New(typeof(List<object>));
        return LinqExpression.ListInit(newList,
            list.Select(e => LinqExpression.Convert(e.Accept(this, ct), typeof(object)))
        );
    }

    private ExtendedMethodInfo? FindMethod(string methodName, LinqExpression[] methodArgs)
    {
        if (_context == null)
            return null;

        var contextType = _context.Type;
        var objectType = typeof(object);

        do
        {
            var methods = contextType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m => m.Name.Equals(methodName, StringComparison.OrdinalIgnoreCase));

            var candidates = new List<ExtendedMethodInfo>();

            foreach (var potentialMethod in methods)
            {
                var methodParams = potentialMethod.GetParameters();
                var preparedArguments = LinqUtils.PrepareMethodArgumentsIfValid(methodParams, methodArgs);

                if (preparedArguments != null)
                {
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
            }

            if (candidates.Count != 0)
                return candidates.OrderBy(method => method.Score).First();

            contextType = contextType.BaseType;
        }
        while (contextType != null && contextType != objectType);

        return null;
    }

    private LinqExpression WithCommonNumericType(LinqExpression left, LinqExpression right,
        Func<LinqExpression, LinqExpression, LinqExpression> action,
        BinaryExpressionType expressionType = BinaryExpressionType.Unknown)
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

        if (typeof(string) != left.Type && typeof(string) != right.Type)
            return action(left, right);

        LinqExpression comparer;
        if (_caseInsensitiveStringComparer)
        {
            if (_ordinalStringComparer)
                comparer = LinqExpression.Constant(StringComparer.OrdinalIgnoreCase);
            else
                comparer = LinqExpression.Constant(StringComparer.CurrentCultureIgnoreCase);
        }
        else
            comparer = LinqExpression.Constant(StringComparer.Ordinal);

        switch (expressionType)
        {
            case BinaryExpressionType.Equal:
                return LinqExpression.Call(comparer, StringComparerEqualsMethod, [left, right]);
            case BinaryExpressionType.NotEqual:
                return LinqExpression.Not(
                    LinqExpression.Call(comparer, StringComparerEqualsMethod, [left, right]));
            case BinaryExpressionType.GreaterOrEqual:
                return LinqExpression.GreaterThanOrEqual(
                    LinqExpression.Call(comparer, StringComparerCompareMethod, [left, right]),
                    LinqExpression.Constant(0));
            case BinaryExpressionType.LesserOrEqual:
                return LinqExpression.LessThanOrEqual(
                    LinqExpression.Call(comparer, StringComparerCompareMethod, [left, right]),
                    LinqExpression.Constant(0));
            case BinaryExpressionType.Greater:
                return LinqExpression.GreaterThan(
                    LinqExpression.Call(comparer, StringComparerCompareMethod, [left, right]),
                    LinqExpression.Constant(0));
            case BinaryExpressionType.Lesser:
                return LinqExpression.LessThan(
                    LinqExpression.Call(comparer, StringComparerCompareMethod, [left, right]),
                    LinqExpression.Constant(0));
        }

        return action(left, right);
    }

    private LinqExpression InOperator(LinqExpression left, LinqExpression arr)
    {
        if (arr == null) return LinqExpression.Constant(false);

        if (!typeof(IEnumerable).IsAssignableFrom(arr.Type))
            return LinqExpression.Constant(false);

        var isString = left.Type == typeof(string);

        var castMi = typeof(Enumerable).GetMethods(BindingFlags.Public | BindingFlags.Static)
            .First(m => m.Name == nameof(Enumerable.Cast) && m.GetParameters().Length == 1)
            .MakeGenericMethod(left.Type);
        var containsMi = typeof(Enumerable).GetMethods(BindingFlags.Public | BindingFlags.Static)
            .First(m => m.Name == nameof(Enumerable.Contains) && m.GetParameters().Length == (isString ? 3 : 2))
            .MakeGenericMethod(left.Type);

        var source = LinqExpression.Call(castMi, LinqExpression.Convert(arr, typeof(IEnumerable)));

        if (isString)
        {
            LinqExpression comparer =
                _caseInsensitiveStringComparer
                    ? (_ordinalStringComparer
                        ? LinqExpression.Constant(StringComparer.OrdinalIgnoreCase)
                        : LinqExpression.Constant(StringComparer.CurrentCultureIgnoreCase))
                    : (_ordinalStringComparer
                        ? LinqExpression.Constant(StringComparer.Ordinal)
                        : LinqExpression.Constant(StringComparer.CurrentCulture));

            return LinqExpression.Call(
                null, containsMi, source, LinqExpression.Convert(left, typeof(string)), comparer);
        }

        return LinqExpression.Call(null, containsMi, source, left);
    }

    public LinqExpression LikeOperator(LinqExpression leftValue, LinqExpression? rightValue)
    {
        if (leftValue is null || rightValue is null)
            return LinqExpression.Constant(false);

        var leftObj = LinqExpression.Convert(leftValue, typeof(string));
        var rightObj = LinqExpression.Convert(rightValue, typeof(string));

        var objToString = typeof(object).GetMethod(nameof(object.ToString))!;
        var leftStr = LinqExpression.Call(leftObj, objToString);
        var rightStr = LinqExpression.Call(rightObj, objToString);

        var miRegexEscape = typeof(Regex).GetMethod(nameof(Regex.Escape), new[] { typeof(string) })!;
        var miStringReplace = typeof(string).GetMethod(nameof(string.Replace), new[] { typeof(string), typeof(string) })!;

        var escaped = LinqExpression.Call(null, miRegexEscape, rightStr);
        var withStar = LinqExpression.Call(escaped, miStringReplace, LinqExpression.Constant("%"), LinqExpression.Constant(".*"));
        var withUnderscore = LinqExpression.Call(withStar, miStringReplace, LinqExpression.Constant("_"), LinqExpression.Constant("."));

        var miConcat = typeof(string).GetMethod(nameof(string.Concat), new[] { typeof(string), typeof(string), typeof(string) })!;
        var pattern = LinqExpression.Call(miConcat, LinqExpression.Constant("^"), withUnderscore, LinqExpression.Constant("$")); // "^" + pattern + "$"

        var miIsMatch = typeof(Regex).GetMethod(nameof(Regex.IsMatch), new[] { typeof(string), typeof(string), typeof(RegexOptions) })!;
        var options = LinqExpression.Constant(_options.HasFlag(ExpressionOptions.CaseInsensitiveStringComparer) ? RegexOptions.IgnoreCase : RegexOptions.None);
        var callIsMatch = LinqExpression.Call(null, miIsMatch, leftStr, pattern, options);

        // if either side is null should be false
        var leftNull = LinqExpression.Equal(leftObj, LinqExpression.Constant(null));
        var rightNull = LinqExpression.Equal(rightObj, LinqExpression.Constant(null));
        var anyNull = LinqExpression.OrElse(leftNull, rightNull);

        return LinqExpression.Condition(anyNull, LinqExpression.Constant(false), callIsMatch);
    }
}