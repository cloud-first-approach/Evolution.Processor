using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Processor.Api.AppSettings;

namespace Processor.Api.Middlewares
{
    public class JwtClaimsParserMiddleware
    {
        private readonly AuthSettings _authSettings;
        private readonly RequestDelegate next;

        public JwtClaimsParserMiddleware(RequestDelegate next, IOptions<AuthSettings> options)
        {
            this._authSettings = options.Value;
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            var bearerToken = httpContext.Request.Headers["Authorization"].FirstOrDefault();
            if (bearerToken != null)
            {
                var token = bearerToken.Split(" ")[1];

                JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
                TokenValidationParameters validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                                            Encoding.ASCII.GetBytes(_authSettings.Secret)
                                        ),
                    ValidIssuer = _authSettings.Issuer,
                    ValidAudience = _authSettings.Audience,
                    ValidateLifetime = true
                };
                handler.ValidateToken(token, validationParameters, out SecurityToken validToken);

                if (validToken != null)
                {
                    var jwtToken = (JwtSecurityToken)validToken;
                    var principal = new BasePrincipal()
                    {
                        Subject = jwtToken.Claims.FirstOrDefault(item=> item.Type.Equals("username"))?.Value
                    };

                    AttachClaimsToContext(httpContext, principal);
                }
            }
            await next(httpContext);
        }

        public void AttachClaimsToContext(HttpContext httpContext, BasePrincipal principal)
        {
            httpContext.Items["Principal"] = principal;
        }
    }

    public class BasePrincipal
    {
        public string Subject { get; set; }
    }

    public static class JwtClaimsParserMiddlewareExtensions {

        public static IApplicationBuilder UseJwtParser(this IApplicationBuilder builder) { 
            return builder.UseMiddleware<JwtClaimsParserMiddleware>(); 
        }
    }
}
