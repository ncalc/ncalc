namespace NCalc.Helpers;

internal sealed class StringCoercionComparer : EqualityComparer<object?>
{
    public new static StringCoercionComparer Default { get; } = new();

    public override bool Equals(object? x, object? y)
    {
        if (x == null || y == null)
            return false;

        return x switch
        {
            int intX when y is string strY => intX.ToString() == strY,
            string strX when y is int intY => strX == intY.ToString(),
            _ => x.Equals(y)
        };
    }

    public override int GetHashCode(object? obj)
    {
        if (obj is int)
            return obj.ToString()?.GetHashCode() ?? string.Empty.GetHashCode();
        return obj?.GetHashCode() ?? string.Empty.GetHashCode();
    }
}