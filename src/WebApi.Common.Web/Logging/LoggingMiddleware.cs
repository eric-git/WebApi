using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using WebApi.Common.Logging;

namespace WebApi.Common.Web.Logging;

public sealed class LoggingMiddleware(RequestDelegate next, IHttpLoggingHandler<ILogger<LoggingMiddleware>> logger)
{
    public async Task Invoke(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var cancellationToken = context.RequestAborted;
        cancellationToken.ThrowIfCancellationRequested();
        context.Request.EnableBuffering();
        var requestData = context.Request.ToPipelineRequestData();
        await logger.LogRequestAsync(requestData, cancellationToken).ConfigureAwait(false);
        context.Request.Body.Position = 0;

        var original = context.Response.Body;
        using MemoryStream buffer = new();
        context.Response.Body = buffer;

        await next(context).ConfigureAwait(false);

        var responseData = context.Response.ToPipelineResponseData();
        await logger.LogResponseAsync(responseData, cancellationToken).ConfigureAwait(false);
        buffer.Position = 0;
        await buffer.CopyToAsync(original, cancellationToken).ConfigureAwait(false);
    }
}