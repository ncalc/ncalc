using NCalc.Exceptions;

namespace NCalc.Helpers;

public static class ParametersHelper
{
    public static Dictionary<string, IEnumerator> GetEnumerators(IDictionary<string,object?> parameters, out int? size)
    {
        var parameterEnumerators = new Dictionary<string, IEnumerator>();
        size = null;
        
        foreach (var parameter in parameters)
        {
            if (parameter.Value is not IEnumerable enumerable) 
                continue;
            
            var list = enumerable as List<object> ?? enumerable.Cast<object>().ToList();
            parameterEnumerators.Add(parameter.Key, list.GetEnumerator());

            var localSize = list.Count;

            if (size == null)
                size = localSize;
            else if (localSize != size)
            {
                throw new NCalcException(
                    "When IterateParameters option is used, IEnumerable parameters must have the same number of items");
            }
        }

        return parameterEnumerators;
    }
}