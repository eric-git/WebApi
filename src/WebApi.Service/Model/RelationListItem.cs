using System.ComponentModel;
using Entity = WebApi.Service.DataAccess.Entity;

namespace WebApi.Service.Model;

internal sealed class RelationListItem
{
    [Description("The ID of the relation")]
    public Guid Id { get; set; }

    [Description("The type of the relation")]
    public string? Type { get; set; }

    [Description("The name of the relation")]
    public string? Name { get; set; }

    public static RelationListItem FromEntity(Entity.Relation relation)
    {
        RelationListItem model = new()
        {
            Id = relation.Id,
            Name = relation.Name,
            Type = relation.Type
        };
        return model;
    }
}