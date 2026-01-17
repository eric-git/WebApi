using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using WebApi.Common.Handler;

namespace WebApi.Common.Web;

public sealed class JwtBearerOptionsConfigure(CertificateHandler clientCertificateHandler) : IConfigureNamedOptions<JwtBearerOptions>
{
    public void Configure(JwtBearerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.BackchannelHttpHandler = clientCertificateHandler;
    }

    public void Configure(string? name, JwtBearerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.BackchannelHttpHandler = clientCertificateHandler;
    }
}