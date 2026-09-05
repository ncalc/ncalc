namespace NCalc.Tests;

public class FactorialTests
{
    [Test]
    [Arguments("171!")]
    [Arguments("99999999999999!")]
    [Arguments("9223372036854775807!")]
    [Arguments("1.5e16!")]
    public void Should_Reject_Factorial_Inputs_Above_The_Safe_Limit(string expression)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Expression(expression).Evaluate(CancellationToken.None));
    }

    [Test]
    public async Task Factorial_Of_Zero()
    {
        await Assert.That("0!").Expression().IsEqualTo(1);
    }

    [Test]
    public async Task Factorial_Of_Positive_Number()
    {
        await Assert.That("5!").Expression().IsEqualTo(120);
    }

    [Test]
    public async Task Multiple_Factorials()
    {
        await Assert.That("3!!").Expression().IsEqualTo(720);
    }

    [Test]
    public async Task Factorial_With_Addition()
    {
        await Assert.That("3! + 2").Expression().IsEqualTo(8);
    }

    [Test]
    public async Task Factorial_With_Exponential()
    {
        await Assert.That("3! ** 2").Expression().IsEqualTo(36d);
    }

    [Test]
    public async Task Factorial_In_Parentheses()
    {
        await Assert.That("(4)! + 1").Expression().IsEqualTo(25);
    }

    [Test]
    public async Task Complex_Expression()
    {
        await Assert.That("2 + 3! * 2").Expression().IsEqualTo(14);
    }
}
