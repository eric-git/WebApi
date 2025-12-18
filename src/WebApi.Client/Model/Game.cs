namespace WebApi.Client.Model;

public class Game
{
    public string? Id { get; set; }

    public string? Type { get; set; }

    public string? Name { get; set; }

    public Attributes? Attributes { get; set; }

    public List<Relation>? Relations { get; set; }
}