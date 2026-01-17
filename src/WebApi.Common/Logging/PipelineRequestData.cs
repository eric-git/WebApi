namespace WebApi.Common.Logging;

public sealed class PipelineRequestData
{
    public required string Method { get; init; }

    public required Uri Uri { get; init; }

    public required IReadOnlyList<KeyValuePair<string, string>> Headers { get; init; }

    public string? ContentType { get; init; }

    public Stream? Body { get; init; }
}