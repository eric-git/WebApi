using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using WebApi.Service;
using static WebApi.Common.MiddlewareExtensions;
using static WebApi.Common.SecurityExtensions;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();
var (_, rsaSecurityKey) = await CreateRsaSecurityKeyFromPemFile("./signing/issuer-public.pem");
builder.Services.AddAuthorization()
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts =>
    {
        opts.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["ISSUER_BASE_URL"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["API_ID"],
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = rsaSecurityKey
        };
    });
var app = builder.Build();
app.UseAuthentication()
    .UseAuthorization();

app.ProvideFavIcon()
    .MapStatus(
        Assembly.GetEntryAssembly()!,
        bool.TryParse(app.Configuration["DOTNET_RUNNING_IN_CONTAINER"], out var inContainer) && inContainer)
    .MapGame();
app.Run();