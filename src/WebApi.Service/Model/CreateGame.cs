using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Entity = WebApi.Service.DataAccess.Entity;

namespace WebApi.Service.Model;

[SuppressMessage("Performance", "CA1812", Justification = "Instantiated by service")]
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

    public Entity.Game ToEntity()
    {
        Entity.Game entity = new()
        {
            Id = Guid.NewGuid(),
            Name = Name,
            Type = Type,
            PlayerName = PlayerName,
            PlayerHealth = PlayerHealth
        };
        return entity;
    }
}