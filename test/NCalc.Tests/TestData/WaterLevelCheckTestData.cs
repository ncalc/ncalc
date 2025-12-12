namespace NCalc.Tests.TestData;

public class WaterLevelCheckTestData
{
    public static IEnumerable<(string, bool, double)> GetTestData()
    {
        yield return ("(waterlevel > 1 AND waterlevel <= 3)", false, 3.2);
        yield return ("(waterlevel > 3 AND waterlevel <= 5)", true, 3.2);
        yield return ("(waterlevel > 1 AND waterlevel <= 3)", false, 3.1);
        yield return ("(waterlevel > 3 AND waterlevel <= 5)", true, 3.1);
        yield return ("(3 < waterlevel AND 5 >= waterlevel)", true, 3.1);
        yield return ("(3.2 < waterlevel AND 5.3 >= waterlevel)", true, 4);
    }
}