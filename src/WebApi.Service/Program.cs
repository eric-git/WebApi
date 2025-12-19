using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using WebApi.Common;
using WebApi.Service;
using static WebApi.Common.SecurityExtensions;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, opts =>
    {
        opts.Authority = builder.Configuration["ISSUER_BASE_URL"];
        opts.Audience = builder.Configuration["API_ID"];
        opts.BackchannelHttpHandler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = CertificateValidationCallback
        };
    });
builder.Services.AddAuthorization();
var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.ProvideFavIcon();
app.MapStatus(
    Assembly.GetEntryAssembly()!,
    bool.TryParse(app.Configuration["DOTNET_RUNNING_IN_CONTAINER"], out var inContainer) && inContainer);
app.MapGame();
app.Run();