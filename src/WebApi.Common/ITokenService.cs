namespace WebApi.Common;

public interface ITokenService
{
    Task<string> GetTokenAsync();
}