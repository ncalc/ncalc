namespace NCalc.Tests;

public class NestedExpressionTests
{
    [Test]
    public async Task Should_Not_Accumulate_Parent_Handlers_On_A_Child_Expression()
    {
        var calls = 0;
        var parent = new Expression("[inner] + g()");
        parent.Parameters["inner"] = new Expression("g()");
        parent.EvaluateFunction += (name, args) =>
        {
            if (name != "g")
                return;

            calls++;
            args.Result = 1;
        };

        for (var i = 0; i < 5; i++)
        {
            calls = 0;
            await Assert.That(parent.Evaluate(CancellationToken.None)).IsEqualTo(2);
            await Assert.That(calls).IsEqualTo(2);
        }
    }

    [Test]
    public async Task Should_Not_Accumulate_Parent_Handlers_On_A_Child_Expression_When_Evaluating_Asynchronously()
    {
        var calls = 0;
        var parent = new Expression("[inner] + g()");
        parent.Parameters["inner"] = new Expression("g()");
        parent.EvaluateFunction += (name, args) =>
        {
            if (name != "g")
                return;

            calls++;
            args.Result = 1;
        };

        for (var i = 0; i < 5; i++)
        {
            calls = 0;
            await Assert.That(await parent.EvaluateAsync(CancellationToken.None)).IsEqualTo(2);
            await Assert.That(calls).IsEqualTo(2);
        }
    }

    [Test]
    public async Task Should_Keep_The_Child_Expression_Own_Handlers()
    {
        var parentCalls = 0;
        var childCalls = 0;
        var child = new Expression("h() + g()");
        child.EvaluateFunction += (name, args) =>
        {
            if (name != "h")
                return;

            childCalls++;
            args.Result = 10;
        };

        var parent = new Expression("[inner]");
        parent.Parameters["inner"] = child;
        parent.EvaluateFunction += (name, args) =>
        {
            if (name != "g")
                return;

            parentCalls++;
            args.Result = 1;
        };

        for (var i = 0; i < 3; i++)
        {
            parentCalls = 0;
            childCalls = 0;
            await Assert.That(parent.Evaluate(CancellationToken.None)).IsEqualTo(11);
            await Assert.That(parentCalls).IsEqualTo(1);
            await Assert.That(childCalls).IsEqualTo(1);
        }
    }

    [Test]
    public async Task Should_Share_Parent_Parameters_With_A_Child_Expression()
    {
        var parent = new Expression("[inner] * 2");
        parent.Parameters["inner"] = new Expression("[x] + 1");
        parent.Parameters["x"] = 4;

        await Assert.That(parent.Evaluate(CancellationToken.None)).IsEqualTo(10);
        await Assert.That(parent.Evaluate(CancellationToken.None)).IsEqualTo(10);
    }
}
