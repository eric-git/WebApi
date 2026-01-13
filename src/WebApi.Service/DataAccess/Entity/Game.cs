using System;
using System.Collections.Generic;

namespace WebApi.Service.DataAccess.Entity;

public partial class Game
{
    public Guid Id { get; set; }

    public string Type { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string PlayerName { get; set; } = null!;

    public int PlayerHealth { get; set; }

    public virtual ICollection<Relation> Relations { get; set; } = new List<Relation>();
}
