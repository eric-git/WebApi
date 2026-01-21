using Microsoft.Extensions.Logging;
using static WebApi.Common.Constants;

namespace WebApi.Common.Handler;

public sealed class CorrelationHandler(ILogger<CorrelationHandler> logger) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var correlationId = request.Headers.TryGetValues(ClientCorrelationIdHeader, out var existing)
            ? existing.First()
            : Guid.NewGuid().ToString("D");
        request.Headers.Remove(ClientCorrelationIdHeader);
        request.Headers.Add(ClientCorrelationIdHeader, correlationId);
        using (logger.BeginScope(new Dictionary<string, object>
               {
                   ["ClientCorrelationId"] = correlationId
               }))
        {
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }
}