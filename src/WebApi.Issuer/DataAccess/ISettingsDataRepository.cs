using WebApi.Issuer.DataAccess.Entity;

namespace WebApi.Issuer.DataAccess;

internal interface ISettingsDataRepository
{
    Task<bool> VerifyClientAccessAsync(Guid clientId, Guid serviceId, IList<string> scopes);

    Task<string?> GetSigningKeyByClientIdAsync(Guid clientId, Guid serviceId, Guid keyId);

    Task<Client?> GetClientDetailsById(Guid clientId);
}