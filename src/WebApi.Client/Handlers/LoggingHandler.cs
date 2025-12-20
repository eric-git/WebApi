using Microsoft.Extensions.Logging;
using WebApi.Common;

namespace WebApi.Client.Handlers;

public class LoggingHandler(ILogger<LoggingHandler> logger) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage requestMessage, CancellationToken cancellationToken)
    {
        await requestMessage.LogAsync(logger);
        var responseMessage = await base.SendAsync(requestMessage, cancellationToken);
        await responseMessage.LogAsync(logger);
        return responseMessage;
    }
}