namespace NCalc.Tests.TestData;

public class EvaluationTestData : TheoryData<string, object>
{
    public EvaluationTestData()
    {
        Add("2 + 3 + 5", 10);
        Add("2 * 3 + 5", 11);
        Add("2 * (3 + 5)", 16);
        Add("2 * (2*(2*(2+1)))", 24);
        Add("10 % 3", 1);
        Add("true or false", true);
        Add("not true", false);
        Add("false || not (false and true)", true);
        Add("3 > 2 and 1 <= (3-2)", true);
        Add("3 % 2 != 10 % 3", false);
        Add("2 > (3 + 5)", false);
    }
}

public class ValuesEvaluationTestData : TheoryData<string, object>
{
    public ValuesEvaluationTestData()
    {
        Add("123456", 123456);
        Add(".2", 0.2d);
        Add("123.456", 123.456d);
        Add("123.", 123d);
        Add("123.E2", 12300d);
        Add("true", true);
        Add("'true'", "true");
        Add("'azerty'", "azerty");
    }
}