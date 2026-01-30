using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Entity = WebApi.Service.DataAccess.Entity;

namespace WebApi.Service.Model;

internal sealed class UpdateRelation
{
    [Description("The type of the relation")]
    [MaxLength(50)]
    [Required]
    public required string Type { get; set; }

    [Description("The name of the relation")]
    [MaxLength(500)]
    [Required]
    public required string Name { get; set; }

    [Description("The attributes of the relation")]
    public Dictionary<string, string>? Attributes { get; set; }

    public void UpdateEntity(Entity.Relation entity)
    {
        entity.Name = Name;
        entity.Type = Type;
        entity.Attributes = JsonSerializer.Serialize(Attributes);
    }
}