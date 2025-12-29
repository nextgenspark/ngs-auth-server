using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace NextGenSpark.AuthServer.OpenIddict
{
    public static class OpenIddictExtensions
    {
        public static IServiceCollection AddAuthServerOpenIddict(
    this IServiceCollection services,
    IConfiguration configuration,
    IWebHostEnvironment env)
        {
            services.AddOpenIddict()
                .AddServer(options =>
                {
                    options.AllowAuthorizationCodeFlow()
                           .RequireProofKeyForCodeExchange();

                    options.AllowClientCredentialsFlow();
                    options.AllowRefreshTokenFlow();

                    options.SetAuthorizationEndpointUris("/connect/authorize");
                    options.SetTokenEndpointUris("/connect/token");

                    options.SetAccessTokenLifetime(TimeSpan.FromMinutes(10));
                    options.SetRefreshTokenLifetime(TimeSpan.FromDays(7));

                    options.DisableAccessTokenEncryption();

                    options.UseAspNetCore()
                        .EnableAuthorizationEndpointPassthrough()
                        .EnableTokenEndpointPassthrough();

                    if (env.IsDevelopment())
                    {
                        options.AddDevelopmentSigningCertificate()
                               .AddDevelopmentEncryptionCertificate();
                    }
                    else
                    {
                        var signingCert = new X509Certificate2(
                            Convert.FromBase64String(configuration["Auth:SigningCert"]!),
                            configuration["Auth:SigningCertPassword"]);

                        var encryptionCert = new X509Certificate2(
                            Convert.FromBase64String(configuration["Auth:EncryptionCert"]!),
                            configuration["Auth:EncryptionCertPassword"]);

                        options.AddSigningCertificate(signingCert);
                        options.AddEncryptionCertificate(encryptionCert);
                    }
                });

            return services;
        }
    }

}
