using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Entity = WebApi.Service.DataAccess.Entity;

namespace WebApi.Service.Model;

[SuppressMessage("Performance", "CA1812", Justification = "Instantiated by service")]
internal sealed class CreateRelation
{
    [Description("The type of the relation")]
    public required string Type { get; set; }

    [Description("The name of the relation")]
    public required string Name { get; set; }

    [Description("The attributes of the relation")]
    public Dictionary<string, string>? Attributes { get; set; }

    public Entity.Relation ToEntity(Guid gameId)
    {
        Entity.Relation entity = new()
        {
            Id = Guid.NewGuid(),
            GameId = gameId,
            Name = Name,
            Type = Type,
            Attributes = JsonSerializer.Serialize(Attributes)
        };
        return entity;
    }
}