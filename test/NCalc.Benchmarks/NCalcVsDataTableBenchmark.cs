using System.Data;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;

namespace NCalc.Benchmarks;

[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net10_0)]
[RankColumn]
[CategoriesColumn]
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class NCalcVsDataTableBenchmark
{
    private Expression Expression { get; set; }

    private DataTable DataTable { get; set; }

    private const string ExpressionString =
        "(1089 = (1000 + 89)) AND 13 IN (1,2,3,4,5,6,7,8,9,10,11,12,13,14,15) AND 'INSERT' = 'INSERT'";

    [GlobalSetup]
    public void Setup()
    {
        Expression = new Expression(ExpressionString, ExpressionOptions.NoCache);
        DataTable = new DataTable();
    }

    [Benchmark]
    public object EvaluateNCalc()
    {
        return Expression.Evaluate();
    }

    [Benchmark]
    public object EvaluateDataTable()
    {
        return DataTable.Compute(ExpressionString, "");
    }
}