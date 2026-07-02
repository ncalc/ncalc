namespace NCalc.Exceptions;

public sealed class NCalcCastException :
    InvalidCastException
{
    public object? SourceValue { get; }
    public Type? TargetType { get; }

    // ReSharper disable once ConvertToPrimaryConstructor
    public NCalcCastException(object? sourceValue, Type targetType, Exception? innerException = null)
        : base($"Can't cast '{sourceValue}' to {targetType.Name}", innerException)
    {
        SourceValue = sourceValue;
        TargetType = targetType;
    }
}