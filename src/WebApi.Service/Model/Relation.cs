using System.Text.Json;
using Entity = WebApi.Service.DataAccess.Entity;

namespace WebApi.Service.Model;

internal sealed class Relation
{
    public string? Id { get; set; }

    public string? Type { get; set; }

    public string? Name { get; set; }

    public Dictionary<string, string>? Attributes { get; set; }

    public static Relation FromEntity(Entity.Relation relation)
    {
        Relation model = new()
        {
            Id = relation.Id.ToString("D"),
            Name = relation.Name,
            Type = relation.Type,
            Attributes = JsonSerializer.Deserialize<Dictionary<string, string>>(relation.Attributes)
        };
        return model;
    }

    public Entity.Relation? ToEntity(Entity.Relation? relation = null)
    {
        var entity = relation ?? new Entity.Relation();
        entity.Id = string.IsNullOrWhiteSpace(Id) ? Guid.Empty : Guid.Parse(Id);
        entity.Name = Name!;
        entity.Type = Type!;
        entity.Attributes = JsonSerializer.Serialize(Attributes);
        return entity;
    }
}