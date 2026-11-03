#if !NET6_0_OR_GREATER

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace NCalc;

internal static class ArgumentNullExceptionPolyfills
{
    extension(ArgumentNullException)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIfNull(
            [NotNull] object? argument,
            [CallerArgumentExpression(nameof(argument))] string? paramName = null)
        {
            if (argument is null)
                throw new ArgumentNullException(paramName);
        }
    }
}
#endif