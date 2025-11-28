namespace NCalc.Tests;

public class FactorialTests
{
    [Fact]
    public void Factorial_Of_Zero()
    {
        var e = new Expression("0!");
        Assert.Equal(1, e.Evaluate(TestContext.Current.CancellationToken));
    }

    [Fact]
    public void Factorial_Of_Positive_Number()
    {
        var e = new Expression("5!");

        Assert.Equal(120, e.Evaluate(TestContext.Current.CancellationToken));
    }

    [Fact]
    public void Multiple_Factorials()
    {
        var e = new Expression("3!!");
        Assert.Equal(720, e.Evaluate(TestContext.Current.CancellationToken));
    }

    [Fact]
    public void Factorial_With_Addition()
    {
        var e = new Expression("3! + 2");
        Assert.Equal(8, e.Evaluate(TestContext.Current.CancellationToken));
    }

    [Fact]
    public void Factorial_With_Exponential()
    {
        var e = new Expression("3! ** 2");
        Assert.Equal(36d, e.Evaluate(TestContext.Current.CancellationToken));
    }

    [Fact]
    public void Factorial_In_Parentheses()
    {
        var e = new Expression("(4)! + 1");
        Assert.Equal(25, e.Evaluate(TestContext.Current.CancellationToken));
    }

    [Fact]
    public void Complex_Expression()
    {
        var e = new Expression("2 + 3! * 2");

        Assert.Equal(14, e.Evaluate(TestContext.Current.CancellationToken));
    }
}
