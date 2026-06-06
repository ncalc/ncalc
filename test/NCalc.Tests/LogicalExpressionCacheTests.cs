using System.Reflection;
using NCalc.Cache;
using NCalc.Factories;

namespace NCalc.Tests;

[Property("Category", "Cache")]
public class LogicalExpressionCacheTests
{
    [Test]
    public async Task Should_ReturnCachedExpression()
    {
        var cache = new LogicalExpressionCache();
        var expression = LogicalExpressionFactory.Create("1 + 1");

        cache.Set("1 + 1", expression);

        await Assert.That(cache.TryGetValue("1 + 1", out var cachedExpression)).IsTrue();
        await Assert.That(cachedExpression).IsSameReferenceAs(expression);
    }

    [Test]
    public async Task Should_EvictLeastRecentlyUsedExpression()
    {
        var cache = new LogicalExpressionCache();
        const int capacity = 128;

        for (var i = 0; i < capacity; i++)
        {
            var expressionText = $"value = {i}";
            cache.Set(expressionText, LogicalExpressionFactory.Create(expressionText));
        }

        await Assert.That(cache.TryGetValue("value = 0", out _)).IsTrue();

        cache.Set("value = 128", LogicalExpressionFactory.Create("value = 128"));

        await Assert.That(cache.TryGetValue("value = 0", out _)).IsTrue();
        await Assert.That(cache.TryGetValue("value = 1", out _)).IsFalse();
        await Assert.That(cache.TryGetValue("value = 128", out _)).IsTrue();
    }

    [Test]
    public async Task Should_ReadDefaultCapacityFromAppContext()
    {
        const string key = "NCalc.LogicalExpressionCache.DefaultCapacity";
        const string value = "7";

        var previousValue = AppContext.GetData(key);

        try
        {
            AppContext.SetData(key, value);

            var cache = new LogicalExpressionCache();
            var capacityField = typeof(LogicalExpressionCache).GetField("_capacity", BindingFlags.Instance | BindingFlags.NonPublic);

            await Assert.That(capacityField).IsNotNull();
            await Assert.That(capacityField!.GetValue(cache)).IsEqualTo(7);
        }
        finally
        {
            AppContext.SetData(key, previousValue);
        }
    }
}
