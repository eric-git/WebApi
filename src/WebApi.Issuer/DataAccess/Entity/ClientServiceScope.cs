using System;
using System.Collections.Generic;

namespace WebApi.Issuer.DataAccess.Entity;

public partial class ClientServiceScope
{
    public Guid Id { get; set; }

    public Guid ClientServiceId { get; set; }

    public string Scope { get; set; } = null!;

    public virtual ClientService ClientService { get; set; } = null!;
}
