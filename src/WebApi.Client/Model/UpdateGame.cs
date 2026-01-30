using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebApi.Client.Model;

internal sealed class UpdateGame
{
    [Description("The type of the game")]
    [MaxLength(50)]
    public required string Type { get; set; }

    [Description("The name of the game")]
    [MaxLength(500)]
    public required string Name { get; set; }

    [Description("The name of the game player")]
    [MaxLength(255)]
    public required string PlayerName { get; set; }

    [Description("The health of the game player")]
    [Range(0, 100)]
    public int PlayerHealth { get; set; }
}