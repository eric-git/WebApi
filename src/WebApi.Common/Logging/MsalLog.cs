using Microsoft.Extensions.Logging;
using static WebApi.Common.Logging.Constants;

namespace WebApi.Common.Logging;

public static partial class MsalLog
{
    [LoggerMessage(
        EventId = MsalError,
        EventName = $"{nameof(MsalEvents)}.{nameof(MsalError)}",
        Level = LogLevel.Error,
        Message = "[MSAL] {Message}")]
    public static partial void Error(
        ILogger logger,
        string message);

    [LoggerMessage(
        EventId = MsalWarning,
        EventName = $"{nameof(MsalEvents)}.{nameof(MsalWarning)}",
        Level = LogLevel.Warning,
        Message = "[MSAL] {Message}")]
    public static partial void Warning(
        ILogger logger,
        string message);

    [LoggerMessage(
        EventId = MsalInfo,
        EventName = $"{nameof(MsalEvents)}.{nameof(MsalInfo)}",
        Level = LogLevel.Information,
        Message = "[MSAL] {Message}")]
    public static partial void Info(
        ILogger logger,
        string message);

    [LoggerMessage(
        EventId = MsalDebug,
        EventName = $"{nameof(MsalEvents)}.{nameof(MsalDebug)}",
        Level = LogLevel.Debug,
        Message = "[MSAL] {Message}")]
    public static partial void Debug(
        ILogger logger,
        string message);
}