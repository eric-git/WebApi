namespace WebApi.Client.Model;

internal sealed class Relation
{
    public string? Id { get; set; }

    public string? Type { get; set; }

    public string? Name { get; set; }

    public Dictionary<string, string>? Attributes { get; set; }
}