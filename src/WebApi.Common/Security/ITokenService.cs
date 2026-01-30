namespace WebApi.Common.Security;

public interface ITokenService
{
    Task<string> GetTokenAsync(CancellationToken cancellationToken);
}