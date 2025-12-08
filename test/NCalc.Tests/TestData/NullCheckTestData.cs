namespace NCalc.Tests.TestData;

public class NullCheckTestData
{
    public static IEnumerable<(string, object)> GetTestData()
    {
        yield return ("if((5 + null > 0), 1, 2)", 2);
        yield return ("if((5 - null > 0), 1, 2)", 2);
        yield return ("if((5 / null > 0), 1, 2)", 2);
        yield return ("if((5 * null > 0), 1, 2)", 2);
        yield return ("if((5 % null > 0), 1, 2)", 2);
    }
}