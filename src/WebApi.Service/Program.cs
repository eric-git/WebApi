using Microsoft.AspNetCore.Authentication.JwtBearer;
using WebApi.Common;
using WebApi.Common.Web;
using WebApi.Service;
using static WebApi.Common.SecurityExtensions;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();
if (builder.Environment.IsDevelopment())
{
    CertificateStore.Load();
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(jwtBearerOptions =>
    {
        var configurationManager = builder.Configuration;
        var webHostEnvironment = builder.Environment;
        jwtBearerOptions.Authority = configurationManager["ISSUER_BASE_URL"];
        jwtBearerOptions.Audience = configurationManager["API_ID"];
        if (webHostEnvironment.IsDevelopment())
        {
            jwtBearerOptions.BackchannelHttpHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = CertificateValidationCallback
            };
        }
    });

builder.Services.AddAuthorization();

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();

var configuration = app.Services.GetRequiredService<IConfiguration>();
app.MapFavIcon();
app.MapStatus(configuration);
app.MapGame();

app.Run();