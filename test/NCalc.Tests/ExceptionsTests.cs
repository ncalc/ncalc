using NCalc.Exceptions;
using NCalc.Factories;

namespace NCalc.Tests;

[Property("Category", "Exceptions")]
public class ExceptionsTests
{
    [Test]
    [Arguments("ifs()")]
    [Arguments("ifs([divider] > 0)")]
    [Arguments("ifs([divider] > 0, [divider] / [divided])")]
    [Arguments("ifs([divider] > 0, [divider] / [divided], [divider < 0], [divider] + [divided])")]
    public async Task Ifs_With_Improper_Arguments_Should_Throw_Exceptions(string expression, CancellationToken cancellationToken)
    {
        await Assert.That(() => new Expression(expression)
            .Evaluate(cancellationToken)).ThrowsExactly<NCalcEvaluationException>();
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
    public async Task Should_Throw_Parse_Exception(string expression, CancellationToken cancellationToken)
    {
        await Assert.That(() => new Expression(expression).Evaluate(cancellationToken)).ThrowsExactly<NCalcParserException>();
    }

    [Test]
    public async Task Should_Detect_Syntax_Errors_Before_Evaluation(CancellationToken cancellationToken)
    {
        //Yes, I know at v3 the test was a + b * ( , but it's impossible to define an empty expression.
        var e = new Expression("a + b * ( 1 + 1");
        await Assert.That(e.Error).IsNull();
        await Assert.That(e.HasErrors(cancellationToken)).IsTrue();
        await Assert.That(e.Error).IsNotNull();

        e = new Expression("* b ");
        await Assert.That(e.Error).IsNull();
        await Assert.That(e.HasErrors(cancellationToken)).IsTrue();
        await Assert.That(e.Error).IsNotNull();
    }

    [Test]
    public async Task Should_Display_Error_If_UncompatibleTypes(CancellationToken cancellationToken)
    {
        var e = new Expression("(a > b) + 10")
        {
            Parameters =
            {
                ["a"] = 1,
                ["b"] = 2
            }
        };

        await Assert.That(() => e.Evaluate(cancellationToken)).ThrowsExactly<InvalidOperationException>();
    }

    [Test]
    public async Task Should_Throw_Exception_On_Lexer_Errors_Issue_6(CancellationToken cancellationToken)
    {
        await Assert.That(() => LogicalExpressionFactory.Create("#t -chers", ct: cancellationToken)).ThrowsExactly<NCalcParserException>();

        var dateSeparator = CultureInfo.CurrentCulture.DateTimeFormat.DateSeparator;
        string dateStr = $"#13{dateSeparator}13{dateSeparator}2222#";
        var invalidDateException = await Assert.That(() =>
            LogicalExpressionFactory.Create(dateStr, ct: cancellationToken)).ThrowsExactly<FormatException>();
        await Assert.That(invalidDateException).IsTypeOf<FormatException>();

        //At v4, DateTime is better handled, and this should no longer cause errors.
        // https://github.com/ncalc/ncalc-async/issues/6
        try
        {
            LogicalExpressionFactory.Create("Format(\"{0:(###) ###-####}\", \"9999999999\")", ct: cancellationToken);
        }
        catch(Exception ex)
        {
            Assert.Fail(ex.Message);
        }
    }

    [Test]
    public async Task ShouldProvideErrorPosition(CancellationToken cancellationToken)
    {
        var expression = new Expression("42a");
        try
        {
            await Assert.That(() => expression.Evaluate(cancellationToken))
                .ThrowsExactly<NCalcParserException>();
        }
        catch (NCalcParserException ex)
        {
            await Assert.That(ex.InnerException.Message)
                .IsEqualTo("Invalid token in expression at position (1:3)");
        }
    }

    [Test]
    public async Task Should_Throw_Function_Not_Found(CancellationToken cancellationToken)
    {
        var expression = new Expression("drop_database()");
        var exception = await Assert.That(() =>
            expression.Evaluate(cancellationToken)).ThrowsExactly<NCalcFunctionNotFoundException>();
        await Assert.That(exception.FunctionName).IsEqualTo("drop_database");
    }

    [Test]
    public async Task Should_Throw_Parameter_Not_Found(CancellationToken cancellationToken)
    {
        var expression = new Expression("{Name} == 'Spinella'");
        var exception = await Assert.That(() =>
            expression.Evaluate(cancellationToken)).ThrowsExactly<NCalcParameterNotDefinedException>();
        await Assert.That(exception.ParameterName).IsEqualTo("Name");
    }

    [Test]
    [Arguments("5+-*10")]
    [Arguments("5+*10")]
    [Arguments("5/-*10")]
    public async Task Should_Throw_Issue_195(string expressionString, CancellationToken cancellationToken)
    {
        var expression = new Expression(expressionString);
        await Assert.That(() =>
            expression.Evaluate(cancellationToken)).ThrowsExactly<NCalcParserException>();
    }

    [Test]
    public async Task Should_Throw_Issue_208(CancellationToken cancellationToken)
    {
        var expression = new Expression("1.3,4.5");
        await Assert.That(() =>
            expression.Evaluate(cancellationToken)).ThrowsExactly<NCalcParserException>();
    }

    [Test]
    public async Task ShouldThrowExceptionWhenNullOrEmpty(CancellationToken cancellationToken)
    {
        await Assert.That(() =>
            new Expression("").Evaluate(cancellationToken)).ThrowsExactly<NCalcException>();
        await Assert.That(() =>
            new Expression((string)null).Evaluate(cancellationToken)).ThrowsExactly<NCalcException>();
    }

    [Test]
    public async Task DateAdditionShouldThrowException(CancellationToken cancellationToken)
    {
        var exp = new Expression("[a] + [b]");
        exp.Parameters["a"] = DateTime.Now;
        exp.Parameters["b"] = DateTime.Now.Date;

        await Assert.That(() => exp.Evaluate(cancellationToken)).ThrowsExactly<InvalidOperationException>();
    }

    [Test]
    public async Task ShouldCancelEvaluation()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var exp = new Expression("11 + 33", ExpressionOptions.None);
        await Assert.That(() => exp.Evaluate(cts.Token)).ThrowsExactly<NCalcParserException>();
    }
}