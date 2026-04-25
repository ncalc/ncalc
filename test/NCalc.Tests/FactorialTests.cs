namespace NCalc.Tests;

public class FactorialTests
{
    [Test]
    public void Factorial_Of_Zero()
    {
        Assert.Expression(1, "0!");
    }

    [Test]
    public void Factorial_Of_Positive_Number()
    {
        Assert.Expression(120, "5!");
    }

    [Test]
    public void Multiple_Factorials()
    {
        Assert.Expression(720, "3!!");
    }

    [Test]
    public void Factorial_With_Addition()
    {
        Assert.Expression(8, "3! + 2");
    }

    [Test]
    public void Factorial_With_Exponential()
    {
        Assert.Expression(36d, "3! ** 2");
    }

    [Test]
    public void Factorial_In_Parentheses()
    {
        Assert.Expression(25, "(4)! + 1");
    }

    [Test]
    public void Complex_Expression()
    {
        Assert.Expression(14, "2 + 3! * 2");
    }
}