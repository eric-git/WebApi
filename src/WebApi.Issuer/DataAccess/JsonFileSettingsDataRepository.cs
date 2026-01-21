using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using WebApi.Issuer.DataAccess.Entity;
using static WebApi.Common.SecurityExtensions;

namespace WebApi.Issuer.DataAccess;

[SuppressMessage("Performance", "CA1812", Justification = "Instantiated by DI container")]
internal sealed class JsonFileSettingsDataRepository : ISettingsDataRepository
{
    private readonly string _dataFilePath;

    public JsonFileSettingsDataRepository(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        var dataFilePath = Path.Combine(configuration["DATA_PATH"]!, "db.data");
        if (!Path.IsPathRooted(dataFilePath))
        {
            dataFilePath = Path.Combine(AppContext.BaseDirectory, dataFilePath);
        }

        _dataFilePath = dataFilePath;
    }

    public Task<bool> VerifyClientAccessAsync(Guid clientId, Guid serviceId, IList<string> scopes)
    {
        var rootElement = GetJsonRootElement();
        var client = rootElement.GetProperty("Clients")
            .EnumerateArray()
            .FirstOrDefault(c => c.GetProperty("Id").GetGuid() == clientId);
        if (client.ValueKind is JsonValueKind.Undefined)
        {
            return Task.FromResult(false);
        }

        var service = client.GetProperty("Services")
            .EnumerateArray()
            .FirstOrDefault(s => s.GetProperty("Id").GetGuid() == serviceId);
        if (service.ValueKind is JsonValueKind.Undefined)
        {
            return Task.FromResult(false);
        }

        var scopesInJson = service.GetProperty("Scopes")
            .EnumerateArray()
            .Select(s => s.GetString())
            .ToList();
        var result = scopes.All(scopesInJson.Contains);
        return Task.FromResult(result);
    }

    public Task<string?> GetSigningKeyByClientIdAsync(Guid clientId, Guid serviceId, Guid keyId)
    {
        var root = GetJsonRootElement();
        var client = root.GetProperty("Clients")
            .EnumerateArray()
            .FirstOrDefault(c =>
                c.TryGetProperty("Id", out var id) &&
                id.GetGuid() == clientId);
        if (client.ValueKind is JsonValueKind.Undefined)
        {
            return Task.FromResult<string?>(null);
        }

        var serviceEntry = client.GetProperty("Services")
            .EnumerateArray()
            .FirstOrDefault(s =>
                s.TryGetProperty("Id", out var id) &&
                id.GetGuid() == serviceId);
        if (serviceEntry.ValueKind is JsonValueKind.Undefined ||
            !serviceEntry.TryGetProperty("KeyId", out var keyIdProp) ||
            keyIdProp.GetGuid() != keyId)
        {
            return Task.FromResult<string?>(null);
        }

        var key = root.GetProperty("Keys")
            .EnumerateArray()
            .FirstOrDefault(k =>
                k.TryGetProperty("Id", out var id) &&
                id.GetGuid() == keyId);
        if (key.ValueKind is JsonValueKind.Undefined)
        {
            return Task.FromResult<string?>(null);
        }

        var pem = key.TryGetProperty("Pem", out var pemProp)
            ? pemProp.GetString()
            : null;
        return Task.FromResult(WrapPublicKey(pem));
    }

    public Task<Client?> GetClientDetailsById(Guid clientId)
    {
        var root = GetJsonRootElement();
        var client = root.GetProperty("Clients")
            .EnumerateArray()
            .FirstOrDefault(c =>
                c.TryGetProperty("Id", out var id) &&
                id.GetGuid() == clientId);
        if (client.ValueKind is JsonValueKind.Undefined)
        {
            return Task.FromResult<Client?>(null);
        }

        Client clientObj = new()
        {
            Id = clientId,
            Name = client.GetProperty(nameof(Client.Name)).GetString()!,
            Email = client.GetProperty(nameof(Client.Email)).GetString()!
        };
        return Task.FromResult<Client?>(clientObj);
    }

    private JsonElement GetJsonRootElement()
    {
        var content = File.ReadAllText(_dataFilePath);
        var jsonDocument = JsonDocument.Parse(content);
        return jsonDocument.RootElement;
    }
}