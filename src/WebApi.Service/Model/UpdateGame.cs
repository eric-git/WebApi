using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Entity = WebApi.Service.DataAccess.Entity;

namespace WebApi.Service.Model;

[SuppressMessage("Performance", "CA1812", Justification = "Instantiated by service")]
internal sealed class UpdateGame
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

    public void UpdateEntity(Entity.Game entity)
    {
        entity.Name = Name;
        entity.Type = Type;
        entity.PlayerName = PlayerName;
        entity.PlayerHealth = PlayerHealth;
    }
}