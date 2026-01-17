namespace WebApi.Common.Handler;

public interface ITokenService
{
    Task<string> GetTokenAsync();
}