using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebApi.Client.Model;

internal sealed class CreateRelation
{
    [Description("The type of the relation")]
    [MaxLength(50)]
    public required string Type { get; set; }

    [Description("The name of the relation")]
    [MaxLength(500)]
    public required string Name { get; set; }

    [Description("The attributes of the relation")]
    public Dictionary<string, string>? Attributes { get; set; }
}