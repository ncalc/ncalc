namespace NCalc.Parser;

/// <summary>
/// Defines the available argument separator options for function arguments.
/// Each option specifies which character should be used to separate arguments in function calls.
/// </summary>
public enum ArgumentSeparator
{
    /// <summary>
    /// Uses semicolon (;) as the argument separator.
    /// This is commonly used in European locales where comma is used as decimal separator.
    /// Example: Max(1; 2; 3)
    /// </summary>
    /// <value>Character: ;</value>
    Semicolon = 0,

    /// <summary>
    /// Uses colon (:) as the argument separator.
    /// This is less common but supported for specific use cases.
    /// Example: Max(1:2:3)
    /// </summary>
    /// <value>Character: :</value>
    Colon = 1,

    /// <summary>
    /// Uses comma (,) as the argument separator.
    /// This is the most common separator used in mathematical expressions and programming languages.
    /// Example: Max(1, 2, 3)
    /// </summary>
    /// <value>Character: ,</value>
    Comma = 2
}
