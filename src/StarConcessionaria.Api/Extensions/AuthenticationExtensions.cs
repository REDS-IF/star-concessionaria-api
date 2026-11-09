using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using StarConcessionaria.Infrastructure.Auth;

namespace StarConcessionaria.Api.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwt = configuration.GetSection(JwtOptions.Section).Get<JwtOptions>() ?? new JwtOptions();
        // Permite ler de Jwt__Key ou Jwt_Key
        var key = !string.IsNullOrEmpty(jwt.Key) ? jwt.Key : configuration["Jwt_Key"];
        if (string.IsNullOrEmpty(key) || Encoding.UTF8.GetByteCount(key) < 32)
        {
            // Chave fallback para evitar que a API falhe na inicializacao caso a variavel nao tenha sido preenchida no Render
            key = "chave-padrao-fallback-star-concessionaria-api-min-32-chars";
        }
        jwt.Key = key;

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1),
                    ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 }
                };
            });
        services.AddAuthorization();
        return services;
    }
}
