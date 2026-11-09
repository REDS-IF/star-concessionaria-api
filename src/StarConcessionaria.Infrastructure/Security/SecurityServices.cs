using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using StarConcessionaria.Domain.Entities;

namespace StarConcessionaria.Infrastructure.Security;

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string passwordHash);
}

public class PasswordHasher : IPasswordHasher
{
    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password);
    public bool Verify(string password, string passwordHash) => BCrypt.Net.BCrypt.Verify(password, passwordHash);
}

public interface ITokenService
{
    string GenerateToken(UsuarioSistema usuario);
}

public class TokenService : ITokenService
{
    private readonly IConfiguration _config;

    public TokenService(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateToken(UsuarioSistema usuario)
    {
        var secret = _config["Jwt:SecretKey"] ?? "StarConcessionaria_Super_Secret_Key_2026_Minimum_32_Chars";
        var issuer = _config["Jwt:Issuer"] ?? "StarConcessionariaApi";
        var audience = _config["Jwt:Audience"] ?? "StarConcessionariaWeb";

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.IdUsu.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, usuario.EmailUsu),
            new Claim(ClaimTypes.Name, usuario.NomeUsu),
            new Claim(ClaimTypes.Role, usuario.PerfilUsu)
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
