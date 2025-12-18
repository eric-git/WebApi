namespace WebApi.Issuer.Model;

public record ServiceAccess(
    string Id,
    string[] Scopes);