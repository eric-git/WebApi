using Microsoft.Extensions.Logging;
using WebApi.Common.Logging;

namespace WebApi.Common.Handler;

public sealed class LoggingHandler(IHttpLoggingHandler<ILogger<LoggingHandler>> httpLoggingHandler) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.Content is not null)
        {
            await request.Content.LoadIntoBufferAsync(cancellationToken).ConfigureAwait(false);
        }

        var requestData = request.ToPipelineRequestData();
        await httpLoggingHandler.LogRequestAsync(requestData).ConfigureAwait(false);

        var responseMessage = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

        var original = await responseMessage.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        MemoryStream buffer = new();
        await original.CopyToAsync(buffer, cancellationToken).ConfigureAwait(false);
        var responseData = responseMessage.ToPipelineResponseData(buffer);
        await httpLoggingHandler.LogResponseAsync(responseData).ConfigureAwait(false);
        buffer.Position = 0;
        responseMessage.Content = new StreamContent(buffer);
        foreach (var header in responseMessage.Content.Headers)
        {
            responseMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        await original.DisposeAsync().ConfigureAwait(false);
        return responseMessage;
    }
}