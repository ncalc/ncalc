using System.Threading.Tasks;
using NCalc.Helpers;

namespace NCalc.Tests;

[Property("Category", "Decimals")]
public class DecimalsTests
{
    [Test]
    public async Task ShouldUseMathNumberTypesWhenCoercingStringNumbers()
    {
        var cultureInfo = CultureInfo.InvariantCulture;

        await Assert.That(MathHelper.Add("2", "3", new MathOptions(), cultureInfo)).IsEqualTo(5d);
        await Assert.That(MathHelper.Add("2", "3", new MathOptions
        {
            FloatingPointNumberType = FloatingPointNumberType.Decimal
        }, cultureInfo)).IsEqualTo(5m);
        await Assert.That(MathHelper.Add("2", "3", new MathOptions
        {
            IntegerNumberType = IntegerNumberType.Int64
        }, cultureInfo)).IsEqualTo(5L);
    }

    [Test]
    public async Task ShouldUseFloatingPointNumberTypeWhenChoosingMathFunctionPrecision()
    {
        var doubleResult = MathHelper.Abs("-1", new MathOptions(), CultureInfo.InvariantCulture);
        var decimalResult = MathHelper.Abs("-1", new MathOptions
        {
            FloatingPointNumberType = FloatingPointNumberType.Decimal
        }, CultureInfo.InvariantCulture);

        await Assert.That(doubleResult).IsEqualTo(1d);
        await Assert.That(decimalResult).IsEqualTo(1m);
    }
}
