namespace NCalc.Tests.TestData;

public class EvaluationTestData
{
    public static IEnumerable<(string, object)> GetTestData()
    {
        yield return ("2 + 3 + 5", 10);
        yield return ("2 * 3 + 5", 11);
        yield return ("2 * (3 + 5)", 16);
        yield return ("2 * (2*(2*(2+1)))", 24);
        yield return ("10 % 3", 1);
        yield return ("true or false", true);
        yield return ("not true", false);
        yield return ("false || not (false and true)", true);
        yield return ("3 > 2 and 1 <= (3-2)", true);
        yield return ("3 % 2 != 10 % 3", false);
    }
}

public class ValuesEvaluationTestData
{
    public static IEnumerable<(string, object)> GetTestData()
    {
        yield return ("123456", 123456);
        yield return (".2", 0.2d);
        yield return ("123.456", 123.456d);
        yield return ("123.", 123d);
        yield return ("123.E2", 12300d);
        yield return ("true", true);
        yield return ("'true'", "true");
        yield return ("'azerty'", "azerty");
    }
}