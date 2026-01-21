using Entity = WebApi.Service.DataAccess.Entity;
using static WebApi.Common.TypeExtensions;

namespace WebApi.Service.Model;

internal sealed class Game
{
    public string? Id { get; set; }

    public string? Type { get; set; }

    public string? Name { get; set; }

    public string? PlayerName { get; set; }

    public int? PlayerHealth { get; set; }

    public List<Relation>? Relations { get; set; }

    public static Game FromEntity(Entity.Game entity)
    {
        Game model = new()
        {
            Id = entity.Id.ToString("D"),
            Name = entity.Name,
            Type = entity.Type,
            PlayerName = entity.PlayerName,
            PlayerHealth = entity.PlayerHealth,
            Relations = entity.Relations.Select(Relation.FromEntity).ToList()
        };
        return model;
    }

    public Entity.Game ToEntity(Entity.Game? entity = null)
    {
        entity ??= new Entity.Game();
        entity.Id = string.IsNullOrWhiteSpace(Id) ? Guid.Empty : Guid.Parse(Id);
        entity.Name = Name!;
        entity.Type = Type!;
        entity.PlayerName = PlayerName!;
        entity.PlayerHealth = PlayerHealth ?? 0;
        var relations = Relations ?? [];
        var itemsToRemove = entity.Relations
            .Where(x => !relations.Any(y => GuidsEqual(x.Id.ToString(), y.Id)))
            .ToList();
        foreach (var relation in itemsToRemove)
        {
            entity.Relations.Remove(relation);
        }

        foreach (var relation in relations)
        {
            var existingItem = entity.Relations.SingleOrDefault(x => GuidsEqual(x.Id.ToString(), relation.Id));
            if (existingItem is null)
            {
                entity.Relations.Add(relation.ToEntity()!);
            }
            else
            {
                relation.ToEntity(existingItem);
            }
        }

        return entity;
    }
}