namespace NCalc.Tests.TestData;

public abstract class TheoryData
{
    private readonly List<object[]> _rows = [];

    protected void AddRow(params object[] row) => _rows.Add(row);

    public IEnumerable<object[]> Rows => _rows;
}

public abstract class TheoryData<T1> : TheoryData
{
    protected void Add(T1 item1) => AddRow(item1);
}

public abstract class TheoryData<T1, T2> : TheoryData
{
    protected void Add(T1 item1, T2 item2) => AddRow(item1, item2);
}

public abstract class TheoryData<T1, T2, T3> : TheoryData
{
    protected void Add(T1 item1, T2 item2, T3 item3) => AddRow(item1, item2, item3);
}
