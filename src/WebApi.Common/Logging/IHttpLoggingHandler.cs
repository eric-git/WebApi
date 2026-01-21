namespace WebApi.Common.Logging;

public interface IHttpLoggingHandler<TLogger>
{
    Task LogRequestAsync(PipelineRequestData request);

    Task LogResponseAsync(PipelineResponseData response);
}