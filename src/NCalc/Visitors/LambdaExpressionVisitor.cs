#nullable disable

using System.Reflection;
using NCalc.Domain;
using NCalc.Helpers;
using NCalc.Reflection;
using Linq = System.Linq.Expressions;
using LinqExpression = System.Linq.Expressions.Expression;
using LinqParameterExpression = System.Linq.Expressions.ParameterExpression;
namespace NCalc.Visitors;

internal class LambdaExpressionVistor : ILogicalExpressionVisitor
{
    private readonly IDictionary<string, object> _parameters;
    private LinqExpression _result;
    private readonly LinqExpression _context;
    private readonly ExpressionOptions _options;
    
    private bool Ordinal => _options.HasOption(ExpressionOptions.OrdinalStringComparer);

    private bool CaseInsensitiveComparer => _options.HasOption(ExpressionOptions.CaseInsensitiveStringComparer);
    
    //TODO:
    private static bool Checked => false; //{ get //{ return (_options & ExpressionOptions.OverflowProtection) == ExpressionOptions.OverflowProtection; } }

    // ReSharper disable once ConvertToPrimaryConstructor
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
                if (Checked) _result = WithCommonNumericType(left, right, LinqExpression.SubtractChecked);
                else _result = WithCommonNumericType(left, right, LinqExpression.Subtract);
                break;
            case BinaryExpressionType.Plus:
                if (Checked) _result = WithCommonNumericType(left, right, LinqExpression.AddChecked);
                else _result = WithCommonNumericType(left, right, LinqExpression.Add);
                break;
            case BinaryExpressionType.Modulo:
                _result = WithCommonNumericType(left, right, LinqExpression.Modulo);
                break;
            case BinaryExpressionType.Div:
                _result = WithCommonNumericType(left, right, LinqExpression.Divide);
                break;
            case BinaryExpressionType.Times:
                if (Checked) _result = WithCommonNumericType(left, right, LinqExpression.MultiplyChecked);
                else _result = WithCommonNumericType(left, right, LinqExpression.Multiply);
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
        if (functionName == "IF")
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
        else if (functionName == "IN")
        {
            var items = LinqExpression.NewArrayInit(args[0].Type,
                new ArraySegment<LinqExpression>(args, 1, args.Length - 1));
            var smi = typeof(Array).GetRuntimeMethod("IndexOf", new[] { typeof(Array), typeof(object) });
            var r = LinqExpression.Call(smi, LinqExpression.Convert(items, typeof(Array)), LinqExpression.Convert(args[0], typeof(object)));
            _result = LinqExpression.GreaterThanOrEqual(r, LinqExpression.Constant(0));
            return;
        }

        //Context methods take precedence over built-in functions because they're user-customisable.
        var mi = FindMethod(function.Identifier.Name, args);
        if (mi != null)
        {
            _result = LinqExpression.Call(_context, mi.BaseMethodInfo, mi.PreparedArguments);
            return;
        }

        void CheckArgumentsLengthForFunction(string funcStr, int argsNum, int argsNeed)
        {
            if (argsNum != argsNeed)
                throw new ArgumentException($"{funcStr} takes exactly {argsNeed} argument");
        };

        void MakeMathCallExpression(MathCallFunctionHelper.MathCallFunction mathMethod, int argsNumActual)
        {
            CheckArgumentsLengthForFunction(mathMethod.MathMethodInfo.Name, argsNumActual, mathMethod.ArgumentCount);

            _result = LinqExpression.Call(mathMethod.MathMethodInfo,
                Enumerable.Range(0, argsNumActual).Select( i => LinqExpression.Convert(args[i], typeof(double)) ));
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
                _result = LinqExpression.Call(MathCallFunctionHelper.Functions["ROUND"].MathMethodInfo, arg0, arg1, LinqExpression.Constant(rounding));
                break;
            default:
                // Regular handling
                if (MathCallFunctionHelper.Functions.TryGetValue(functionName, out var func))
                {
                    MakeMathCallExpression(func, actualNumArgs);
                }
                else
                {
                    throw new MissingMethodException($"method not found: {functionName}");
                }
                break;
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
        if (parameters.Length == 0 && arguments.Length == 0) return Tuple.Create(0, arguments);
        if (parameters.Length == 0) return null;

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
        var convertingFromPrimitiveType = TypeHelper.ImplicitPrimitiveConversionTable.TryGetValue(from, out var possibleConversions);
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
        
        if (_options.HasOption(ExpressionOptions.AllowBooleanCalculation))
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
            comparer = LinqExpression.Property(null, typeof(StringComparer), Ordinal ? "OrdinalIgnoreCase" : "CurrentCultureIgnoreCase");
        }
        else 
            comparer = LinqExpression.Property(null, typeof(StringComparer), "Ordinal");

        if ((typeof(string) == left.Type || typeof(string) == right.Type))
        {
            switch (expressiontype)
            {
                case BinaryExpressionType.Equal: return LinqExpression.Call(comparer, typeof(StringComparer).GetRuntimeMethod("Equals", new[] { typeof(string), typeof(string) }), new LinqExpression[] { left, right });
                case BinaryExpressionType.NotEqual: return LinqExpression.Not(LinqExpression.Call(comparer, typeof(StringComparer).GetRuntimeMethod("Equals", new[] { typeof(string), typeof(string) }), new LinqExpression[] { left, right }));
                case BinaryExpressionType.GreaterOrEqual: return LinqExpression.GreaterThanOrEqual(LinqExpression.Call(comparer, typeof(StringComparer).GetRuntimeMethod("Compare", new[] { typeof(string), typeof(string) }), new LinqExpression[] { left, right }), LinqExpression.Constant(0));
                case BinaryExpressionType.LesserOrEqual: return LinqExpression.LessThanOrEqual(LinqExpression.Call(comparer, typeof(StringComparer).GetRuntimeMethod("Compare", new[] { typeof(string), typeof(string) }), new LinqExpression[] { left, right }), LinqExpression.Constant(0));
                case BinaryExpressionType.Greater: return LinqExpression.GreaterThan(LinqExpression.Call(comparer, typeof(StringComparer).GetRuntimeMethod("Compare", new[] { typeof(string), typeof(string) }), new LinqExpression[] { left, right }), LinqExpression.Constant(0));
                case BinaryExpressionType.Lesser: return LinqExpression.LessThan(LinqExpression.Call(comparer, typeof(StringComparer).GetRuntimeMethod("Compare", new[] { typeof(string), typeof(string) }), new LinqExpression[] { left, right }), LinqExpression.Constant(0));
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