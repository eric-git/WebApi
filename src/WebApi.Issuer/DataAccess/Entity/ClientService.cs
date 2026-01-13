using System;
using System.Collections.Generic;

namespace WebApi.Issuer.DataAccess.Entity;

public partial class ClientService
{
    public Guid Id { get; set; }

    public Guid ClientId { get; set; }

    public Guid ServiceId { get; set; }

    public Guid KeyId { get; set; }

    public virtual Client Client { get; set; } = null!;

    public virtual ICollection<ClientServiceScope> ClientServiceScopes { get; set; } = new List<ClientServiceScope>();

    public virtual Key Key { get; set; } = null!;

    public virtual Service Service { get; set; } = null!;
}
