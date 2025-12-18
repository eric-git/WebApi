using Microsoft.Extensions.Logging;

namespace WebApi.Common;

public class LoggingHandler(ILogger<LoggingHandler> logger) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage requestMessage, CancellationToken cancellationToken)
    {
        await requestMessage.Log(logger);
        var responseMessage = await base.SendAsync(requestMessage, cancellationToken);
        await responseMessage.Log(logger);
        return responseMessage;
    }
}