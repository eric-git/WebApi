using System.ComponentModel;
using Entity = WebApi.Service.DataAccess.Entity;

namespace WebApi.Service.Model;

internal sealed class Game
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

    [Description("The relations of the game")]
    public List<Relation>? Relations { get; set; }

    public static Game FromEntity(Entity.Game entity)
    {
        List<Relation> relations = [.. (entity.Relations ?? []).Select(Relation.FromEntity)];
        Game model = new()
        {
            Id = entity.Id,
            Name = entity.Name,
            Type = entity.Type,
            PlayerName = entity.PlayerName,
            PlayerHealth = entity.PlayerHealth,
            Relations = relations.Count > 0 ? relations : null
        };
        return model;
    }

    public Entity.Game ToEntityForJson()
    {
        Entity.Game entity = new()
        {
            Id = Id,
            Name = Name,
            Type = Type,
            PlayerName = PlayerName,
            PlayerHealth = PlayerHealth
        };
        var relations = (Relations ?? []).Select(x => x.ToEntityForJson(Id, entity)).ToList();
        entity.Relations = relations;
        return entity;
    }
}