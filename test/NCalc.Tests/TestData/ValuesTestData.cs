using System.Collections;

namespace NCalc.Tests.TestData;

public class ValuesTestData : IEnumerable<object[]>
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