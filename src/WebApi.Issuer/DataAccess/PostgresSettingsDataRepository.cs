using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using WebApi.Issuer.DataAccess.Entity;
using static WebApi.Common.Security.Helper;

namespace WebApi.Issuer.DataAccess;

[SuppressMessage("Performance", "CA1812", Justification = "Instantiated by DI container")]
internal sealed class PostgresSettingsDataRepository(AppDbContext appDbContext) : ISettingsDataRepository
{
    public async Task<bool> VerifyClientAccessAsync(Guid clientId, Guid serviceId, IList<string> scopes, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var result = await appDbContext.ClientServices
            .AnyAsync(x =>
                x.ClientId == clientId &&
                x.ServiceId == serviceId &&
                scopes.All(y => x.ClientServiceScopes.Any(z => z.Scope == y)), cancellationToken)
            .ConfigureAwait(false);
        return result;
    }

    public async Task<string?> GetSigningKeyByClientIdAsync(Guid clientId, Guid serviceId, Guid keyId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var signingKey = await appDbContext.ClientServices
            .Where(x => x.ClientId == clientId && x.ServiceId == serviceId && x.KeyId == keyId)
            .Select(x => x.Key.Pem)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
        return WrapPublicKey(signingKey);
    }

    public async Task<Client?> GetClientDetailsById(Guid clientId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var client = await appDbContext.Clients.FindAsync([clientId], cancellationToken).ConfigureAwait(false);
        return client;
    }
}