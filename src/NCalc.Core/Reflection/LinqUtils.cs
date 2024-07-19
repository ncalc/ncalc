#nullable disable

using System.Reflection;
using NCalc.Helpers;

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
    public static Tuple<int, System.Linq.Expressions.Expression[]> PrepareMethodArgumentsIfValid(ParameterInfo[] parameters, System.Linq.Expressions.Expression[] arguments)
    {
        if (parameters.Length == 0 && arguments.Length == 0)
            return Tuple.Create(0, arguments);

        if (parameters.Length == 0)
            return null;

        var lastParameter = parameters.Last();
        var hasParamsKeyword = lastParameter.IsDefined(typeof(ParamArrayAttribute));
        if (hasParamsKeyword && parameters.Length > arguments.Length) return null;
        var newArguments = new System.Linq.Expressions.Expression[parameters.Length];
        System.Linq.Expressions.Expression[] paramsKeywordArgument = null;
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
            paramsKeywordArgument = new System.Linq.Expressions.Expression[arguments.Length - parameters.Length + 1];
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
            newArguments[paramsParameterPosition] = System.Linq.Expressions.Expression.NewArrayInit(paramsElementType, paramsKeywordArgument);
        }
        return Tuple.Create(functionMemberScore, newArguments);
    }

    private static bool TryCastImplicitly(Type from, Type to, ref System.Linq.Expressions.Expression argument)
    {
        var convertingFromPrimitiveType = TypeHelper.ImplicitPrimitiveConversionTable.TryGetValue(from, out var possibleConversions);
        if (!convertingFromPrimitiveType || !possibleConversions.Contains(to))
        {
            argument = null;
            return false;
        }
        argument = System.Linq.Expressions.Expression.Convert(argument, to);
        return true;
    }

    public static System.Linq.Expressions.Expression UnwrapNullable(System.Linq.Expressions.Expression expression)
    {
        var ti = expression.Type.GetTypeInfo();
        if (ti.IsGenericType && ti.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            return System.Linq.Expressions.Expression.Condition(
                System.Linq.Expressions.Expression.Property(expression, "HasValue"),
                System.Linq.Expressions.Expression.Property(expression, "Value"),
                System.Linq.Expressions.Expression.Default(expression.Type.GetTypeInfo().GenericTypeArguments[0]));
        }

        return expression;
    }
}