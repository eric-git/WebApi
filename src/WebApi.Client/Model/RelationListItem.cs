using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace WebApi.Client.Model;

[SuppressMessage("Performance", "CA1812", Justification = "Instantiated by client")]
internal sealed class RelationListItem
{
    [Description("The ID of the relation")]
    public Guid Id { get; set; }

    [Description("The type of the relation")]
    public string? Type { get; set; }

    [Description("The name of the relation")]
    public string? Name { get; set; }
}