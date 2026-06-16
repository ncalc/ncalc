using System.Text.Json;
using NCalc.SourceGenerators.Models;

namespace NCalc.SourceGenerators.Helpers;

internal static class NumericTypeMetadataProvider
{
    private const string ResourceName = "NCalc.SourceGenerators.Metadata.NumericTypeMetadata.json";

    private static readonly Lazy<NumericTypeMetadata> Metadata = new(Load);

    public static NumericTypeMetadata GetMetadata()
    {
        return Metadata.Value;
    }

    private static NumericTypeMetadata Load()
    {
        using var stream = typeof(NumericTypeMetadataProvider).Assembly.GetManifestResourceStream(ResourceName);
        if (stream == null)
        {
            throw new InvalidOperationException($"Unable to find embedded resource '{ResourceName}'.");
        }

        return JsonSerializer.Deserialize<NumericTypeMetadata>(stream)
               ?? throw new InvalidOperationException("Unable to read numeric type metadata.");
    }
}