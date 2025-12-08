namespace NCalc.Tests.TestData;

public class BuiltInFunctionsTestData
{
    public static IEnumerable<(string, object, double?)> GetTestData()
    {
        yield return ("Abs(-1)", 1d, null);
        yield return ("Acos(1)", 0d, null);
        yield return ("Asin(0)", 0d, null);
        yield return ("Atan(0)", 0d, null);
        yield return ("Ceiling(1.5)", 2d, null);
        yield return ("Cos(0)", 1d, null);
        yield return ("Exp(0)", 1d, null);
        yield return ("Floor(1.5)", 1d, null);
        yield return ("IEEERemainder(3,2)", -1d, null);
        yield return ("Log(1,10)", 0d, null);
        yield return ("Ln(1)", 0d, null);
        yield return ("Log10(1)", 0d, null);
        yield return ("Pow(3,2)", 9d, null);
        yield return ("Round(3.222,2)", 3.22d, null);
        yield return ("Sign(-10)", -1, null);
        yield return ("Sin(0)", 0d, null);
        yield return ("Sqrt(4)", 2d, null);
        yield return ("Tan(0)", 0d, null);
        yield return ("Truncate(1.7)", 1d, null);
        yield return ("Atan2(-1,0)", -Math.PI / 2, 1e-16);
        yield return ("Atan2(1,0)", Math.PI / 2, 1e-16);
        yield return ("Atan2(0,-1)", Math.PI, 1e-16);
        yield return ("Atan2(0,1)", 0d, 1e-16);
        yield return ("Max(1,10)", 10, null);
        yield return ("Min(1,10)", 1, null);
    }
}