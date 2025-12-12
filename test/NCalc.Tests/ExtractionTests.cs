using NCalc.Parser;

namespace NCalc.Tests;

[Property("Category", "Extraction")]
public class ExtractionTests
{
    [Test]
    public async Task ShouldGetParametersIssue103(CancellationToken cancellationToken)
    {
        var expression = new Expression("PageState == 'LIST' && a == 1 && customFunction() == true || in(1 + 1, 1, 2, 3)", ExpressionOptions.CaseInsensitiveStringComparer)
        {
            Parameters =
            {
                ["a"] = 1
            }
        };
        expression.DynamicParameters["PageState"] = _ => "List";
        expression.Functions["customfunction"] = _ => true;

        var parameters = expression.GetParameterNames(cancellationToken);
        await Assert.That(parameters).Contains("a");
        await Assert.That(parameters).Contains("PageState");
        await Assert.That(parameters.Count).IsEqualTo(2);
    }

    [Test]
    public async Task ShouldGetParametersOneTimeIssue141(CancellationToken cancellationToken)
    {
        var expression = new Expression("if(x=0,x,y)",
                ExpressionOptions.CaseInsensitiveStringComparer);
        var parameters = expression.GetParameterNames(cancellationToken);

        await Assert.That(parameters.Count).IsEqualTo(2);
    }

    [Test]
    public async Task ShouldGetParametersWithUnary(CancellationToken cancellationToken)
    {
        var expression = new Expression("-0.68");
        var p = expression.GetParameterNames(cancellationToken);
        await Assert.That(p).IsEmpty();
    }

    [Test]
    [Arguments("(a, b, c)", 3)]
    [Arguments("725 - 1 == result * secret_operation(secretValue)", 2)]
    [Arguments("getLightsaberColor(selectedJedi) == selectedColor", 2)]
    public async Task ShouldGetParameters(string formula, int expectedCount, CancellationToken cancellationToken)
    {
        var expression = new Expression(formula);
        var p = expression.GetParameterNames(cancellationToken);
        await Assert.That(p.Count).IsEqualTo(expectedCount);
    }

    [Arguments("(a, drop_database(), c) == toUpper(getName())", 3)]
    [Arguments("Abs(523/2) == Abs(523/2)", 1)]
    [Arguments("getLightsaberColor('Yoda') == selectedColor", 1)]
    [Test]
    public async Task ShouldGetFunctions(string formula, int expectedCount, CancellationToken cancellationToken)
    {
        var expression = new Expression(formula);
        var functions = expression.GetFunctionNames(cancellationToken);
        await Assert.That(functions.Count).IsEqualTo(expectedCount);
    }

    [Test]
    public async Task ShouldGetParametersInsideFunctionsIssue305(CancellationToken cancellationToken)
    {
        var expression = new Expression("if([Value] >= 50, 'background-color: #80ffcc;', null)", ExpressionOptions.AllowNullParameter);
        var parameters = expression.GetParameterNames(cancellationToken);
        await Assert.That(parameters.Count).IsEqualTo(2);
    }

    [Test]
    public async Task ShouldGetFunctionsInsideFunctionsIssue305(CancellationToken cancellationToken)
    {
        var expression = new Expression("if(getValue() >= 50, 'background-color: #80ffcc;', null)", ExpressionOptions.AllowNullParameter);
        var functions = expression.GetFunctionNames(cancellationToken);
        await Assert.That(functions.Count).IsEqualTo(2);
    }

    [Test]
    public async Task ShouldGetNestedFunctionsIssue334(CancellationToken cancellationToken)
    {
        const string expressionText = "[a] + GetTimeValue(if([c] > [d]; test([e] > 0; [g]; [h]); [f]); 1; 'sec')";
        var options = LogicalExpressionParserOptions.WithArgumentSeparator(ArgumentSeparator.Semicolon);
        var context = new LogicalExpressionParserContext(expressionText, ExpressionOptions.None)
        {
            ParserOptions = options
        };

        var logicalExpression = LogicalExpressionParser.Parse(context);
        var expression = new Expression(logicalExpression);
        var functions = expression.GetFunctionNames(cancellationToken);
        await Assert.That(functions).Contains("GetTimeValue");
        await Assert.That(functions).Contains("if");
        await Assert.That(functions).Contains("test");
    }
}