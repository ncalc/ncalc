namespace NCalc.Helpers;

public static class LikeOperatorHelper
{
    /// <summary>
    /// Escapes a string so it is matched literally when used as a LIKE pattern.
    /// </summary>
    public static string EscapeLike(string value)
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));

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
    /// <param name="context">The context containing options for the comparison.</param>
    /// <returns>
    /// <c>true</c> if the value matches the pattern; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// The comparison is case-insensitive if the <see cref="ExpressionOptions.CaseInsensitiveStringComparer"/> flag is set in the <paramref name="context"/>.
    /// </remarks>
    public static bool Like(object? leftValue, object? rightValue, ExpressionContext context)
    {
        if (leftValue == null || rightValue == null)
            return false;

        var value = leftValue.ToString()!;
        var pattern = rightValue.ToString()!;
        var comparisonOptions = context.ComparisonOptions;

        return Like(value, pattern, comparisonOptions.IsCaseInsensitive, comparisonOptions.IsOrdinal, comparisonOptions.CultureInfo);
    }

    public static bool Like(string? value, string? pattern, bool ignoreCase, bool isOrdinal, CultureInfo cultureInfo)
    {
        if (value == null || pattern == null)
            return false;

        if (!isOrdinal)
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

                bool charactersEqual;
                if (!ignoreCase)
                {
                    charactersEqual = value[valueIndex] == pattern[literalIndex];
                }
                else
                {
                    charactersEqual = cultureInfo.CompareInfo.Compare(
                        value,
                        valueIndex,
                        1,
                        pattern,
                        literalIndex,
                        1,
                        CompareOptions.IgnoreCase) == 0;
                }

                if (charactersEqual)
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
}
