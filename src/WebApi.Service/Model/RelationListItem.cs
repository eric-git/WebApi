using System.ComponentModel;
using System.Text.Json;
using static WebApi.Common.Constants;
using Entity = WebApi.Service.DataAccess.Entity;

namespace WebApi.Service.Model;

internal sealed class RelationListItem
{
    [Description("The ID of the relation")]
    public Guid Id { get; set; }

    [Description("The type of the relation")]
    public string Type { get; set; } = null!;

    [Description("The name of the relation")]
    public string Name { get; set; } = null!;

    public static RelationListItem FromEntity(Entity.Relation relation)
    {
        var attributes = JsonSerializer.Deserialize<Dictionary<string, string>>(relation.Attributes, DataSerializationOptions);
        RelationListItem model = new()
        {
            Id = relation.Id,
            Name = relation.Name,
            Type = relation.Type
        };
        return model;
    }
}