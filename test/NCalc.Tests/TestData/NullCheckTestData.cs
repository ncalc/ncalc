namespace NCalc.Tests.TestData;

public class NullCheckTestData : TheoryData<string, object>
{
    public NullCheckTestData()
    {
        Add("if((5 + null > 0), 1, 2)", 2);
        Add("if((5 - null > 0), 1, 2)", 2);
        Add("if((5 / null > 0), 1, 2)", 2);
        Add("if((5 * null > 0), 1, 2)", 2);
        Add("if((5 % null > 0), 1, 2)", 2);
    }

    public static IEnumerable<object[]> GetEnumerator() => new NullCheckTestData().Rows;
}
