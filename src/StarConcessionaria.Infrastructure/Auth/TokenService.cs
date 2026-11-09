using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StarConcessionaria.Application.Auth;
using StarConcessionaria.Domain.Entities;

namespace StarConcessionaria.Infrastructure.Auth;

public class TokenService : ITokenService
{
    private readonly JwtOptions _options;

    public TokenService(IOptions<JwtOptions> options) => _options = options.Value;

    public (string Token, DateTimeOffset ExpiraEm) Gerar(UsuarioSistema usuario)
    {
        var perfil = Perfis.Normalizar(usuario.Perfil)
            ?? throw new InvalidOperationException($"Perfil invalido: {usuario.Perfil}");

        var expiraEm = DateTimeOffset.UtcNow.AddMinutes(_options.ExpiracaoMinutos);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.Nome),
            new Claim(ClaimTypes.Role, perfil),
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key)), SecurityAlgorithms.HmacSha256);

        var jwt = new JwtSecurityToken(_options.Issuer, _options.Audience, claims,
            notBefore: DateTime.UtcNow, expires: expiraEm.UtcDateTime, signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(jwt), expiraEm);
    }
}
