using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using static WebApi.Common.Constants;
using static WebApi.Common.Logging.Constants;

namespace WebApi.Common.Logging;

public sealed class HttpPipelineLogger<TLogger>(TLogger logger) : IHttpPipelineLogger<TLogger>
    where TLogger : ILogger
{
    private const string Empty = "<empty>";
    private const string Omitted = "<omitted>";
    private readonly LogCallback _logDelegate = logger.Log;
    private readonly EventId _requestEventId = new(RequestReceived, $"{nameof(HttpPipelineEvents)}.{nameof(RequestReceived)}");
    private readonly EventId _responseEventId = new(ResponseSent, $"{nameof(HttpPipelineEvents)}.{nameof(ResponseSent)}");

    public async Task LogRequestAsync(PipelineRequestData request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(request);
        _logDelegate(LogLevel.Information,
            _requestEventId,
            "=== Request ===\nMethod: {Method}\nUrl: {Url}\nHeaders:\n{Headers}\nBody:\n{Body}",
            request.Method,
            request.Uri,
            FormatHeaders(request.Headers),
            await FormatBodyAsync(request.Body, request.ContentType, cancellationToken).ConfigureAwait(false)
        );
    }

    public async Task LogResponseAsync(PipelineResponseData response, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(response);
        _logDelegate(
            LogLevel.Information,
            _responseEventId,
            "=== Response ===\nStatus: {Status}\nHeaders:\n{Headers}\nBody:\n{Body}",
            response.StatusCode,
            FormatHeaders(response.Headers),
            await FormatBodyAsync(response.Body, response.ContentType, cancellationToken).ConfigureAwait(false)
        );
    }

    private static async Task<string> FormatBodyAsync(Stream? body, string? contentType, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (body is null || body.Length is 0)
        {
            return Empty;
        }

        if (!MediaTypeHeaderValue.TryParse(contentType, out var mediaType))
        {
            return Omitted;
        }

        var isFormData = mediaType.SubType.Equals("x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase);
        var isJsonData = mediaType.SubType.EndsWith("json", StringComparison.OrdinalIgnoreCase);
        if (!isFormData &&
            !isJsonData &&
            !mediaType.SubType.EndsWith("xml", StringComparison.OrdinalIgnoreCase) &&
            !mediaType.SubType.Equals("javascript", StringComparison.OrdinalIgnoreCase) &&
            !mediaType.Type.Equals("text", StringComparison.OrdinalIgnoreCase))
        {
            return Omitted;
        }

        using StreamReader reader = new(body, leaveOpen: true);
        body.Position = 0;
        var content = await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(content))
        {
            return Empty;
        }

        if (isFormData && TryFormatFormBody(content, out var formattedForm))
        {
            return formattedForm!;
        }

        if (isJsonData && TryFormatJsonBody(content, out var formattedJson))
        {
            return formattedJson!;
        }

        return content;
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