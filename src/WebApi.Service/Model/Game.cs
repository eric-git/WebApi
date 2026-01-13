namespace WebApi.Service.Model;

public class Game
{
    public string? Id { get; set; }

    public string? Type { get; set; }

    public string? Name { get; set; }

    public string? PlayerName { get; set; }

    public int? PlayerHealth { get; set; }

    public List<Relation>? Relations { get; set; }
}