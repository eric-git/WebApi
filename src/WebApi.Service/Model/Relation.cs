using System.ComponentModel;
using System.Text.Json;
using Entity = WebApi.Service.DataAccess.Entity;
using static WebApi.Common.Constants;

namespace WebApi.Service.Model;

internal sealed class Relation
{
    [Description("The ID of the relation")]
    public Guid Id { get; set; }

    [Description("The type of the relation")]
    public string? Type { get; set; }

    [Description("The name of the relation")]
    public string? Name { get; set; }

    [Description("The attributes of the relation")]
    public Dictionary<string, string>? Attributes { get; set; }

    public static Relation FromEntity(Entity.Relation relation)
    {
        var attributes = JsonSerializer.Deserialize<Dictionary<string, string>>(relation.Attributes, DataSerializationOptions);
        Relation model = new()
        {
            Id = relation.Id,
            Name = relation.Name,
            Type = relation.Type,
            Attributes = attributes?.Count > 0 ? attributes : null
        };
        return model;
    }

    public Entity.Relation ToEntityForJson(Guid gameId, Entity.Game game)
    {
        var attributes = JsonSerializer.Serialize(Attributes, DataSerializationOptions);
        Entity.Relation entity = new()
        {
            Id = Id,
            Name = Name,
            Type = Type,
            Attributes = attributes,
            Game = game,
            GameId = gameId
        };
        return entity;
    }
}