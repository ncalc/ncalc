using Microsoft.Extensions.Logging;

namespace NCalc.Logging;

internal static partial class LogMessages
{
    [LoggerMessage(
        EventId = 0,
        Level = LogLevel.Information,
        Message = "Expression retrieved from cache: {Expression}")]
    public static partial void LogRetrievedFromCache(this ILogger logger, string expression);

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Expression added to cache: {Expression}")]
    public static partial void LogAddedToCache(this ILogger logger, string expression);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Information,
        Message = "Expression remove from cache: {Expression}")]
    public static partial void LogRemovedFromCache(this ILogger logger, string expression);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Error,
        Message = "Error creating logical expression: {ExpressionString}")]
    public static partial void LogErrorCreatingLogicalExpression(this ILogger logger, Exception exception,
        string expressionString);
}