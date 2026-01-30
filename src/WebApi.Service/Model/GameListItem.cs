using System.ComponentModel;
using Entity = WebApi.Service.DataAccess.Entity;

namespace WebApi.Service.Model;

internal sealed class GameListItem
{
    [Description("The ID of the game")]
    public Guid Id { get; set; }

    [Description("The type of the game")]
    public string Type { get; set; } = null!;

    [Description("The name of the game")]
    public string Name { get; set; } = null!;

    [Description("The name of the game player")]
    public string PlayerName { get; set; } = null!;

    [Description("The health of the game player")]
    public int PlayerHealth { get; set; }

    public static GameListItem FromEntity(Entity.Game entity)
    {
        GameListItem model = new()
        {
            Id = entity.Id,
            Name = entity.Name,
            Type = entity.Type,
            PlayerName = entity.PlayerName,
            PlayerHealth = entity.PlayerHealth
        };
        return model;
    }
}