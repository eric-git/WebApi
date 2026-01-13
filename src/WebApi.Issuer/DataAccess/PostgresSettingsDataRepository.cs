using Microsoft.EntityFrameworkCore;
using static WebApi.Common.SecurityExtensions;

namespace WebApi.Issuer.DataAccess;

public class PostgresSettingsDataRepository(AppDbContext appDbContext) : ISettingsDataRepository
{
    public async Task<bool> VerifyClientAccessAsync(Guid clientId, Guid serviceId, IList<string> scopes)
    {
        var result = await appDbContext.ClientServices.AnyAsync(x =>
            x.ClientId == clientId &&
            x.ServiceId == serviceId &&
            scopes.All(y => x.ClientServiceScopes.Any(z => z.Scope == y)));
        return result;
    }

    public async Task<string?> GetSigningKeyByClientIdAsync(Guid clientId, Guid serviceId, Guid keyId)
    {
        var signingKey = await appDbContext.ClientServices
            .Where(x => x.ClientId == clientId && x.ServiceId == serviceId && x.KeyId == keyId)
            .Select(x => x.Key.Pem)
            .FirstOrDefaultAsync();
        return WrapPublicKey(signingKey);
    }
}