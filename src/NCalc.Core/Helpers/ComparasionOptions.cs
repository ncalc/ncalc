namespace NCalc.Helpers;

public readonly struct ComparasionOptions
{
    public required CultureInfo CultureInfo { get; init; }
    public required bool IsCaseInsensitive { get; init; }
    public required bool IsOrdinal { get; init; }

    public void Deconstruct(out CultureInfo cultureInfo, out bool isCaseInsensitive, out bool isOrdinal)
    {
        cultureInfo = CultureInfo;
        isCaseInsensitive = IsCaseInsensitive;
        isOrdinal = IsOrdinal;
    }
}