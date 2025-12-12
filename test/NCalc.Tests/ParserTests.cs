using NCalc.Domain;
using NCalc.Factories;
using NCalc.Parser;

namespace NCalc.Tests;

[Property("Category", "Parser")]
public class ParserTests
{
    [Test]
    [Arguments("11+33 ", 44)]
    [Arguments(" 11+33", 44)]
    [Arguments(" 11+33 ", 44)]
    [Arguments("0.0-1.1", -1.1)]
    public async Task ShouldIgnoreWhitespacesIssue222(string formula, object expectedValue, CancellationToken cancellationToken)
    {
        var expression = new Expression(formula, CultureInfo.InvariantCulture);
        var result = expression.Evaluate(cancellationToken);

        await Assert.That(result).IsEqualTo(expectedValue);
    }

    [Test]
    [Arguments("not( true )", false)]
    [Arguments("not ( true )", false)]
    [Arguments("not(true)", false)]
    [Arguments(" not(true) ", false)]
    public async Task NotBehaviorIssue226(string formula, object expectedValue, CancellationToken cancellationToken)
    {
        var expression = new Expression(formula, CultureInfo.InvariantCulture);
        var result = expression.Evaluate(cancellationToken);

        await Assert.That(result).IsEqualTo(expectedValue);
    }

    [Test]
    public async Task ShouldHandleNewLines(CancellationToken cancellationToken)
    {
        const string formula = """
                               2+3


                               """;

        var expression = new Expression(formula, CultureInfo.InvariantCulture);
        var result = expression.Evaluate(cancellationToken);

        await Assert.That(result).IsEqualTo(5);
    }

    [Test]
    public async Task RequireClosingAtIdentifiersIssue244(CancellationToken cancellationToken)
    {
        const string formula = "[{Diagnostic}.Data]";

        var logicalExpression = LogicalExpressionFactory.Create(formula, ct: cancellationToken);

        await Assert.That(logicalExpression).IsTypeOf<Identifier>();

        await Assert.That(((Identifier)logicalExpression).Name).IsEqualTo("{Diagnostic}.Data");
    }

    [Test]
    public async Task AllowCharValues(CancellationToken cancellationToken)
    {
        const string formula = "'c'";

        var logicalExpression = LogicalExpressionFactory.Create(formula, ExpressionOptions.AllowCharValues, ct: cancellationToken);

        await Assert.That(logicalExpression).IsTypeOf<ValueExpression>();

        await Assert.That(((ValueExpression)logicalExpression).Value).IsEqualTo('c');
    }

    [Arguments("(1+2)*3", 9)]
    [Arguments("(8 * 8) + 1", 65)]
    [Arguments("1 + 1", 2)]
    [Arguments("-1 - 1", -2)]
    [Test]
    public async Task ShouldHandleBinaryExpression(string formula, int expectedResult, CancellationToken cancellationToken)
    {
        var logicalExpression = LogicalExpressionFactory.Create(formula, ct: cancellationToken);

        await Assert.That(logicalExpression).IsTypeOf<BinaryExpression>();

        var expression = new Expression(logicalExpression);

        await Assert.That(expression.Evaluate(cancellationToken)).IsEqualTo(expectedResult);
    }

    [Arguments("(1,2,3,4,5)", 5)]
    [Arguments("()", 0)]
    [Arguments("('Hello', func())", 2)]
    [Test]
    public async Task ShouldParseLists(string formula, int arrayExpectedCount, CancellationToken cancellationToken)
    {
        var logicalExpression = LogicalExpressionFactory.Create(formula, ct: cancellationToken);

        await Assert.That(logicalExpression).IsTypeOf<LogicalExpressionList>();

        await Assert.That(((LogicalExpressionList)logicalExpression).Count).IsEqualTo(arrayExpectedCount);
    }

    [Arguments("78b1941f4e7941c9bef656fad7326538")]
    [Arguments("b1548bd5-2556-4d2a-9f47-bb8d421026dd")]
    [Arguments("f44e449f-b02f-4f81-96d8-9292c5623b8b")]
    [Arguments("db6ff6c1-8290-4bdb-8c32-b78f3f2f6231")]
    [Arguments("e21aefee-8ffe-422c-a03f-7beb60975992")]
    [Arguments("13f722a3-5139-4eda-bffb-ed32e8cc90d1")]
    [Arguments("58ede4c6-2323-4b1a-ad66-c062fccb8473")]
    [Arguments("F6A41126850F41489192C89A949B2392")]
    [Arguments("E3825D69DAC04CA88D73E8CC3C8ADA2F")]
    [Arguments("B0D256EB-E88C-4409-A80B-E7341295B9F1")]
    [Arguments("C785E8B3-D080-45AF-947E-3D8F246788A6")]
    [Test]
    public async Task ShouldParseGuids(string formula, CancellationToken cancellationToken)
    {
        var logicalExpression = LogicalExpressionFactory.Create(formula, ct: cancellationToken);

        await Assert.That(logicalExpression).IsTypeOf<ValueExpression>();

        await Assert.That(new Expression(logicalExpression).Evaluate(cancellationToken)).IsTypeOf<Guid>();
    }

    [Test]
    public async Task ShouldParseGuidInsideFunction(CancellationToken cancellationToken)
    {
        var logicalExpression = LogicalExpressionFactory.Create("getUser(78b1941f4e7941c9bef656fad7326538)", ct: cancellationToken);

        if (logicalExpression is Function function)
        {
            await Assert.That(function.Parameters[0] is ValueExpression { Value: Guid }).IsTrue();
        }
        else
        {
            Assert.Fail("Expression is not a Function");
        }
    }

    [Test]
    public async Task OperatorPriorityIssue337(CancellationToken cancellationToken)
    {
        await Assert.That((bool)new Expression("true or true and false").Evaluate(cancellationToken)!).IsTrue();
    }

    [Test]
    public async Task ShouldNotFailIssue372()
    {
        var chars = new List<string>();
        for (var c = 'a'; c < 'z'; ++c)
        {
            chars.Add(c.ToString());
        }

        for (var c = 'A'; c < 'Z'; ++c)
        {
            chars.Add(c.ToString());
        }

        var failed = new List<string>();
        foreach (var c in chars)
        {
            try
            {
                var context = new LogicalExpressionParserContext(c, ExpressionOptions.None);
                LogicalExpressionParser.Parse(context);
            }
            catch (Exception)
            {
                failed.Add(c);
            }
        }

        await Assert.That(failed).IsEmpty();
    }

    [Test]
    public async Task ShouldNotFailIssue399(CancellationToken cancellationToken)
    {
        bool exceptionThrown = false;

        try
        {
            const ExpressionOptions expressionOptions = ExpressionOptions.DecimalAsDefault | ExpressionOptions.NoCache;
            var expression = new Expression("0.3333333333333333333333 + 1.6666666666666666666667", expressionOptions, CultureInfo.InvariantCulture);
            var result = expression.Evaluate(cancellationToken);
        }
        catch
        {
            exceptionThrown = true;
        }

        await Assert.That(exceptionThrown).IsFalse();
    }
}