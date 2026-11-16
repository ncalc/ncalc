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
    [Arguments("13!")]
    [Arguments("170!")]
    public void Should_Throw_When_An_Int32_Factorial_Overflows(string expression)
    {
        // 13! is 6227020800, which used to wrap around to 1932053504.
        Assert.Throws<OverflowException>(() => new Expression(expression).Evaluate(CancellationToken.None));
    }

    [Test]
    public async Task Should_Compute_The_Largest_Int32_Factorial()
    {
        await Assert.That("12!").Expression().IsEqualTo(479001600);
    }

    [Test]
    public async Task Should_Compute_Int64_Factorials_Up_To_Twenty()
    {
        var expression = new Expression("[n]!");
        expression.Parameters["n"] = 20L;
        await Assert.That(expression.Evaluate(CancellationToken.None)).IsEqualTo(2432902008176640000L);
    }

    [Test]
    public void Should_Throw_When_An_Int64_Factorial_Overflows()
    {
        var expression = new Expression("[n]!");
        expression.Parameters["n"] = 21L;
        Assert.Throws<OverflowException>(() => expression.Evaluate(CancellationToken.None));
    }

    [Test]
    public async Task Should_Compute_Decimal_Factorials_Up_To_Twenty_Seven()
    {
        var expression = new Expression("[n]!");
        expression.Parameters["n"] = 27m;
        await Assert.That(expression.Evaluate(CancellationToken.None)).IsEqualTo(10888869450418352160768000000m);
    }

    [Test]
    public void Should_Throw_When_A_Decimal_Factorial_Overflows()
    {
        var expression = new Expression("[n]!");
        expression.Parameters["n"] = 28m;
        Assert.Throws<OverflowException>(() => expression.Evaluate(CancellationToken.None));
    }

    [Test]
    public async Task Should_Compute_Double_Factorials_Up_To_The_Limit()
    {
        var expression = new Expression("[n]!");
        expression.Parameters["n"] = 170d;
        await Assert.That(expression.Evaluate(CancellationToken.None)).IsEqualTo(7.257415615307994E+306);
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
