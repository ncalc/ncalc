using NCalc.Exceptions;
using NCalc.Factories;
using System.Threading.Tasks;

namespace NCalc.Tests;

[Property("Category", "Exceptions")]
public class ExceptionsTests
{
    [Test]
    [Arguments("ifs()")]
    [Arguments("ifs([divider] > 0)")]
    [Arguments("ifs([divider] > 0, [divider] / [divided])")]
    [Arguments("ifs([divider] > 0, [divider] / [divided], [divider < 0], [divider] + [divided])")]
    public void Ifs_With_Improper_Arguments_Should_Throw_Exceptions(string expression)
    {
        Assert.Throws<NCalcEvaluationException>(() => new Expression(expression)
            .Evaluate(CancellationToken.None));
    }

    [Test]
    [Arguments(". + 2")]
    [Arguments("(3 + 2")]
    [Arguments("42a")]
    [Arguments("42a.3")]
    [Arguments("42.3a")]
    [Arguments("42a.3b")]
    [Arguments("42.3e-5a")]
    [Arguments("42 + [a + 10")]
    [Arguments("42 a")]
    [Arguments("42 '")]
    [Arguments("Abs(-1) ]")]
    [Arguments("42. 3")]
    [Arguments("42 .3")]
    [Arguments("42 . 3")]
    public void Should_Throw_Parse_Exception(string expression)
    {
        Assert.Throws<NCalcParserException>(() => new Expression(expression).Evaluate(CancellationToken.None));
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
    public void Should_Display_Error_If_UncompatibleTypes()
    {
        var e = new Expression("(a > b) + 10")
        {
            Parameters =
            {
                ["a"] = 1,
                ["b"] = 2
            }
        };

        Assert.Throws<InvalidOperationException>(() =>
        {
            e.Evaluate(CancellationToken.None);
        });
    }

    [Test]
    public async Task Should_Throw_Exception_On_Lexer_Errors_Issue_6()
    {
        Assert.Throws<NCalcParserException>(() => LogicalExpressionFactory.Create("#t -chers", ct: CancellationToken.None));

        var dateSeparator = CultureInfo.CurrentCulture.DateTimeFormat.DateSeparator;
        string dateStr = $"#13{dateSeparator}13{dateSeparator}2222#";
        var invalidDateException = Assert.Throws<FormatException>(() =>
            LogicalExpressionFactory.Create(dateStr, ct: CancellationToken.None));
        await Assert.That(invalidDateException).IsTypeOf<FormatException>();

        //At v4, DateTime is better handled, and this should no longer cause errors.
        // https://github.com/ncalc/ncalc-async/issues/6
        try
        {
            LogicalExpressionFactory.Create("Format(\"{0:(###) ###-####}\", \"9999999999\")", ct: CancellationToken.None);
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
    [Arguments("5+-*10")]
    [Arguments("5+*10")]
    [Arguments("5/-*10")]
    public void Should_Throw_Issue_195(string expressionString)
    {
        var expression = new Expression(expressionString);
        Assert.Throws<NCalcParserException>(() =>
            expression.Evaluate(CancellationToken.None));
    }

    [Test]
    public void Should_Throw_Issue_208()
    {
        var expression = new Expression("1.3,4.5");
        Assert.Throws<NCalcParserException>(() =>
            expression.Evaluate(CancellationToken.None));
    }

    [Test]
    public void ShouldThrowExceptionWhenNullOrEmpty()
    {
        Assert.Throws<NCalcException>(() =>
            new Expression("").Evaluate(CancellationToken.None));
        Assert.Throws<NCalcException>(() =>
            new Expression((string)null).Evaluate(CancellationToken.None));
    }

    [Test]
    public void DateAdditionShouldThrowException()
    {
        var exp = new Expression("[a] + [b]");
        exp.Parameters["a"] = DateTime.Now;
        exp.Parameters["b"] = DateTime.Now.Date;

        Assert.Throws<InvalidOperationException>(() => exp.Evaluate(CancellationToken.None));
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
