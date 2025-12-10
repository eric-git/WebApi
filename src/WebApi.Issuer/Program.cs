using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using OpenIddict.Abstractions;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenIddict()
    .AddCore(options => options.UseInMemoryStores())
    .AddServer(options =>
    {
        options.SetIssuer(new Uri("https://localhost:5001/"));
        options.SetTokenEndpointUris("/connect/token");
        options.AllowClientCredentials();
        options.AcceptClientAssertions();
        options.AddDevelopmentEncryptionCertificate()
               .AddDevelopmentSigningCertificate();
        options.UseAspNetCore().EnableTokenEndpointPassthrough();
        options.RegisterScopes(OpenIddictConstants.Scopes.OpenId, "api.read", "api.write");
    })
    .AddValidation(options =>
    {
        options.UseLocalServer();
        options.UseAspNetCore();
    });

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
builder.Services.AddAuthorization();
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Seed clients from Clients.json
using (var scope = app.Services.CreateScope())
{
    var manager = scope.ServiceProvider.GetRequiredService<OpenIddictApplicationManager>();
    var json = File.ReadAllText(Path.Combine(app.Environment.ContentRootPath, "Clients.json"));
    var clients = JsonSerializer.Deserialize<List<ClientDescriptor>>(json) ?? new();
    foreach (var c in clients)
    {
        var existing = await manager.FindByClientIdAsync(c.ClientId);
        if (existing is null)
        {
            var desc = new OpenIddictApplicationDescriptor
            {
                ClientId = c.ClientId,
                DisplayName = c.DisplayName,
                Permissions = c.Permissions?.ToHashSet() ?? new HashSet<string>()
            };
            if (c.Jwks is not null)
            {
                desc.JsonWebKeySet = new OpenIddictJsonWebKeySet
                {
                    Keys = c.Jwks.Keys.Select(k => new OpenIddictJsonWebKey
                    {
                        KeyId = k.Kid,
                        Type = k.Kty,
                        Use = k.Use,
                        Parameters = new Dictionary<string, string>
                        {
                            ["n"] = k.N,
                            ["e"] = k.E
                        }
                    }).ToList()
                };
            }
            await manager.CreateAsync(desc);
        }
    }
}

app.MapControllers();
app.Run();

// DTOs
public class ClientDescriptor
{
    public string ClientId { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public List<string>? Permissions { get; set; }
    public Jwks? Jwks { get; set; }
}
public class Jwks { public List<Jwk> Keys { get; set; } = new(); }
public class Jwk {
    public string Kid { get; set; } = "";
    public string Kty { get; set; } = "";
    public string Use { get; set; } = "sig";
    public string N { get; set; } = "";
    public string E { get; set; } = "AQAB";
     }

public class OpenIddictJsonWebKeySet {
    public List<OpenIddictJsonWebKey> Keys { get; set; } = new();
      }
public class OpenIddictJsonWebKey
{ public string? KeyId { get; set; } public string? Type { get; set; } public string? Use { get; set; } public Dictionary<string, string> Parameters { get; set; } = new();
 }