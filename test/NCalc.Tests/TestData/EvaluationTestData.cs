using System.Collections;

namespace NCalc.Tests.TestData;

public class EvaluationTestData : IEnumerable<object[]>
{
    public static readonly Dictionary<string, object> Data = new()
    {
        { "2 + 3 + 5", 10 },
        { "2 * 3 + 5", 11 },
        { "2 * (3 + 5)", 16 },
        { "2 * (2*(2*(2+1)))", 24 },
        { "10 % 3", 1 },
        { "true or false", true },
        { "not true", false },
        { "false || not (false and true)", true },
        { "3 > 2 and 1 <= (3-2)", true },
        { "3 % 2 != 10 % 3", false }
    };

    public EvaluationTestData()
    {
    }

    public IEnumerator<object[]> GetEnumerator()
    {
        foreach (var kvp in Data)
        {
            yield return [kvp.Key, kvp.Value];
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public class ValuesEvaluationTestData : IEnumerable<object[]>
{
    public static readonly Dictionary<string, object> Data = new()
    {
        { "123456", 123456 },
        { ".2", 0.2d },
        { "123.456", 123.456d },
        { "123.", 123d },
        { "123.E2", 12300d },
        { "true", true },
        { "'true'", "true" },
        { "'azerty'", "azerty" }
    };

    public IEnumerator<object[]> GetEnumerator()
    {
        foreach (var kvp in Data)
        {
            yield return new object[] { kvp.Key, kvp.Value };
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}