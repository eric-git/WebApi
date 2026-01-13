using System;
using System.Collections.Generic;

namespace WebApi.Issuer.DataAccess.Entity;

public partial class Client
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<ClientService> ClientServices { get; set; } = new List<ClientService>();
}
