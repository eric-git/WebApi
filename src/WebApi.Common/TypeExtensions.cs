using WebApi.Common.Logging;

namespace WebApi.Common;

public static class TypeExtensions
{
    public static bool GuidsEqual(string? a, string? b)
    {
        return Guid.TryParse(a, out var g1) &&
               Guid.TryParse(b, out var g2) &&
               g1 == g2;
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