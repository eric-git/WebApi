namespace WebApi.Issuer.Model;

public record Client(
    string Id,
    ServiceAccess[] Services);