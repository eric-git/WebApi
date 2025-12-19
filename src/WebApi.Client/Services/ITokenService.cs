namespace WebApi.Client.Services;

public interface ITokenService
{
    Task<string> GetTokenAsync();
}