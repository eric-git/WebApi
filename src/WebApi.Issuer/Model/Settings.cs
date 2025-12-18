namespace WebApi.Issuer.Model;

public record Settings(
    Service[] Services,
    Client[] Clients);