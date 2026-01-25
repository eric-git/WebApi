using Microsoft.Extensions.Logging;
using WebApi.Common.Logging;
using static WebApi.Common.Constants;

namespace WebApi.Common;

public static class ErrorHandlingExtensions
{
    public static void HandleException(string source, ILoggerFactory loggerFactory, Exception? exception)
    {
        ArgumentNullException.ThrowIfNull(loggerFactory);
        var logger = loggerFactory.CreateLogger(GlobalExceptionHandler);
        LogCallback logCallback = logger.Log;
        logCallback(LogLevel.Error,
            new EventId(ErrorOccurred, nameof(ErrorOccurred)),
            "{Source}\n{Message}",
            source,
            exception?.Message);
    }
}