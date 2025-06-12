using System.Numerics;
using System.Reflection;

using ExtendedNumerics;

using NCalc.Domain;
using NCalc.Exceptions;
using NCalc.Helpers;
using NCalc.Reflection;

using Linq = System.Linq.Expressions;
using LinqExpression = System.Linq.Expressions.Expression;
using LinqParameterExpression = System.Linq.Expressions.ParameterExpression;

namespace NCalc.Visitors;

public sealed class LambdaExpressionVisitor : ILogicalExpressionVisitor<LinqExpression>
{
    private readonly IDictionary<string, object?>? _parameters;
    private readonly LinqExpression? _context;
    private readonly ExpressionOptions _options;
    private readonly bool _ordinalStringComparer;
    private readonly bool _caseInsensitiveStringComparer;
    private readonly bool _checked;

    private ExpressionContext? _expressionContext;

    private static readonly MethodInfo StringComparerEqualsMethod =
        typeof(StringComparer).GetMethod("Equals", [typeof(string), typeof(string)])!;

    private static readonly MethodInfo StringComparerCompareMethod =
        typeof(StringComparer).GetMethod("Compare", [typeof(string), typeof(string)])!;

    public LambdaExpressionVisitor(LinqParameterExpression? context, ExpressionContext? expressionContext, IDictionary<string, object?>? parameters, ExpressionOptions options)
    {
        if (context != null)
            _context = context;
        if (expressionContext != null)
            _expressionContext = expressionContext;
        if (parameters != null)
            _parameters = parameters;

        _options = options;
        _ordinalStringComparer = _options.HasFlag(ExpressionOptions.OrdinalStringComparer);
        _checked = _options.HasFlag(ExpressionOptions.OverflowProtection);
        _caseInsensitiveStringComparer = _options.HasFlag(ExpressionOptions.CaseInsensitiveStringComparer);
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

        if ((expression.LeftExpression is PercentExpression) && (expression.RightExpression is PercentExpression))
        {
            return expression.Type switch
            {
                BinaryExpressionType.Minus => _checked ? OfPercent(left, right, LinqExpression.SubtractChecked, expression.Type) : OfPercent(left, right, LinqExpression.Subtract, expression.Type),
                BinaryExpressionType.Plus => _checked ? OfPercent(left, right, LinqExpression.AddChecked, expression.Type) : OfPercent(left, right, LinqExpression.Add, expression.Type),

                BinaryExpressionType.Unknown => throw new ArgumentOutOfRangeException(),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        else
        if (expression.LeftExpression is PercentExpression)
        {
            return expression.Type switch
            {
                BinaryExpressionType.Div => OfPercent(left, right, LinqExpression.Divide, expression.Type),
                BinaryExpressionType.Times => _checked ? OfPercent(left, right, LinqExpression.MultiplyChecked, expression.Type) : OfPercent(left, right, LinqExpression.Multiply, expression.Type),

                BinaryExpressionType.Unknown => throw new ArgumentOutOfRangeException(),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        else
        if ((expression.RightExpression is PercentExpression) && (expression.Type != BinaryExpressionType.Assignment))
        {
            return expression.Type switch
            {
                BinaryExpressionType.Minus => _checked ? WithPercent(left, right, LinqExpression.SubtractChecked, expression.Type) : WithPercent(left, right, LinqExpression.Subtract, expression.Type),
                BinaryExpressionType.Plus => _checked ? WithPercent(left, right, LinqExpression.AddChecked, expression.Type) : WithPercent(left, right, LinqExpression.Add, expression.Type),
                BinaryExpressionType.Div => WithPercent(left, right, LinqExpression.Divide, expression.Type),
                BinaryExpressionType.Times => _checked ? WithPercent(left, right, LinqExpression.MultiplyChecked, expression.Type) : WithPercent(left, right, LinqExpression.Multiply, expression.Type),
                BinaryExpressionType.Unknown => throw new ArgumentOutOfRangeException(),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        else
        {
            return expression.Type switch
            {
                BinaryExpressionType.StatementSequence => SkipAndReturn(left, right),
                BinaryExpressionType.Assignment => UpdateParameter<object?>(expression.LeftExpression, right),

                BinaryExpressionType.PlusAssignment => UpdateParameter<object?>(expression.LeftExpression, _checked ? WithCommonNumericType(left, right, LinqExpression.AddChecked, BinaryExpressionType.Plus) : WithCommonNumericType(left, right, LinqExpression.Add, BinaryExpressionType.Plus)),
                BinaryExpressionType.MinusAssignment => UpdateParameter<object?>(expression.LeftExpression, _checked ? WithCommonNumericType(left, right, LinqExpression.SubtractChecked, BinaryExpressionType.Minus) : WithCommonNumericType(left, right, LinqExpression.Subtract, BinaryExpressionType.Minus)),

                BinaryExpressionType.MultiplyAssignment => UpdateParameter<object?>(expression.LeftExpression, _checked ? WithCommonNumericType(left, right, LinqExpression.MultiplyChecked) : WithCommonNumericType(left, right, LinqExpression.Multiply)),
                BinaryExpressionType.DivAssignment => UpdateParameter<object?>(expression.LeftExpression, WithCommonNumericType(left, right, LinqExpression.Divide)),

                BinaryExpressionType.AndAssignment => UpdateParameter<object?>(expression.LeftExpression, LinqExpression.And(left, right)),
                BinaryExpressionType.OrAssignment => UpdateParameter<object?>(expression.LeftExpression, LinqExpression.Or(left, right)),
                BinaryExpressionType.XOrAssignment => UpdateParameter<object?>(expression.LeftExpression, LinqExpression.ExclusiveOr(left, right)),

                BinaryExpressionType.And => LinqExpression.AndAlso(left, right),
                BinaryExpressionType.Or => LinqExpression.OrElse(left, right),
                BinaryExpressionType.XOr => BooleanXOr(left, right),
                BinaryExpressionType.NotEqual => WithCommonNumericType(left, right, LinqExpression.NotEqual, expression.Type),
                BinaryExpressionType.LessOrEqual => WithCommonNumericType(left, right, LinqExpression.LessThanOrEqual, expression.Type),
                BinaryExpressionType.GreaterOrEqual => WithCommonNumericType(left, right, LinqExpression.GreaterThanOrEqual, expression.Type),
                BinaryExpressionType.Less => WithCommonNumericType(left, right, LinqExpression.LessThan, expression.Type),
                BinaryExpressionType.Greater => WithCommonNumericType(left, right, LinqExpression.GreaterThan, expression.Type),
                BinaryExpressionType.Equal => WithCommonNumericType(left, right, LinqExpression.Equal, expression.Type),
                BinaryExpressionType.Minus => _checked ? WithCommonNumericType(left, right, LinqExpression.SubtractChecked, BinaryExpressionType.Minus) : WithCommonNumericType(left, right, LinqExpression.Subtract, BinaryExpressionType.Minus),
                BinaryExpressionType.Plus => _checked ? WithCommonNumericType(left, right, LinqExpression.AddChecked, BinaryExpressionType.Plus) : WithCommonNumericType(left, right, LinqExpression.Add, BinaryExpressionType.Plus),
                BinaryExpressionType.Modulo => WithCommonNumericType(left, right, LinqExpression.Modulo),
                BinaryExpressionType.Div => WithCommonNumericType(left, right, LinqExpression.Divide),
                BinaryExpressionType.Times => _checked ? WithCommonNumericType(left, right, LinqExpression.MultiplyChecked) : WithCommonNumericType(left, right, LinqExpression.Multiply),
                BinaryExpressionType.BitwiseOr => LinqExpression.Or(left, right),
                BinaryExpressionType.BitwiseAnd => LinqExpression.And(left, right),
                BinaryExpressionType.BitwiseXOr => LinqExpression.ExclusiveOr(left, right),
                BinaryExpressionType.LeftShift => LinqExpression.LeftShift(left, right),
                BinaryExpressionType.RightShift => LinqExpression.RightShift(left, right),
                BinaryExpressionType.Exponentiation => LinqExpression.Power(LinqExpression.Convert(left, typeof(double)), LinqExpression.Convert(right, typeof(double))),
                BinaryExpressionType.Factorial => Factorial(left, right),
                BinaryExpressionType.Unknown => throw new ArgumentOutOfRangeException(),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

    public LinqExpression Visit(UnaryExpression expression)
    {
        var operand = expression.Expression.Accept(this);

        return expression.Type switch
        {
            UnaryExpressionType.Not => LinqExpression.Not(operand),
            UnaryExpressionType.Negate => LinqExpression.Negate(operand),
            UnaryExpressionType.BitwiseNot => LinqExpression.Not(operand),
            UnaryExpressionType.SqRoot => Sqrt(operand),
#if NET8_0_OR_GREATER
            UnaryExpressionType.CbRoot => Cbrt(operand),
#endif
            UnaryExpressionType.FourthRoot => Frthrt(operand),
            UnaryExpressionType.Positive => operand,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public LinqExpression Visit(PercentExpression expression)
    {
        return expression.Expression.Accept(this);
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
            var smi = typeof(Array).GetMethod("IndexOf", [typeof(Array), typeof(object)]);
            var r = LinqExpression.Call(smi!, LinqExpression.Convert(items, typeof(Array)),
                LinqExpression.Convert(args[0], typeof(object)));
            return LinqExpression.GreaterThanOrEqual(r, LinqExpression.Constant(0));
        }

        //Context methods take precedence over built-in functions because they're user-customizable.
        var mi = FindMethod(_expressionContext?.Options.HasFlag(ExpressionOptions.LowerCaseIdentifierLookup) == true ? function.Identifier.Name.ToLowerInvariant() : function.Identifier.Name, args);
        if (mi != null)
        {
            return LinqExpression.Call(_context, mi.MethodInfo, mi.PreparedArguments);
        }

        Linq.UnaryExpression arg0;
        Linq.UnaryExpression arg1;

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
            case "ROUND":
                CheckArgumentsLengthForFunction(functionName, function.Parameters.Count, 2);

                if (_options == ExpressionOptions.DecimalAsDefault)
                    arg0 = LinqExpression.Convert(args[0], typeof(decimal));
                else
                    arg0 = LinqExpression.Convert(args[0], typeof(double));

                arg1 = LinqExpression.Convert(args[1], typeof(int));

                var rounding = (_options & ExpressionOptions.RoundAwayFromZero) == ExpressionOptions.RoundAwayFromZero
                    ? MidpointRounding.AwayFromZero
                    : MidpointRounding.ToEven;
                return LinqExpression.Call(MathFunctionHelper.Functions["ROUND"].First().MethodInfo, arg0, arg1,
                    LinqExpression.Constant(rounding));

            default:
                // Regular handling
                if (MathFunctionHelper.Functions.TryGetValue(functionName, out var f))
                {
                    MathMethodInfo func;

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

    public LinqExpression Visit(Identifier identifier)
    {
        var identifierName = _expressionContext?.Options.HasFlag(ExpressionOptions.LowerCaseIdentifierLookup) == true ? identifier.Name.ToLowerInvariant() : identifier.Name;

        if (_context == null)
        {
            if (_parameters != null && _parameters.TryGetValue(identifierName, out var param))
                return LinqExpression.Constant(param);

            throw new NCalcParameterNotDefinedException(identifier.Name);
        }

        return LinqExpression.PropertyOrField(_context, identifierName);
    }

    public LinqExpression Visit(LogicalExpressionList list)
    {
        throw new NotSupportedException("Collections are not supported for Lambda expressions yet. Please open a issue at https://www.github.com/ncalc/ncalc if you want this support.");
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

    private static Linq.Expression<Func<BigInteger>> ConvertToBigInteger(LinqExpression intExpr)
    {
        if (intExpr.Type != typeof(int) && intExpr.Type != typeof(long) && intExpr.Type != typeof(uint) && intExpr.Type != typeof(ulong) && intExpr.Type != typeof(string) && intExpr.Type != typeof(decimal) && intExpr.Type != typeof(double) && intExpr.Type != typeof(float))
            throw new InvalidOperationException("Expression must return a signed or unsigned int or long.");

        // Create the constructor call: new BigInteger(longExpr.Body)
        if (intExpr.Type == typeof(string))
        {
            MethodInfo? methodInfo = typeof(BigInteger).GetMethod("Parse", [typeof(string)]);
            if (methodInfo == null)
                throw new InvalidOperationException($"Could not find a BigInteger Parse method that takes a parameter of type '{intExpr.Type}'");
            var newExpr = LinqExpression.Call(methodInfo, intExpr);

            // Build the new lambda expression
            return LinqExpression.Lambda<Func<BigInteger>>(newExpr);
        }
        else
        {
            ConstructorInfo? constructor = null;
            if (intExpr.Type == typeof(Int32))
                constructor = typeof(BigInteger).GetConstructor(new[] { typeof(int) });
            if (intExpr.Type == typeof(UInt32))
                constructor = typeof(BigInteger).GetConstructor(new[] { typeof(uint) });
            if (intExpr.Type == typeof(Int64))
                constructor = typeof(BigInteger).GetConstructor(new[] { typeof(long) });
            if (intExpr.Type == typeof(UInt64))
                constructor = typeof(BigInteger).GetConstructor(new[] { typeof(ulong) });
            if (intExpr.Type == typeof(decimal))
                constructor = typeof(BigInteger).GetConstructor(new[] { typeof(decimal) });
            if (intExpr.Type == typeof(double))
                constructor = typeof(BigInteger).GetConstructor(new[] { typeof(double) });
            if (intExpr.Type == typeof(float))
                constructor = typeof(BigInteger).GetConstructor(new[] { typeof(float) });

            if (constructor == null)
                throw new InvalidOperationException($"Could not find a BigInteger constructor that takes a parameter of type '{intExpr.Type}'");
            var newExpr = LinqExpression.New(constructor!, intExpr);

            // Build the new lambda expression
            return LinqExpression.Lambda<Func<BigInteger>>(newExpr);
        }
    }

    private void OnUpdateParameter(string name, NCalc.Handlers.UpdateParameterArgs args)
    {
        _expressionContext?.UpdateParameterHandler?.Invoke(name, args);
    }

    private LinqExpression SkipAndReturn(LinqExpression left, LinqExpression right)
    {
        // Combine into a block: evaluate both, return right
        var block = LinqExpression.Block(
            LinqExpression.Convert(left, typeof(object)), // Evaluate and discard
            right // Result returned
        );

        return block;
    }

    private LinqExpression UpdateParameter<T>(LogicalExpression leftExpression, LinqExpression valueExpr)
    {
        if (leftExpression is Identifier identifier)
        {
            var identifierName = identifier.Name;

            var sideEffect = new Func<T, bool>((value) =>
            {
                var parameterArgs = new NCalc.Handlers.UpdateParameterArgs(identifierName, identifier.Id, value);
                OnUpdateParameter(identifierName, parameterArgs);

                if (parameterArgs.UpdateParameterLists)
                {
                    if (_parameters != null)
                        _parameters[_expressionContext?.Options.HasFlag(ExpressionOptions.LowerCaseIdentifierLookup) == true ? identifierName.ToLowerInvariant() : identifierName] = value;

                    if (_context != null)
                        return true;
                }
                return false;
            });

            var convertToT = LinqExpression.Convert(valueExpr, typeof(T));

            Linq.LambdaExpression sideEffectFunc = bool (T x) => sideEffect(x);

            var invokeSideEffect = LinqExpression.Invoke(sideEffectFunc, convertToT);

            // Variable to store evaluated result
            var valueVar = LinqExpression.Variable(typeof(T), "value");
            var needUpdateParamsVar = LinqExpression.Variable(typeof(bool), "needUpdateParams");

            // If required, update parameters
            LinqExpression maybeUpdateParameters = (_context != null) ? LinqExpression.Assign(LinqExpression.PropertyOrField(_context, identifierName), valueExpr) : LinqExpression.Empty();

            // Sequence: run side-effect, then return value
            var block = LinqExpression.Block(
                [valueVar, needUpdateParamsVar],
                LinqExpression.Assign(valueVar, convertToT),
                LinqExpression.Assign(needUpdateParamsVar, invokeSideEffect),
                LinqExpression.IfThen(needUpdateParamsVar, maybeUpdateParameters),
                valueVar
            );
            return block;
        }
        else
        {
            return valueExpr;
        }
    }

    private LinqExpression BooleanXOr(LinqExpression left, LinqExpression right)
    {
        if (left == null) throw new ArgumentNullException(nameof(left));
        if (right == null) throw new ArgumentNullException(nameof(right));

        // Convert each to boolean: true if not default(T), false if default(T)
        LinqExpression leftIsDefault = LinqExpression.Equal(left, LinqExpression.Default(left.Type));
        LinqExpression leftAsBool = LinqExpression.Not(leftIsDefault);

        LinqExpression rightIsDefault = LinqExpression.Equal(right, LinqExpression.Default(right.Type));
        LinqExpression rightAsBool = LinqExpression.Not(rightIsDefault);

        // Apply boolean XOR: A ^ B
        return LinqExpression.ExclusiveOr(leftAsBool, rightAsBool);
    }

    private LinqExpression Factorial(LinqExpression left, LinqExpression right)
    {
        left = LinqUtils.UnwrapNullable(left);
        right = LinqUtils.UnwrapNullable(right);

        if (left.Type == typeof(BigInteger) && right.Type == typeof(BigInteger))
        {
            MethodInfo? factorialMethod = typeof(MathHelper).GetMethod("Factorial", [typeof(object), typeof(object), typeof(MathHelperOptions)]);
            if (factorialMethod != null)
            {
                return LinqExpression.Call(factorialMethod, left, right, LinqExpression.Constant(new MathHelperOptions()));
            }
        }
        else
        if (left.Type == typeof(BigInteger))
        {
            MethodInfo? factorialMethod = typeof(MathHelper).GetMethod("Factorial", [typeof(object), typeof(object), typeof(MathHelperOptions)]);
            if (factorialMethod != null)
            {
                Linq.Expression<Func<long, BigInteger>> expr = x => new BigInteger(x);
                return LinqExpression.Call(factorialMethod, left, ConvertToBigInteger(right), LinqExpression.Constant(new MathHelperOptions()));
            }
        }
        else
        if (right.Type == typeof(BigInteger))
        {
            MethodInfo? factorialMethod = typeof(MathHelper).GetMethod("Factorial", [typeof(object), typeof(object), typeof(MathHelperOptions)]);
            if (factorialMethod != null)
            {
                Linq.Expression<Func<long, BigInteger>> expr = x => new BigInteger(x);
                return LinqExpression.Call(factorialMethod, ConvertToBigInteger(left), right, LinqExpression.Constant(new MathHelperOptions()));
            }
        }
        else
        {
            MethodInfo? factorialMethod = typeof(MathHelper).GetMethod("Factorial", [typeof(long), typeof(long), typeof(MathHelperOptions)]);
            if (factorialMethod != null)
            {
                return LinqExpression.Call(factorialMethod, LinqExpression.Convert(left, typeof(long)), LinqExpression.Convert(right, typeof(long)), LinqExpression.Constant(new MathHelperOptions()));
            }
        }
        return LinqExpression.Constant(0);
    }

    private LinqExpression Sqrt(LinqExpression left)
    {
        left = LinqUtils.UnwrapNullable(left);
        MethodInfo? sqrtMethod = typeof(Math).GetMethod("Sqrt");
        if (sqrtMethod != null)
        {
            return LinqExpression.Call(sqrtMethod, LinqExpression.Convert(left, typeof(double)));
        }
        return LinqExpression.Constant(0);
    }

    private LinqExpression Frthrt(LinqExpression left)
    {
        left = LinqUtils.UnwrapNullable(left);
        MethodInfo? sqrtMethod = typeof(Math).GetMethod("Sqrt");
        if (sqrtMethod != null)
        {
            return LinqExpression.Call(sqrtMethod, LinqExpression.Call(sqrtMethod, LinqExpression.Convert(left, typeof(double))));
        }
        return LinqExpression.Constant(0);
    }

#if NET8_0_OR_GREATER
    private LinqExpression Cbrt(LinqExpression left)
    {
        left = LinqUtils.UnwrapNullable(left);
        MethodInfo? cbrtMethod = typeof(Math).GetMethod("Cbrt");
        if (cbrtMethod != null)
        {
            return LinqExpression.Call(cbrtMethod, LinqExpression.Convert(left, typeof(double)));
        }
        return LinqExpression.Constant(0);
    }
#endif
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

        if (_options.HasFlag(ExpressionOptions.SupportTimeOperations))
        {
            if ((left.Type == typeof(DateTime)) && (right.Type == typeof(DateTime)))
            {
                if (expressionType == BinaryExpressionType.Minus)
                {
                    MethodInfo? subtractMethod = typeof(DateTime).GetMethod("Subtract", [typeof(DateTime)]);
                    if (subtractMethod != null)
                    {
                        return LinqExpression.Call(left, subtractMethod, right);
                    }
                }
                return LinqExpression.Constant(0);
            }
            else
            if ((left.Type == typeof(DateTime)) && (right.Type == typeof(TimeSpan)))
            {
                switch (expressionType)
                {
                    case BinaryExpressionType.Plus:
                        MethodInfo? addMethod = typeof(DateTime).GetMethod("Add", [typeof(TimeSpan)]);
                        if (addMethod != null)
                        {
                            return LinqExpression.Call(left, addMethod, right);
                        }
                        break;
                    case BinaryExpressionType.Minus:
                        MethodInfo? subtractMethod = typeof(DateTime).GetMethod("Subtract", [typeof(TimeSpan)]);
                        if (subtractMethod != null)
                        {
                            return LinqExpression.Call(left, subtractMethod, right);
                        }
                        break;
                }
                return LinqExpression.Constant(0);
            }
            else
            if ((left.Type == typeof(TimeSpan)) && (right.Type == typeof(TimeSpan)))
            {
                switch (expressionType)
                {
                    case BinaryExpressionType.Plus:
                        MethodInfo? addMethod = typeof(TimeSpan).GetMethod("Add", [typeof(TimeSpan)]);
                        if (addMethod != null)
                        {
                            return LinqExpression.Call(left, addMethod, right);
                        }
                        break;
                    case BinaryExpressionType.Minus:
                        MethodInfo? subtractMethod = typeof(TimeSpan).GetMethod("Subtract", [typeof(TimeSpan)]);
                        if (subtractMethod != null)
                        {
                            return LinqExpression.Call(left, subtractMethod, right);
                        }
                        break;
                }
                return LinqExpression.Constant(0);
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
            case BinaryExpressionType.LessOrEqual:
                return LinqExpression.LessThanOrEqual(
                    LinqExpression.Call(comparer, StringComparerCompareMethod, [left, right]),
                    LinqExpression.Constant(0));
            case BinaryExpressionType.Greater:
                return LinqExpression.GreaterThan(
                    LinqExpression.Call(comparer, StringComparerCompareMethod, [left, right]),
                    LinqExpression.Constant(0));
            case BinaryExpressionType.Less:
                return LinqExpression.LessThan(
                    LinqExpression.Call(comparer, StringComparerCompareMethod, [left, right]),
                    LinqExpression.Constant(0));
        }

        return action(left, right);
    }

    /// <summary>
    /// Operations with a percent value being the right operand
    /// </summary>
    private LinqExpression WithPercent(LinqExpression left, LinqExpression right,
        Func<LinqExpression, LinqExpression, LinqExpression> action,
        BinaryExpressionType expressionType = BinaryExpressionType.Unknown)
    {
        left = LinqUtils.UnwrapNullable(left);
        right = LinqUtils.UnwrapNullable(right);

        LinqExpression threshold = LinqExpression.Constant(100000);
        LinqExpression cents = LinqExpression.Constant(100);

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

            if (threshold.Type != type)
            {
                threshold = LinqExpression.Convert(threshold, type);
            }
            if (cents.Type != type)
            {
                cents = LinqExpression.Convert(cents, type);
            }
        }

        if (typeof(string) != left.Type && typeof(string) != right.Type)
        {
            switch (expressionType)
            {
                case BinaryExpressionType.Times:
                    return LinqExpression.Condition(
                        LinqExpression.GreaterThanOrEqual(left, threshold),
                        action(left, LinqExpression.Divide(right, cents)),
                        LinqExpression.Divide(action(left, right), cents));
                case BinaryExpressionType.Div:
                        return action(LinqExpression.Multiply(left, cents), right);
                case BinaryExpressionType.Plus:
                case BinaryExpressionType.Minus:
                    LinqExpression actionResult = action(cents, right);
                    return LinqExpression.Divide(_checked ? LinqExpression.MultiplyChecked(left, actionResult) : LinqExpression.Multiply(left, actionResult), cents);
            }
            return action(left, right);
        }

        return LinqExpression.Constant(0);
    }

    /// <summary>
    /// Operations with a percent value being the left operand
    /// </summary>
    private LinqExpression OfPercent(LinqExpression left, LinqExpression right,
        Func<LinqExpression, LinqExpression, LinqExpression> action,
        BinaryExpressionType expressionType = BinaryExpressionType.Unknown)
    {
        left = LinqUtils.UnwrapNullable(left);
        right = LinqUtils.UnwrapNullable(right);

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
        else
            type = typeof(double);

        if (typeof(string) != left.Type && typeof(string) != right.Type)
        {
            LinqExpression result;
            switch (expressionType)
            {
                case BinaryExpressionType.Times:
                case BinaryExpressionType.Div:
                case BinaryExpressionType.Plus:
                case BinaryExpressionType.Minus:
                    result = action(left, right);
                    break;
                default:
                    result = LinqExpression.Constant(0);
                    break;
            }

            MethodInfo? toStringMethod = type.GetMethod("ToString", Type.EmptyTypes);
            MethodInfo? concatMethod = typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string) });
            if ((toStringMethod != null) && (concatMethod != null))
            {
                return LinqExpression.Call(concatMethod, LinqExpression.Call(result, toStringMethod), LinqExpression.Constant("%"));
            }
        }
        return LinqExpression.Constant(0);
    }
}