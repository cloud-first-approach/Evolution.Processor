using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Processor.Api
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var secret = configuration.GetSection("AuthSettings").GetValue<string>("Secret");
            var issuer = configuration.GetSection("AuthSettings").GetValue<string>("Issuer");
            var audience = configuration.GetSection("AuthSettings").GetValue<string>("Audience");
            var duration = configuration.GetSection("AuthSettings").GetValue<int>("ValidDuration");

            services
                            .AddAuthentication(
                                x =>
                                {
                                    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                                    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                                }
                            )
                            .AddJwtBearer(
                                x =>
                                {
                                    x.RequireHttpsMetadata = false;
                                    x.SaveToken = true;
                                    x.TokenValidationParameters = new TokenValidationParameters
                                    {
                                        ValidateIssuerSigningKey = true,
                                        IssuerSigningKey = new SymmetricSecurityKey(
                                            Encoding.ASCII.GetBytes(secret)
                                        ),
                                        ValidIssuer = issuer,
                                        ValidAudience = audience,
                                        ValidateLifetime = true
                                    };
                                }
                            );

            return services;
        }
    }
}
