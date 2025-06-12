namespace NCalc.Visitors
{
    public record SerializationContext : ExpressionContextBase
    {
        public SerializationContext(ExpressionOptions options, CultureInfo? cultureInfo)
        {
            Options = options;
            CultureInfo = cultureInfo ?? CultureInfo.CurrentCulture;
        }

        public SerializationContext(ExpressionOptions options, CultureInfo? cultureInfo, AdvancedExpressionOptions? advancedOptions)
        {
            Options = options;
            CultureInfo = cultureInfo ?? CultureInfo.CurrentCulture;
            AdvancedOptions = advancedOptions;
        }
    }
}
