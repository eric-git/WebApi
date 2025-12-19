namespace WebApi.Issuer.Model;

public class ServiceAccess
{
    public string? Id { get; set; }

    public List<string>? Scopes { get; set; }
}