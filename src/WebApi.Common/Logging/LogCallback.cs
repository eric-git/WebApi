using Microsoft.Extensions.Logging;

namespace WebApi.Common.Logging;

public delegate void LogCallback(LogLevel level, EventId eventId, string? message, params object?[] args);