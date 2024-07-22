namespace NCalc.Cache.Configuration;

public class LogicalExpressionMemoryCacheOptions
{
    /// <summary>
    /// The duration of the cache relative to now.
    /// Default Value: 15 minutes.
    /// </summary>
    public TimeSpan AbsoluteExpirationRelativeToNow { get; set; } = TimeSpan.FromMinutes(15);
}