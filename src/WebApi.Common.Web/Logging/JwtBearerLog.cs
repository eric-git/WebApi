using Microsoft.Extensions.Logging;
using static WebApi.Common.Logging.Constants;

namespace WebApi.Common.Web.Logging;

public static partial class JwtBearerLog
{
    [LoggerMessage(
        EventId = JwtAuthenticationFailed,
        EventName = $"{nameof(JwtEvents)}.{nameof(JwtAuthenticationFailed)}",
        Level = LogLevel.Error,
        Message = "JWT authentication failed")]
    public static partial void AuthenticationFailed(
        ILogger logger,
        Exception exception);

    [LoggerMessage(
        EventId = JwtChallengeTriggered,
        EventName = $"{nameof(JwtEvents)}.{nameof(JwtChallengeTriggered)}",
        Level = LogLevel.Warning,
        Message = "JWT challenge triggered. Error: {Error}, Description: {Description}")]
    public static partial void ChallengeTriggered(
        ILogger logger,
        string? error,
        string? description);

    [LoggerMessage(
        EventId = JwtForbidden,
        EventName = $"{nameof(JwtEvents)}.{nameof(JwtForbidden)}",
        Level = LogLevel.Warning,
        Message = "JWT forbidden for user {User}")]
    public static partial void Forbidden(
        ILogger logger,
        string? user);

    [LoggerMessage(
        EventId = JwtMessageReceived,
        EventName = $"{nameof(JwtEvents)}.{nameof(JwtMessageReceived)}",
        Level = LogLevel.Information,
        Message = "Message received. Token: {Token}")]
    public static partial void MessageReceived(
        ILogger logger,
        string? token);

    [LoggerMessage(
        EventId = JwtTokenValidated,
        EventName = $"{nameof(JwtEvents)}.{nameof(JwtTokenValidated)}",
        Level = LogLevel.Information,
        Message = "Token validated for subject {Subject}")]
    public static partial void TokenValidated(
        ILogger logger,
        string? subject);
}