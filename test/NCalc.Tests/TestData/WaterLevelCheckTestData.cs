namespace NCalc.Tests.TestData;

public class WaterLevelCheckTestData : TheoryData<string, bool, double>
{
    public WaterLevelCheckTestData()
    {
        Add("(waterlevel > 1 AND waterlevel <= 3)", false, 3.2);
        Add("(waterlevel > 3 AND waterlevel <= 5)", true, 3.2);
        Add("(waterlevel > 1 AND waterlevel <= 3)", false, 3.1);
        Add("(waterlevel > 3 AND waterlevel <= 5)", true, 3.1);
        Add("(3 < waterlevel AND 5 >= waterlevel)", true, 3.1);
        Add("(3.2 < waterlevel AND 5.3 >= waterlevel)", true, 4);
    }
}