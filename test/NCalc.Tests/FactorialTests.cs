namespace NCalc.Tests;

public class FactorialTests
{
    [Fact]
    public void Factorial_Of_Zero()
    {
        Assert.Expression(1, "0!");
    }

    [Fact]
    public void Factorial_Of_Positive_Number()
    {
        Assert.Expression(120, "5!");
    }

    [Fact]
    public void Multiple_Factorials()
    {
        Assert.Expression(720, "3!!");
    }

    [Fact]
    public void Factorial_With_Addition()
    {
        Assert.Expression(8, "3! + 2");
    }

    [Fact]
    public void Factorial_With_Exponential()
    {
        Assert.Expression(36d, "3! ** 2");
    }

    [Fact]
    public void Factorial_In_Parentheses()
    {
        Assert.Expression(25, "(4)! + 1");
    }

    [Fact]
    public void Complex_Expression()
    {
        Assert.Expression(14, "2 + 3! * 2");
    }
}