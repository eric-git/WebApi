using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace WebApi.Client.Model;

[SuppressMessage("Performance", "CA1812", Justification = "Instantiated by client")]
internal sealed class GameListItem
{
    [Description("The ID of the game")]
    public Guid Id { get; set; }

    [Description("The type of the game")]
    public string? Type { get; set; }

    [Description("The name of the game")]
    public string? Name { get; set; }

    [Description("The name of the game player")]
    public string? PlayerName { get; set; }

    [Description("The health of the game player")]
    public int PlayerHealth { get; set; }
}