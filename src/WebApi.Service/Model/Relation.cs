namespace WebApi.Service.Model;

public class Relation
{
    public string? Id { get; set; }

    public string? Type { get; set; }

    public string? Name { get; set; }

    public Dictionary<string, string>? Attributes { get; set; }
}