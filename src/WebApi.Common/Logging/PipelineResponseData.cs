namespace WebApi.Common.Logging;

public sealed class PipelineResponseData
{
    public required int StatusCode { get; init; }

    public required IReadOnlyList<KeyValuePair<string, string>> Headers { get; init; }

    public string? ContentType { get; init; }

    public Stream? Body { get; init; }
}