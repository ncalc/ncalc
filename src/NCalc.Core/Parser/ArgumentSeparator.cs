namespace NCalc.Parser;

/// <summary>
/// Defines the available argument separator options for function arguments.
/// Each option specifies which character should be used to separate arguments in function calls.
/// </summary>
public enum ArgumentSeparator
{
    /// <summary>
    /// ArgSeparator_Semicolon: Uses semicolon (;) as the argument separator.
    /// Example: Max(1; 2; 3)
    /// </summary>
    Semicolon = 0,
    /// <summary>
    /// ArgSeparator_Colon: Uses colon (:) as the argument separator.
    /// Example: Max(1:2:3)
    /// </summary>
    Colon = 1,
    /// <summary>
    /// ArgSeparator_Comma: Uses comma (,) as the argument separator.
    /// Example: Max(1, 2, 3)
    /// </summary>
    Comma = 2
}
