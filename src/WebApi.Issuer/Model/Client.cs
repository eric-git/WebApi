namespace WebApi.Issuer.Model;

public class Client
{
    public string? Id { get; set; }

    public List<ServiceAccess>? Services { get; set; }
}