using NCalc.Exceptions;
using NCalc.Factories;
using System.Threading.Tasks;

namespace NCalc.Tests;

[Property("Category", "Exceptions")]
public class ExceptionsTests
{
    [Test]
    [Arguments("()>0.")]
    [Arguments("009&()")]
    [Arguments("0000000000000000<<()")]
    [Arguments("(' (' +'2(' , 2)/--1")]
    [Arguments("!() --19")]
    public async Task Should_Wrap_Invalid_Cast_During_Evaluation(string expression)
    {
        var exception = Assert.Throws<NCalcEvaluationException>(() => new Expression(expression)
            .Evaluate(CancellationToken.None));

        await Assert.That(exception.InnerException).IsTypeOf<InvalidCastException>();
    }

    [Test]
    public async Task Should_Wrap_Typed_Evaluation_Conversion_Failures()
    {
        var exception = Assert.Throws<NCalcCastException>(() => new Expression("'abc'")
            .Evaluate<int>(CancellationToken.None));

        await Assert.That(exception.SourceValue).IsEqualTo("abc");
        await Assert.That(exception.TargetType).IsEqualTo(typeof(int));
        await Assert.That(exception.InnerException).IsTypeOf<FormatException>();
    }

    [Test]
    public async Task Should_Wrap_Async_Typed_Evaluation_Conversion_Failures()
    {
        var expression = new Expression("'abc'");

        var exception = await Assert.ThrowsAsync<NCalcCastException>(async () =>
            await expression.EvaluateAsync<int>(CancellationToken.None));

        await Assert.That(exception.SourceValue).IsEqualTo("abc");
        await Assert.That(exception.TargetType).IsEqualTo(typeof(int));
        await Assert.That(exception.InnerException).IsTypeOf<FormatException>();
    }

    [Test]
    public async Task Should_Detect_Syntax_Errors_Before_Evaluation()
    {
        //Yes, I know at v3 the test was a + b * ( , but it's impossible to define an empty expression.
        var e = new Expression("a + b * ( 1 + 1");
        await Assert.That(e.Error).IsNull();
        await Assert.That(e.HasErrors(CancellationToken.None)).IsTrue();
        await Assert.That(e.Error).IsNotNull();

        e = new Expression("* b ");
        await Assert.That(e.Error).IsNull();
        await Assert.That(e.HasErrors(CancellationToken.None)).IsTrue();
        await Assert.That(e.Error).IsNotNull();
    }

    [Test]
    public async Task Should_Throw_Exception_On_Lexer_Errors_Issue_6()
    {
        Assert.Throws<NCalcParserException>(() => LogicalExpressionFactory.Create("#t -chers", cancellationToken: CancellationToken.None));

        var dateSeparator = CultureInfo.CurrentCulture.DateTimeFormat.DateSeparator;
        string dateStr = $"#13{dateSeparator}13{dateSeparator}2222#";
        var invalidDateException = Assert.Throws<FormatException>(() =>
            LogicalExpressionFactory.Create(dateStr, cancellationToken: CancellationToken.None));
        await Assert.That(invalidDateException).IsTypeOf<FormatException>();

        //At v4, DateTime is better handled, and this should no longer cause errors.
        // https://github.com/ncalc/ncalc-async/issues/6
        try
        {
            LogicalExpressionFactory.Create("Format(\"{0:(###) ###-####}\", \"9999999999\")", cancellationToken: CancellationToken.None);
        }
        catch
        {
            Assert.Fail("Assertion failure");
        }
    }

    [Test]
    public async Task ShouldProvideErrorPosition()
    {
        var expression = new Expression("42a");
        try
        {
            expression.Evaluate(CancellationToken.None);
            Assert.Fail("Expected a parser exception.");
        }
        catch (NCalcParserException ex)
        {
            await Assert.That(ex.InnerException.Message).IsEqualTo("Invalid token in expression at position (1:3)");
        }
    }

    [Test]
    public async Task Should_Throw_Function_Not_Found()
    {
        var expression = new Expression("drop_database()");
        var exception = Assert.Throws<NCalcFunctionNotFoundException>(() =>
            expression.Evaluate(CancellationToken.None));
        await Assert.That(exception.FunctionName).IsEqualTo("drop_database");
    }

    [Test]
    public async Task Should_Throw_Parameter_Not_Found()
    {
        var expression = new Expression("{Name} == 'Spinella'");
        var exception = Assert.Throws<NCalcParameterNotDefinedException>(() =>
            expression.Evaluate(CancellationToken.None));
        await Assert.That(exception.ParameterName).IsEqualTo("Name");
    }

    [Test]
    public void ShouldCancelEvaluation()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var exp = new Expression("11 + 33", ExpressionOptions.None);
        Assert.Throws<NCalcParserException>(() => exp.Evaluate(cts.Token));
    }
}
