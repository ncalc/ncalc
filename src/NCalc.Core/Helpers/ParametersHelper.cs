using NCalc.Exceptions;

namespace NCalc.Helpers;

/// <summary>
/// Provides helper methods for handling parameters.
/// </summary>
public static class ParametersHelper
{
    /// <summary>
    /// Gets enumerators for the IEnumerable parameters and checks that they all have the same number of items.
    /// </summary>
    /// <param name="parameters">The dictionary of parameters.</param>
    /// <param name="size">The size of the enumerable, if any. Set to null if there are no IEnumerable parameters.</param>
    /// <returns>A dictionary of parameter names and their corresponding enumerators.</returns>
    /// <exception cref="NCalcException">Thrown when the IEnumerable parameters do not have the same number of items.</exception>
    public static Dictionary<string, IEnumerator> GetEnumerators(IDictionary<string, object?> parameters, out int? size)
    {
        var parameterEnumerators = new Dictionary<string, IEnumerator>();
        size = null;

        foreach (var parameter in parameters)
        {
            if (parameter.Value is IEnumerable enumerable)
            {
                var list = enumerable as List<object> ?? enumerable.Cast<object>().ToList();
                parameterEnumerators.Add(parameter.Key, list.GetEnumerator());

                var localSize = list.Count;

                if (size == null)
                {
                    size = localSize;
                }
                else if (localSize != size)
                {
                    throw new NCalcException(
                        "When IterateParameters option is used, IEnumerable parameters must have the same number of items");
                }
            }
        }

        return parameterEnumerators;
    }
}