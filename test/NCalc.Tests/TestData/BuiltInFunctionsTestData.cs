namespace NCalc.Tests.TestData;

public class BuiltInFunctionsTestData : TheoryData<string, object, double?>
{
    public BuiltInFunctionsTestData()
    {
        Add("Abs(-1)", 1d, null);
        Add("Acos(1)", 0d, null);
        Add("Asin(0)", 0d, null);
        Add("Atan(0)", 0d, null);
        Add("Ceiling(1.5)", 2d, null);
        Add("Cos(0)", 1d, null);
        Add("Exp(0)", 1d, null);
        Add("Floor(1.5)", 1d, null);
        Add("IEEERemainder(3,2)", -1d, null);
        Add("Log(1,10)", 0d, null);
        Add("Ln(1)", 0d, null);
        Add("Log10(1)", 0d, null);
        Add("Pow(3,2)", 9d, null);
        Add("Round(3.222,2)", 3.22d, null);
        Add("Sign(-10)", -1, null);
        Add("Sin(0)", 0d, null);
        Add("Sqrt(4)", 2d, null);
        Add("Tan(0)", 0d, null);
        Add("Truncate(1.7)", 1d, null);
        Add("Atan2(-1,0)", -Math.PI / 2, 1e-16);
        Add("Atan2(1,0)", Math.PI / 2, 1e-16);
        Add("Atan2(0,-1)", Math.PI, 1e-16);
        Add("Atan2(0,1)", 0d, 1e-16);
        Add("Max(1,10)", 10, null);
        Add("Min(1,10)", 1, null);
    }
}