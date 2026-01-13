using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;

namespace WebApi.Common.Web;

public class JwtBearerOptionsConfigure(ClientCertificateHandler clientCertificateHandler) : IConfigureNamedOptions<JwtBearerOptions>
{
    public void Configure(JwtBearerOptions options)
    {
        options.BackchannelHttpHandler = clientCertificateHandler;
    }

    public void Configure(string? name, JwtBearerOptions options)
    {
        options.BackchannelHttpHandler = clientCertificateHandler;
    }
}