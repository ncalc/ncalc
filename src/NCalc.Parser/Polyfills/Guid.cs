#if NETSTANDARD2_0

using System.Runtime.CompilerServices;

namespace NCalc;

internal static class GuidPolyfills
{
    extension(Guid)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Guid Parse(ReadOnlySpan<char> input) =>
            Guid.Parse(input.ToString());
    }
}
#endif