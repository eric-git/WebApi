using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace WebApi.Common.Web.Logging;

public sealed class CorrelationMiddleware(RequestDelegate next, ILogger<CorrelationMiddleware> logger)
{
    private const string TraceParentHeader = "traceparent";
    private const string ClientRequestIdHeader = "client-request-id";
    private const string ReturnClientRequestIdHeader = "return-client-request-id";

    public async Task Invoke(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var cancellationToken = context.RequestAborted;
        cancellationToken.ThrowIfCancellationRequested();
        Dictionary<string, object> state = new(StringComparer.OrdinalIgnoreCase);
        if (context.Request.Headers.TryGetValue(Constants.ClientCorrelationIdHeader, out var clientCorrelationId))
        {
            context.Response.Headers[Constants.ClientCorrelationIdHeader] = clientCorrelationId;
            state[Constants.ClientCorrelationIdHeader] = clientCorrelationId.ToString();
        }

        if (context.Request.Headers.TryGetValue(TraceParentHeader, out var traceParent))
        {
            context.Response.Headers[TraceParentHeader] = traceParent;
            state[TraceParentHeader] = traceParent.ToString();
        }

        if (context.Request.Headers.TryGetValue(ClientRequestIdHeader, out var clientRequestId) &&
            context.Request.Headers.TryGetValue(ReturnClientRequestIdHeader, out var returnClientRequestId) &&
            bool.TryParse(returnClientRequestId, out var shouldReturn) &&
            shouldReturn)
        {
            context.Response.Headers[ClientRequestIdHeader] = clientRequestId;
            state[ClientRequestIdHeader] = clientRequestId.ToString();
        }

        if (state.Count is 0)
        {
            await next(context).ConfigureAwait(false);
            return;
        }

        using (logger.BeginScope(state))
        {
            await next(context).ConfigureAwait(false);
        }
    }
}