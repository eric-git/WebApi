using WebApi.Common.Web;
using WebApi.Issuer;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

var app = builder.Build();

var configuration = app.Services.GetRequiredService<IConfiguration>();
var issuerBaseUrl = configuration["ISSUER_BASE_URL"]!;
var tokenLiftTime = configuration["TOKEN_LIFETIME_MINUTES"]!;
app.MapFavIcon();
app.MapStatus(configuration);
app.MapIssuer(issuerBaseUrl, int.Parse(tokenLiftTime));

app.Run();