#nullable disable

using NCalc.Domain;
using System.Reflection;
using NCalc.Reflection;
using Linq = System.Linq.Expressions;
using LinqExpression = System.Linq.Expressions.Expression;

namespace NCalc.Visitors;

internal class LambdaExpressionVistor : ILogicalExpressionVisitor
{
    private readonly IDictionary<string, object> _parameters;
    private LinqExpression _result;
    private readonly LinqExpression _context;
    private readonly ExpressionOptions _options;
    private readonly Dictionary<Type, HashSet<Type>> _implicitPrimitiveConversionTable = new() {
        { typeof(sbyte), [typeof(short), typeof(int), typeof(long), typeof(float), typeof(double), typeof(decimal)] },
        { typeof(byte),
            [
                typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float),
                typeof(double), typeof(decimal)
            ]
        },
        { typeof(short), [typeof(int), typeof(long), typeof(float), typeof(double), typeof(decimal)] },
        { typeof(ushort),
            [typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal)]
        },
        { typeof(int), [typeof(long), typeof(float), typeof(double), typeof(decimal)] },
        { typeof(uint), [typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal)] },
        { typeof(long), [typeof(float), typeof(double), typeof(decimal)] },
        { typeof(char),
            [
                typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double),
                typeof(decimal)
            ]
        },
        { typeof(float), [typeof(double)] },
        { typeof(ulong), [typeof(float), typeof(double), typeof(decimal)] },
    };

    private struct MathCallFunction
    {
        public MethodInfo MathMethodInfo;
        public int ArgumentCount;
    }

    private static MathCallFunction GetMathCallFunctionHelper(string method, int argCount) => new()
    {
        MathMethodInfo = typeof(Math).GetMethod(method, Enumerable.Repeat(typeof(double), argCount).ToArray()),
        ArgumentCount = argCount };

    private static readonly Dictionary<string, MathCallFunction> MathCallFunctions = new()
    {
        { "ABS",           GetMathCallFunctionHelper(nameof(Math.Abs),           1 ) },
        { "ACOS",          GetMathCallFunctionHelper(nameof(Math.Acos),          1 ) },
        { "ASIN",          GetMathCallFunctionHelper(nameof(Math.Asin),          1 ) },
        { "ATAN",          GetMathCallFunctionHelper(nameof(Math.Atan),          1 ) },
        { "ATAN2",         GetMathCallFunctionHelper(nameof(Math.Atan2),         2 ) },
        { "CEILING",       GetMathCallFunctionHelper(nameof(Math.Ceiling),       1 ) },
        { "COS",           GetMathCallFunctionHelper(nameof(Math.Cos),           1 ) },
        { "COSH",          GetMathCallFunctionHelper(nameof(Math.Cosh),          1 ) },
        { "EXP",           GetMathCallFunctionHelper(nameof(Math.Exp),           1 ) },
        { "FLOOR",         GetMathCallFunctionHelper(nameof(Math.Floor),         1 ) },
        { "IEEEREMAINDER", GetMathCallFunctionHelper(nameof(Math.IEEERemainder), 2 ) },
        { "LOG",           GetMathCallFunctionHelper(nameof(Math.Log),           2 ) },
        { "LOG10",         GetMathCallFunctionHelper(nameof(Math.Log10),         1 ) },
        { "SIGN",          GetMathCallFunctionHelper(nameof(Math.Sign),          1 ) },
        { "SIN",           GetMathCallFunctionHelper(nameof(Math.Sin),           1 ) },
        { "SINH",          GetMathCallFunctionHelper(nameof(Math.Sinh),          1 ) },
        { "SQRT",          GetMathCallFunctionHelper(nameof(Math.Sqrt),          1 ) },
        { "TAN",           GetMathCallFunctionHelper(nameof(Math.Tan),           1 ) },
        { "TANH",          GetMathCallFunctionHelper(nameof(Math.Tanh),          1 ) },
        { "TRUNCATE",      GetMathCallFunctionHelper(nameof(Math.Truncate),      1 ) },

        // Exceptional handling
        { "ROUND",         new() { 
            MathMethodInfo = typeof(Math).GetMethod(nameof(Math.Round), [typeof(double), typeof(int), typeof(MidpointRounding)
            ]),
            ArgumentCount = 2 } }
    };

    private bool CaseInsensitiveComparer => _options.HasFlag(ExpressionOptions.CaseInsensitiveComparer);

    public LambdaExpressionVistor(IDictionary<string, object> parameters, ExpressionOptions options)
    {
        _parameters = parameters;
        _options = options;
    }

    public LambdaExpressionVistor(Linq.ParameterExpression context, ExpressionOptions options)
    {
        _context = context;
        _options = options;
    }

    public LinqExpression Result => _result;

    public void Visit(LogicalExpression expression)
    {
        throw new NotImplementedException();
    }

    public void Visit(TernaryExpression expression)
    {
        expression.LeftExpression.Accept(this);
        var test = _result;

        expression.MiddleExpression.Accept(this);
        var ifTrue = _result;

        expression.RightExpression.Accept(this);
        var ifFalse = _result;

        _result = LinqExpression.Condition(test, ifTrue, ifFalse);
    }

    public void Visit(BinaryExpression expression)
    {
        expression.LeftExpression.Accept(this);
        var left = _result;

        expression.RightExpression.Accept(this);
        var right = _result;

        switch (expression.Type)
        {
            case BinaryExpressionType.And:
                _result = LinqExpression.AndAlso(left, right);
                break;
            case BinaryExpressionType.Or:
                _result = LinqExpression.OrElse(left, right);
                break;
            case BinaryExpressionType.NotEqual:
                _result = WithCommonNumericType(left, right, LinqExpression.NotEqual, expression.Type);
                break;
            case BinaryExpressionType.LesserOrEqual:
                _result = WithCommonNumericType(left, right, LinqExpression.LessThanOrEqual, expression.Type);
                break;
            case BinaryExpressionType.GreaterOrEqual:
                _result = WithCommonNumericType(left, right, LinqExpression.GreaterThanOrEqual, expression.Type);
                break;
            case BinaryExpressionType.Lesser:
                _result = WithCommonNumericType(left, right, LinqExpression.LessThan, expression.Type);
                break;
            case BinaryExpressionType.Greater:
                _result = WithCommonNumericType(left, right, LinqExpression.GreaterThan, expression.Type);
                break;
            case BinaryExpressionType.Equal:
                _result = WithCommonNumericType(left, right, LinqExpression.Equal, expression.Type);
                break;
            case BinaryExpressionType.Minus:
                _result = WithCommonNumericType(left, right, LinqExpression.Subtract);
                break;
            case BinaryExpressionType.Plus:
                _result = WithCommonNumericType(left, right, LinqExpression.Add);
                break;
            case BinaryExpressionType.Modulo:
                _result = WithCommonNumericType(left, right, LinqExpression.Modulo);
                break;
            case BinaryExpressionType.Div:
                _result = WithCommonNumericType(left, right, LinqExpression.Divide);
                break;
            case BinaryExpressionType.Times:
                _result = WithCommonNumericType(left, right, LinqExpression.Multiply);
                break;
            case BinaryExpressionType.BitwiseOr:
                _result = LinqExpression.Or(left, right);
                break;
            case BinaryExpressionType.BitwiseAnd:
                _result = LinqExpression.And(left, right);
                break;
            case BinaryExpressionType.BitwiseXOr:
                _result = LinqExpression.ExclusiveOr(left, right);
                break;
            case BinaryExpressionType.LeftShift:
                _result = LinqExpression.LeftShift(left, right);
                break;
            case BinaryExpressionType.RightShift:
                _result = LinqExpression.RightShift(left, right);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void Visit(UnaryExpression expression)
    {
        expression.Expression.Accept(this);
        switch (expression.Type)
        {
            case UnaryExpressionType.Not:
                _result = LinqExpression.Not(_result);
                break;
            case UnaryExpressionType.Negate:
                _result = LinqExpression.Negate(_result);
                break;
            case UnaryExpressionType.BitwiseNot:
                _result = LinqExpression.Not(_result);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void Visit(ValueExpression expression)
    {
        _result = LinqExpression.Constant(expression.Value);
    }

    public void Visit(Function function)
    {
        var args = new LinqExpression[function.Expressions.Length];
        for (var i = 0; i < function.Expressions.Length; i++)
        {
            function.Expressions[i].Accept(this);
            args[i] = _result;
        }

        var functionName = function.Identifier.Name.ToUpperInvariant();
        switch (functionName)
        {
            case "IF":
            {
                var numberTypePriority = new Type[] { typeof(double), typeof(float), typeof(long), typeof(int), typeof(short) };
                var index1 = Array.IndexOf(numberTypePriority, args[1].Type);
                var index2 = Array.IndexOf(numberTypePriority, args[2].Type);
                if (index1 >= 0 && index2 >= 0 && index1 != index2)
                {
                    args[1] = LinqExpression.Convert(args[1], numberTypePriority[Math.Min(index1, index2)]);
                    args[2] = LinqExpression.Convert(args[2], numberTypePriority[Math.Min(index1, index2)]);
                }
                _result = LinqExpression.Condition(args[0], args[1], args[2]);
                return;
            }
            case "IN":
            {
                var items = LinqExpression.NewArrayInit(args[0].Type,
                    new ArraySegment<LinqExpression>(args, 1, args.Length - 1));
                var smi = typeof(Array).GetRuntimeMethod("IndexOf", [typeof(Array), typeof(object)]);
                var r = LinqExpression.Call(smi, LinqExpression.Convert(items, typeof(Array)), LinqExpression.Convert(args[0], typeof(object)));
                _result = LinqExpression.GreaterThanOrEqual(r, LinqExpression.Constant(0));
                return;
            }
        }

        //Context methods take precedence over built-in functions because they're user-customisable.
        var mi = FindMethod(function.Identifier.Name, args);
        if (mi != null)
        {
            _result = LinqExpression.Call(_context, mi.BaseMethodInfo, mi.PreparedArguments);
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
                _result = LinqExpression.Condition(LinqExpression.GreaterThan(arg0, arg1), arg0, arg1);
                break;
            case "MIN":
                CheckArgumentsLengthForFunction(functionName, function.Expressions.Length, 2);
                arg0 = LinqExpression.Convert(args[0], typeof(double));
                arg1 = LinqExpression.Convert(args[1], typeof(double));
                _result = LinqExpression.Condition(LinqExpression.LessThan(arg0, arg1), arg0, arg1);
                break;
            case "POW":
                CheckArgumentsLengthForFunction(functionName, function.Expressions.Length, 2);
                arg0 = LinqExpression.Convert(args[0], typeof(double));
                arg1 = LinqExpression.Convert(args[1], typeof(double));
                _result = LinqExpression.Power(arg0, arg1);
                break;
            case "ROUND":
                CheckArgumentsLengthForFunction(functionName, function.Expressions.Length, 2);
                arg0 = LinqExpression.Convert(args[0], typeof(double));
                arg1 = LinqExpression.Convert(args[1], typeof(int));
                var rounding = (_options & ExpressionOptions.RoundAwayFromZero) == ExpressionOptions.RoundAwayFromZero ? MidpointRounding.AwayFromZero : MidpointRounding.ToEven;
                _result = LinqExpression.Call(MathCallFunctions["ROUND"].MathMethodInfo, arg0, arg1, LinqExpression.Constant(rounding));
                break;
            default:
                // Regular handling
                if (MathCallFunctions.TryGetValue(functionName, out var func))
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

        void CheckArgumentsLengthForFunction(string funcStr, int argsNum, int argsNeed)
        {
            if (argsNum != argsNeed)
                throw new ArgumentException($"{funcStr} takes exactly {argsNeed} argument");
        }

        void MakeMathCallExpression(MathCallFunction mathMethod, int argsNumActual)
        {
            CheckArgumentsLengthForFunction(mathMethod.MathMethodInfo.Name, argsNumActual, mathMethod.ArgumentCount);

            _result = LinqExpression.Call(mathMethod.MathMethodInfo,
                Enumerable.Range(0, argsNumActual).Select( i => LinqExpression.Convert(args[i], typeof(double)) ));
        }
    }

    public void Visit(Identifier function)
    {
        if (_context == null)
        {
            _result = LinqExpression.Constant(_parameters[function.Name]);
        }
        else
        {
            _result = LinqExpression.PropertyOrField(_context, function.Name);
        }
    }

    private ExtendedMethodInfo FindMethod(string methodName, LinqExpression[] methodArgs)
    {
        if (_context == null) return null;

        var contextTypeInfo = _context.Type.GetTypeInfo();
        var objectTypeInfo = typeof(object).GetTypeInfo();
        do
        {
            var methods = contextTypeInfo.DeclaredMethods.Where(m => m.Name.Equals(methodName, StringComparison.OrdinalIgnoreCase) && m.IsPublic && !m.IsStatic);
            var candidates = new List<ExtendedMethodInfo>();
            foreach (var potentialMethod in methods)
            {
                var methodParams = potentialMethod.GetParameters();
                var preparedArguments = PrepareMethodArgumentsIfValid(methodParams, methodArgs);

                if (preparedArguments != null)
                {
                    var candidate = new ExtendedMethodInfo()
                    {
                        BaseMethodInfo = potentialMethod,
                        PreparedArguments = preparedArguments.Item2,
                        Score = preparedArguments.Item1
                    };
                    if (candidate.Score == 0) return candidate;
                    candidates.Add(candidate);
                }
            }
            if (candidates.Count != 0) return candidates.OrderBy(method => method.Score).First();
            contextTypeInfo = contextTypeInfo.BaseType.GetTypeInfo();
        } while (contextTypeInfo != objectTypeInfo);
        return null;
    }

    /// <summary>
    /// Returns a tuple where the first item is a score, and the second is a list of prepared arguments. 
    /// Score is a simplified indicator of how close the arguments' types are to the parameters'. A score of 0 indicates a perfect match between arguments and parameters. 
    /// Prepared arguments refers to having the arguments implicitly converted where necessary, and "params" arguments collated into one array.
    /// </summary>
    /// <param name="parameters"></param>
    /// <param name="arguments"></param>
    /// <returns></returns>
    private Tuple<int, LinqExpression[]> PrepareMethodArgumentsIfValid(ParameterInfo[] parameters, LinqExpression[] arguments)
    {
        switch (parameters.Length)
        {
            case 0 when arguments.Length == 0:
                return Tuple.Create(0, arguments);
            case 0:
                return null;
        }

        var lastParameter = parameters.Last();
        var hasParamsKeyword = lastParameter.IsDefined(typeof(ParamArrayAttribute));
        if (hasParamsKeyword && parameters.Length > arguments.Length) return null;
        var newArguments = new LinqExpression[parameters.Length];
        LinqExpression[] paramsKeywordArgument = null;
        Type paramsElementType = null;
        var paramsParameterPosition = 0;
        if (!hasParamsKeyword)
        {
            if (parameters.Length != arguments.Length) return null;
        }
        else
        {
            paramsParameterPosition = lastParameter.Position;
            paramsElementType = lastParameter.ParameterType.GetElementType();
            paramsKeywordArgument = new LinqExpression[arguments.Length - parameters.Length + 1];
        }

        var functionMemberScore = 0;
        for (var i = 0; i < arguments.Length; i++)
        {
            var isParamsElement = hasParamsKeyword && i >= paramsParameterPosition;
            var argument = arguments[i];
            var argumentType = argument.Type;
            var parameterType = isParamsElement ? paramsElementType : parameters[i].ParameterType;
            if (argumentType != parameterType)
            {
                var canCastImplicitly = TryCastImplicitly(argumentType, parameterType, ref argument);
                if (!canCastImplicitly) return null;
                functionMemberScore++;
            }
            if (!isParamsElement)
            {
                newArguments[i] = argument;
            }
            else
            {
                paramsKeywordArgument[i - paramsParameterPosition] = argument;
            }
        }

        if (hasParamsKeyword)
        {
            newArguments[paramsParameterPosition] = LinqExpression.NewArrayInit(paramsElementType, paramsKeywordArgument);
        }
        return Tuple.Create(functionMemberScore, newArguments);
    }

    private bool TryCastImplicitly(Type from, Type to, ref LinqExpression argument)
    {
        var convertingFromPrimitiveType = _implicitPrimitiveConversionTable.TryGetValue(from, out var possibleConversions);
        if (!convertingFromPrimitiveType || !possibleConversions.Contains(to))
        {
            argument = null;
            return false;
        }
        argument = LinqExpression.Convert(argument, to);
        return true;
    }

    private LinqExpression WithCommonNumericType(LinqExpression left, LinqExpression right,
        Func<LinqExpression, LinqExpression, LinqExpression> action, BinaryExpressionType expressiontype = BinaryExpressionType.Unknown)
    {
        left = UnwrapNullable(left);
        right = UnwrapNullable(right);

        //TODO: Add ExpressionOptions.BooleanCalculation
        // if (_options.HasFlag(ExpressionOptions.BooleanCalculation))
        // {
        //     if (left.Type == typeof(bool))
        //     {
        //         left = Linq.Expression.Condition(left, Linq.Expression.Constant(1.0), Linq.Expression.Constant(0.0));
        //     }
        //
        //     if (right.Type == typeof(bool))
        //     {
        //         right = Linq.Expression.Condition(right, Linq.Expression.Constant(1.0), Linq.Expression.Constant(0.0));
        //     }
        // }

        var precedence = new[]
        {
            typeof(decimal),
            typeof(double),
            typeof(float),
            typeof(ulong),
            typeof(long),
            typeof(uint),
            typeof(int),
            typeof(ushort),
            typeof(short),
            typeof(byte),
            typeof(sbyte)
        };

        var l = Array.IndexOf(precedence, left.Type);
        var r = Array.IndexOf(precedence, right.Type);
        if (l >= 0 && r >= 0)
        {
            var type = precedence[Math.Min(l, r)];
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
        if (CaseInsensitiveComparer)
        {
            comparer = LinqExpression.Property(null, typeof(StringComparer), "CurrentCultureIgnoreCase");
        }
        else comparer = LinqExpression.Property(null, typeof(StringComparer), "Ordinal");

        if ((typeof(string) == left.Type || typeof(string) == right.Type))
        {
            switch (expressiontype)
            {
                case BinaryExpressionType.Equal: return LinqExpression.Call(comparer, typeof(StringComparer).GetRuntimeMethod("Equals",
                    [typeof(string), typeof(string)]), [left, right]);
                case BinaryExpressionType.NotEqual: return LinqExpression.Not(LinqExpression.Call(comparer, typeof(StringComparer).GetRuntimeMethod("Equals",
                    [typeof(string), typeof(string)]), [left, right]));
                case BinaryExpressionType.GreaterOrEqual: return LinqExpression.GreaterThanOrEqual(LinqExpression.Call(comparer, typeof(StringComparer).GetRuntimeMethod("Compare",
                    [typeof(string), typeof(string)]), [left, right]), LinqExpression.Constant(0));
                case BinaryExpressionType.LesserOrEqual: return LinqExpression.LessThanOrEqual(LinqExpression.Call(comparer, typeof(StringComparer).GetRuntimeMethod("Compare",
                    [typeof(string), typeof(string)]), [left, right]), LinqExpression.Constant(0));
                case BinaryExpressionType.Greater: return LinqExpression.GreaterThan(LinqExpression.Call(comparer, typeof(StringComparer).GetRuntimeMethod("Compare",
                    [typeof(string), typeof(string)]), [left, right]), LinqExpression.Constant(0));
                case BinaryExpressionType.Lesser: return LinqExpression.LessThan(LinqExpression.Call(comparer, typeof(StringComparer).GetRuntimeMethod("Compare",
                    [typeof(string), typeof(string)]), [left, right]), LinqExpression.Constant(0));
            }
        }
        return action(left, right);
    }

    private static LinqExpression UnwrapNullable(LinqExpression expression)
    {
        var ti = expression.Type.GetTypeInfo();
        if (ti.IsGenericType && ti.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            return LinqExpression.Condition(
                LinqExpression.Property(expression, "HasValue"),
                LinqExpression.Property(expression, "Value"),
                LinqExpression.Default(expression.Type.GetTypeInfo().GenericTypeArguments[0]));
        }

        return expression;
    }
}