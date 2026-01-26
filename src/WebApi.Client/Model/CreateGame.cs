using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebApi.Client.Model;

internal sealed class CreateGame
{
    [Description("The type of the game")]
    public required string Type { get; set; }

    [Description("The name of the game")]
    public required string Name { get; set; }

    [Description("The name of the game player")]
    public required string PlayerName { get; set; }

    [Description("The health of the game player")]
    [Range(0, 100)]
    public int PlayerHealth { get; set; }
}