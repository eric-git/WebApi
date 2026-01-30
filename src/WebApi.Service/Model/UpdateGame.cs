using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Entity = WebApi.Service.DataAccess.Entity;

namespace WebApi.Service.Model;

internal sealed class UpdateGame
{
    [Description("The type of the game")]
    [MaxLength(50)]
    [Required]
    public required string Type { get; set; }

    [Description("The name of the game")]
    [MaxLength(500)]
    [Required]
    public required string Name { get; set; }

    [Description("The name of the game player")]
    [MaxLength(255)]
    [Required]
    public required string PlayerName { get; set; }

    [Description("The health of the game player")]
    [Range(0, 100)]
    public int PlayerHealth { get; set; }

    public void UpdateEntity(Entity.Game entity)
    {
        entity.Name = Name;
        entity.Type = Type;
        entity.PlayerName = PlayerName;
        entity.PlayerHealth = PlayerHealth;
    }
}