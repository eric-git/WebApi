using System.ComponentModel;

namespace WebApi.Client.Model;

internal sealed class CreateRelation
{
    [Description("The type of the relation")]
    public required string Type { get; set; }

    [Description("The name of the relation")]
    public required string Name { get; set; }

    [Description("The attributes of the relation")]
    public Dictionary<string, string>? Attributes { get; set; }
}