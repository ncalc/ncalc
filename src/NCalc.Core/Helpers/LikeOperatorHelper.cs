namespace NCalc.Helpers;

public static class LikeOperatorHelper
{
    /// <summary>
    /// Escapes a string so it is matched literally when used as a LIKE pattern.
    /// </summary>
    public static string EscapeLike(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        StringBuilder? result = null;

        for (var i = 0; i < value.Length; i++)
        {
            var character = value[i];

            if (character is '\\' or '%' or '_')
            {
                result ??= new StringBuilder(value.Length + 1).Append(value, 0, i);
                result.Append('\\');
            }

            result?.Append(character);
        }

        return result?.ToString() ?? value;
    }

    /// <summary>
    /// Determines whether a specified string matches a pattern using SQL-like wildcards.
    /// </summary>
    /// <param name="leftValue">The left operand.</param>
    /// <param name="rightValue">The right operand.</param>
    /// <param name="stringComparer">The comparer used for literal comparison.</param>
    /// <returns>
    /// <c>true</c> if the value matches the pattern; otherwise, <c>false</c>.
    /// </returns>
    public static bool Like(object? leftValue, object? rightValue, StringComparer stringComparer)
    {
        if (leftValue == null || rightValue == null)
            return false;

        var value = leftValue.ToString()!;
        var pattern = rightValue.ToString()!;
        return Like(value, pattern, stringComparer);
    }

    public static bool Like(string? value, string? pattern, StringComparer stringComparer)
    {
        if (value == null || pattern == null)
            return false;

        var comparison = CharacterComparison.For(stringComparer);

        if (!comparison.IsOrdinal)
        {
            value = value.Normalize(NormalizationForm.FormC);
            pattern = pattern.Normalize(NormalizationForm.FormC);
        }

        var valueIndex = 0;
        var patternIndex = 0;
        var lastPercentIndex = -1;
        var retryValueIndex = -1;

        while (valueIndex < value.Length)
        {
            if (patternIndex < pattern.Length)
            {
                var patternCharacter = pattern[patternIndex];

                switch (patternCharacter)
                {
                    case '%':
                        lastPercentIndex = patternIndex++;
                        retryValueIndex = valueIndex;
                        continue;
                    case '_':
                        valueIndex++;
                        patternIndex++;
                        continue;
                }

                var literalIndex = patternIndex;
                var patternAdvance = 1;

                if (patternCharacter == '\\' && patternIndex + 1 < pattern.Length)
                {
                    literalIndex++;
                    patternAdvance++;
                }

                if (comparison.CharactersEqual(value, valueIndex, pattern, literalIndex))
                {
                    valueIndex++;
                    patternIndex += patternAdvance;
                    continue;
                }
            }

            if (lastPercentIndex < 0)
                return false;

            patternIndex = lastPercentIndex + 1;
            valueIndex = ++retryValueIndex;
        }

        while (patternIndex < pattern.Length && pattern[patternIndex] == '%')
            patternIndex++;

        return patternIndex == pattern.Length;
    }

    /// <summary>
    /// Compares single characters of the value and the pattern the way the configured <see cref="StringComparer"/>
    /// compares one-character strings, without allocating those strings. The well-known ordinal and culture
    /// comparers are recognized up front; any other comparer falls back to comparing substrings.
    /// </summary>
    private readonly struct CharacterComparison
    {
        private enum Mode
        {
            Ordinal,
            OrdinalIgnoreCase,
            Culture,
            Custom
        }

        private readonly Mode _mode;
        private readonly CompareInfo? _compareInfo;
        private readonly CompareOptions _compareOptions;
        private readonly StringComparer? _comparer;

        private CharacterComparison(Mode mode, CompareInfo? compareInfo, CompareOptions compareOptions, StringComparer? comparer)
        {
            _mode = mode;
            _compareInfo = compareInfo;
            _compareOptions = compareOptions;
            _comparer = comparer;
        }

        public bool IsOrdinal => _mode is Mode.Ordinal or Mode.OrdinalIgnoreCase;

        public static CharacterComparison For(StringComparer comparer)
        {
            if (ReferenceEquals(comparer, StringComparer.Ordinal))
                return new CharacterComparison(Mode.Ordinal, null, CompareOptions.None, null);

            if (ReferenceEquals(comparer, StringComparer.OrdinalIgnoreCase))
                return new CharacterComparison(Mode.OrdinalIgnoreCase, null, CompareOptions.None, null);

            // The culture-aware comparers compare equal to another instance built for the same culture and options.
            var currentCulture = CultureInfo.CurrentCulture;
            if (comparer.Equals(StringComparer.CurrentCulture))
                return Culture(currentCulture, CompareOptions.None);

            if (comparer.Equals(StringComparer.CurrentCultureIgnoreCase))
                return Culture(currentCulture, CompareOptions.IgnoreCase);

            if (comparer.Equals(StringComparer.InvariantCulture))
                return Culture(CultureInfo.InvariantCulture, CompareOptions.None);

            if (comparer.Equals(StringComparer.InvariantCultureIgnoreCase))
                return Culture(CultureInfo.InvariantCulture, CompareOptions.IgnoreCase);

            return new CharacterComparison(Mode.Custom, null, CompareOptions.None, comparer);

            static CharacterComparison Culture(CultureInfo culture, CompareOptions options) =>
                new(Mode.Culture, culture.CompareInfo, options, null);
        }

        public bool CharactersEqual(string value, int valueIndex, string pattern, int patternIndex)
        {
            return _mode switch
            {
                Mode.Ordinal => value[valueIndex] == pattern[patternIndex],
                Mode.OrdinalIgnoreCase => char.ToUpperInvariant(value[valueIndex]) == char.ToUpperInvariant(pattern[patternIndex]),
                Mode.Culture => _compareInfo!.Compare(value, valueIndex, 1, pattern, patternIndex, 1, _compareOptions) == 0,
                _ => _comparer!.Equals(value.Substring(valueIndex, 1), pattern.Substring(patternIndex, 1))
            };
        }
    }
}
