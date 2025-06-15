namespace NCalc.Tests.TestData;

public class ValuesTestData : TheoryData<string, object>
{
    public ValuesTestData()
    {
        Add("123456", 123456);
        Add(".2", 0.2d);
        Add("123.456", 123.456d);
        Add("123.", 123d);
        //Add("123.E2", 12300d);
        Add("true", true);
        Add("'true'", "true");
        Add("'azerty'", "azerty");
    }
}