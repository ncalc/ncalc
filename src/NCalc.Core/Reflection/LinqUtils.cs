using System.Reflection;
using NCalc.Helpers;
using LinqExpression = System.Linq.Expressions.Expression;

namespace NCalc.Reflection;

public static class LinqUtils
{
    /// <summary>
    /// Returns a tuple where the first item is a score, and the second is a list of prepared arguments.
    /// Score is a simplified indicator of how close the arguments' types are to the parameters'. A score of 0 indicates a perfect match between arguments and parameters.
    /// Prepared arguments refers to having the arguments implicitly converted where necessary, and "params" arguments collated into one array.
    /// </summary>
    /// <param name="parameters"></param>
    /// <param name="arguments"></param>
    /// <returns></returns>
    public static Tuple<int, LinqExpression[]>? PrepareMethodArgumentsIfValid(ParameterInfo[] parameters, LinqExpression[] arguments)
    {
        if (parameters.Length == 0 && arguments.Length == 0)
            return Tuple.Create(0, arguments);

        if (parameters.Length == 0)
            return null;

        var lastParameter = parameters.Last();
        var hasParamsKeyword = lastParameter.IsDefined(typeof(ParamArrayAttribute));
        if (hasParamsKeyword && parameters.Length > arguments.Length)
            return null;

        var newArguments = new LinqExpression[parameters.Length];
        LinqExpression[]? paramsKeywordArgument = null;
        Type? paramsElementType = null;
        int paramsParameterPosition = 0;

        if (!hasParamsKeyword)
        {
            if (parameters.Length != arguments.Length)
                return null;
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
            var argument = arguments[i];
            var argumentType = argument.Type;

            var isParamsElement = hasParamsKeyword && i >= paramsParameterPosition;
            var parameterType = isParamsElement ? paramsElementType : parameters[i].ParameterType;

            if (argumentType != parameterType)
            {
                var canCastImplicitly = TryCastImplicitly(argumentType, parameterType!, ref argument);
                if (!canCastImplicitly)
                    return null;

                functionMemberScore++;
            }

            if (!isParamsElement)
            {
                newArguments[i] = argument!;
            }
            else
            {
                if (paramsKeywordArgument != null)
                    paramsKeywordArgument[i - paramsParameterPosition] = argument!;
            }
        }

        if (hasParamsKeyword)
        {
            newArguments[paramsParameterPosition] = LinqExpression.NewArrayInit(paramsElementType!, paramsKeywordArgument!);
        }

        return Tuple.Create(functionMemberScore, newArguments);
    }

    private static bool TryCastImplicitly(Type from, Type to, ref LinqExpression? argument)
    {
        if (argument == null)
            return false;

        var convertingFromPrimitiveType = TypeHelper.ImplicitPrimitiveConversionTable.TryGetValue(from, out var possibleConversions);
        if (!convertingFromPrimitiveType || !possibleConversions!.Contains(to))
        {
            argument = null;
            return false;
        }

        argument = LinqExpression.Convert(argument, to);
        return true;
    }

    public static LinqExpression UnwrapNullable(LinqExpression expression)
    {
        var type = expression.Type;
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            return LinqExpression.Condition(
                LinqExpression.Property(expression, "HasValue"),
                LinqExpression.Property(expression, "Value"),
                LinqExpression.Default(type.GenericTypeArguments[0]));
        }

        return expression;
    }
}