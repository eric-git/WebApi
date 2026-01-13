using System;
using System.Collections.Generic;

namespace WebApi.Service.DataAccess.Entity;

public partial class Relation
{
    public Guid Id { get; set; }

    public Guid GameId { get; set; }

    public string Type { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Attributes { get; set; } = null!;

    public virtual Game Game { get; set; } = null!;
}
