using System.Text.Json;
using Microsoft.Extensions.Logging;
using static WebApi.Common.Constants;

namespace WebApi.Common.Logging;

public sealed class HttpLoggingHandler<TLogger>(TLogger logger) : IHttpLoggingHandler<TLogger>
    where TLogger : ILogger
{
    private const string Empty = "<empty>";
    private readonly LogCallback _logDelegate = logger.Log;
    private readonly EventId _requestEventId = new(RequestReceived, $"{nameof(HttpPipelineEvents)}.{nameof(RequestReceived)}");
    private readonly EventId _responseEventId = new(ResponseSent, $"{nameof(HttpPipelineEvents)}.{nameof(ResponseSent)}");

    public async Task LogRequestAsync(PipelineRequestData request)
    {
        ArgumentNullException.ThrowIfNull(request);
        _logDelegate(LogLevel.Information,
            _requestEventId,
            "=== Request ===\nMethod: {Method}\nUrl: {Url}\nHeaders:\n{Headers}\nBody:\n{Body}",
            request.Method,
            request.Uri,
            FormatHeaders(request.Headers),
            await FormatBodyAsync(request.Body, request.ContentType).ConfigureAwait(false)
        );
    }

    public async Task LogResponseAsync(PipelineResponseData response)
    {
        ArgumentNullException.ThrowIfNull(response);
        _logDelegate(
            LogLevel.Information,
            _responseEventId,
            "=== Response ===\nStatus: {Status}\nHeaders:\n{Headers}\nBody:\n{Body}",
            response.StatusCode,
            FormatHeaders(response.Headers),
            await FormatBodyAsync(response.Body, response.ContentType).ConfigureAwait(false)
        );
    }

    private static async Task<string> FormatBodyAsync(Stream? body, string? contentType)
    {
        if (body is null)
        {
            return Empty;
        }

        using StreamReader reader = new(body, leaveOpen: true);
        body.Position = 0;
        var content = await reader.ReadToEndAsync().ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(content))
        {
            return Empty;
        }

        return contentType switch
        {
            "application/json" when TryFormatJsonBody(content, out var formattedJson) => formattedJson!,
            "application/x-www-form-urlencoded" when TryFormatFormBody(content, out var formattedForm) => formattedForm!,
            _ => content
        };
    }

    private static string FormatHeaders(IReadOnlyList<KeyValuePair<string, string>> httpHeaders)
    {
        var headerList = httpHeaders
            .Select(header => $"[{header.Key}, {string.Join("; ", header.Value)}]");
        return string.Join(", ", headerList);
    }

    private static bool TryFormatFormBody(string content, out string? formatted)
    {
        if (string.IsNullOrEmpty(content))
        {
            formatted = null;
            return false;
        }

        var pairs = content.Split('&', StringSplitOptions.RemoveEmptyEntries);
        formatted = string.Join($"&{Environment.NewLine}", pairs);
        return true;
    }

    private static bool TryFormatJsonBody(string content, out string? formatted)
    {
        formatted = null;
        if (string.IsNullOrWhiteSpace(content))
        {
            return false;
        }

        try
        {
            using var jsonDocument = JsonDocument.Parse(content);
            formatted = JsonSerializer.Serialize(jsonDocument.RootElement, DataSerializationOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}