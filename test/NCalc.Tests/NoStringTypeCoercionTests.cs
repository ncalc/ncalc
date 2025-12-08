namespace NCalc.Tests;

[Property("Category", "NoTypeCoercion")]
public class NoStringTypeCoercionTests
{
    [Test]
    [Arguments("'to' + 'to'", "toto")]
    [Arguments("'one' + 2", "one2")]
    [Arguments("'one' + 2.1", "one2.1")]
    [Arguments("2 + 'one'", "2one")]
    [Arguments("2.1 + 'one'", "2.1one")]
    [Arguments("'1' + '2'", "12")]
    [Arguments("'1.1' + '2'", "1.12")]
    [Arguments("'1' + '2.2'", "12.2")]
    [Arguments("1 + 2", 3)]
    [Arguments("1.5 + 2.5", 4.0)]
    [Arguments("'10' + 'mA - ' + (10 + 20) + 'mA'", "10mA - 30mA")]
    public async Task ShouldUseStringConcatenationIfEitherValueIsAString(string expression, object expected, CancellationToken cancellationToken)
    {
        var res = new Expression(expression, ExpressionOptions.NoStringTypeCoercion, CultureInfo.InvariantCulture)
            .Evaluate(cancellationToken);
        await Assert.That(res).IsEqualTo(expected);
    }

    [Test]
    [Arguments("1 in ('1')", false)]
    [Arguments("'1' in (1)", false)]
    [Arguments("1 in ('1',2)", false)]
    public async Task ShouldRespectDisabledCoercionAtInOperator(string expression, bool expected, CancellationToken cancellationToken)
    {
        await Assert.That(new Expression(expression, ExpressionOptions.NoStringTypeCoercion)
            .Evaluate(cancellationToken)).IsEqualTo(expected);
    }

    [Test]
    [Arguments("1 in ('1')", true)]
    [Arguments("'1' in (1)", true)]
    [Arguments("1 in ('1',2)", true)]
    [Arguments("'' in ('')", true)]
    [Arguments("'' not in ('')", false)]
    [Arguments("'' in ('','1')", true)]
    [Arguments("'1' in ('')", false)]
    [Arguments("'1' in ('', 1)", true)]
    [Arguments("('1' ='1'  AND '' IN ('6') OR ( '1' ='2'  AND '' IN ('6')) OR ( '1' ='6'  AND '' IN ('6')))", false)]
    public async Task ShouldRespectCoercionAtInOperator(string expression, bool expected, CancellationToken cancellationToken)
    {
        await Assert.That(new Expression(expression).Evaluate(cancellationToken)).IsEqualTo(expected);
    }
}