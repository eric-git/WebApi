namespace WebApi.Issuer.DataAccess;

public interface ISettingsDataRepository
{
    Task<bool> VerifyClientAccessAsync(Guid clientId, Guid serviceId, IList<string> scopes);

    Task<string?> GetSigningKeyByClientIdAsync(Guid clientId, Guid serviceId, Guid keyId);
}