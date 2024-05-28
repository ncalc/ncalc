namespace NCalc;

/// <summary>
/// Stores the context of an <see cref="Expression"/>, for both parsing and evaluation.
/// </summary>
public class ExpressionContext
{
    public ExpressionOptions Options { get; set; } = ExpressionOptions.None;
    public CultureInfo CultureInfo { get; set; } = CultureInfo.CurrentCulture;

    public ExpressionContext()
    {
        
    }

    public ExpressionContext(ExpressionOptions options, CultureInfo? cultureInfo)
    {
        Options = options;
        CultureInfo = cultureInfo ?? CultureInfo.CurrentCulture;
    }
    
    public static implicit operator ExpressionContext(ExpressionOptions options)
    {
        return new() { Options = options };
    }
    
    public static implicit operator ExpressionContext(CultureInfo cultureInfo)
    {
        return new() { CultureInfo = cultureInfo };
    }
}