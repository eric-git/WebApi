namespace WebApi.Common.Logging;

public interface IHttpPipelineLogger<TLogger>
{
    Task LogRequestAsync(PipelineRequestData request, CancellationToken cancellationToken);

    Task LogResponseAsync(PipelineResponseData response, CancellationToken cancellationToken);
}