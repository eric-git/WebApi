using System.Collections.ObjectModel;

namespace WebApi.Client.Model;

internal sealed class Game
{
    public string? Id { get; set; }

    public string? Type { get; set; }

    public string? Name { get; set; }

    public string? PlayerName { get; set; }

    public int? PlayerHealth { get; set; }

    public Collection<Relation>? Relations { get; set; }
}