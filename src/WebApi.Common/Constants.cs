using System.Text.Encodings.Web;
using System.Text.Json;

namespace WebApi.Common;

public static class Constants
{
    public const string ClientCorrelationIdHeader = "X-Client-Correlation-ID";

    // JWT events
    public const int JwtEvents = 1000;
    public const int JwtAuthenticationFailed = 1001;
    public const int JwtChallengeTriggered = 1002;
    public const int JwtForbidden = 1003;
    public const int JwtMessageReceived = 1004;
    public const int JwtTokenValidated = 1005;

    // MSAL events
    public const int MsalEvents = 2000;
    public const int MsalError = 2001;
    public const int MsalWarning = 2002;
    public const int MsalInfo = 2003;
    public const int MsalDebug = 2004;

    // HTTP pipeline events
    public const int HttpPipelineEvents = 3000;
    public const int RequestReceived = 3001;
    public const int ResponseSent = 3002;

    public static JsonSerializerOptions DataSerializationOptions { get; } = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };
}