using Microsoft.Extensions.Logging;

namespace NCalc.Logging;

internal static class DefaultLoggerFactory
{
    public static ILoggerFactory Value { get; }

    static DefaultLoggerFactory()
    {
        Value = LoggerFactory.Create(options =>
        {
            if (AppContext.TryGetSwitch("NCalc.Logging.EnableConsole", out var enableConsole) && enableConsole)
            {
                options.AddConsole();
            }

            if (AppContext.TryGetSwitch("NCalc.Logging.EnableTrace", out var enableTrace) && enableTrace)
            {
                options.AddTraceSource("NCalc");
            }
        });
    }
}