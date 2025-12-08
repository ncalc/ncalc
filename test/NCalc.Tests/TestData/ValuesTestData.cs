namespace NCalc.Tests.TestData;

public class ValuesTestData
{
    public static IEnumerable<(string, object)> GetTestData()
    {
        yield return ("123456", 123456);
        yield return (".2", 0.2d);
        yield return ("123.456", 123.456d);
        yield return ("123.", 123d);
        yield return ("123.0E2", 12300d);
        yield return ("true", true);
        yield return ("'true'", "true");
        yield return ("'azerty'", "azerty");
    }
}