namespace NCalc.Tests;

public class FactorialTests
{
    [Test]
    public async Task Factorial_Of_Zero(CancellationToken cancellationToken)
    {
        var e = new Expression("0!");
        await Assert.That(e.Evaluate(cancellationToken)).IsEqualTo(1);
    }

    [Test]
    public async Task Factorial_Of_Positive_Number(CancellationToken cancellationToken)
    {
        var e = new Expression("5!");

        await Assert.That(e.Evaluate(cancellationToken)).IsEqualTo(120);
    }

    [Test]
    public async Task Multiple_Factorials(CancellationToken cancellationToken)
    {
        var e = new Expression("3!!");
        await Assert.That(e.Evaluate(cancellationToken)).IsEqualTo(720);
    }

    [Test]
    public async Task Factorial_With_Addition(CancellationToken cancellationToken)
    {
        var e = new Expression("3! + 2");
        await Assert.That(e.Evaluate(cancellationToken)).IsEqualTo(8);
    }

    [Test]
    public async Task Factorial_With_Exponential(CancellationToken cancellationToken)
    {
        var e = new Expression("3! ** 2");
        await Assert.That(e.Evaluate(cancellationToken)).IsEqualTo(36d);
    }

    [Test]
    public async Task Factorial_In_Parentheses(CancellationToken cancellationToken)
    {
        var e = new Expression("(4)! + 1");
        await Assert.That(e.Evaluate(cancellationToken)).IsEqualTo(25);
    }

    [Test]
    public async Task Complex_Expression(CancellationToken cancellationToken)
    {
        var e = new Expression("2 + 3! * 2");
        await Assert.That(e.Evaluate(cancellationToken)).IsEqualTo(14);
    }
}