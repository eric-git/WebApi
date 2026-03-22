using Microsoft.Extensions.Logging;
using static WebApi.Common.Constants;
using static WebApi.Common.Logging.Constants;

namespace WebApi.Common.Logging;

public static class Helper
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

    public static PipelineRequestData ToPipelineRequestData(this HttpRequestMessage request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return new PipelineRequestData
        {
            Method = request.Method.Method,
            Uri = request.RequestUri!,
            Headers = request.Headers
                             .Select(h => new KeyValuePair<string, string>(h.Key, string.Join(",", h.Value)))
                             .OrderBy(h => h.Key, StringComparer.OrdinalIgnoreCase)
                             .ToList(),
            ContentType = request.Content?.Headers.ContentType?.MediaType,
            Body = request.Content?.ReadAsStream()
        };
    }

    public static PipelineResponseData ToPipelineResponseData(this HttpResponseMessage response, Stream? buffer)
    {
        ArgumentNullException.ThrowIfNull(response);
        return new PipelineResponseData
        {
            StatusCode = (int)response.StatusCode,
            Headers = response.Headers
                              .Select(h => new KeyValuePair<string, string>(h.Key, string.Join(",", h.Value)))
                              .Concat(
                                  response.Content?.Headers.Select(h =>
                                      new KeyValuePair<string, string>(h.Key, string.Join(",", h.Value)))
                                  ?? Enumerable.Empty<KeyValuePair<string, string>>()
                              )
                              .OrderBy(h => h.Key, StringComparer.OrdinalIgnoreCase)
                              .ToList(),
            ContentType = response.Content?.Headers.ContentType?.MediaType,
            Body = buffer
        };
    }
}