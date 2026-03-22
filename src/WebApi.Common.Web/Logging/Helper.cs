using Microsoft.AspNetCore.Http;
using WebApi.Common.Logging;

namespace WebApi.Common.Web.Logging;

public static class Helper
{
    public static PipelineRequestData ToPipelineRequestData(this HttpRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return new PipelineRequestData
        {
            Method = request.Method,
            Uri = new Uri($"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}"),
            Headers = request.Headers
                             .Select(h => new KeyValuePair<string, string>(h.Key, h.Value.ToString()))
                             .OrderBy(h => h.Key, StringComparer.OrdinalIgnoreCase)
                             .ToList(),
            ContentType = request.ContentType,
            Body = request.Body
        };
    }

    public static PipelineResponseData ToPipelineResponseData(this HttpResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);
        return new PipelineResponseData
        {
            StatusCode = response.StatusCode,
            Headers = response.Headers
                              .Select(h => new KeyValuePair<string, string>(h.Key, h.Value.ToString()))
                              .OrderBy(h => h.Key, StringComparer.OrdinalIgnoreCase)
                              .ToList(),
            ContentType = response.ContentType,
            Body = response.Body
        };
    }
}