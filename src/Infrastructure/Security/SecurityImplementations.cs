using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using StarConcessionaria.Application.Services;
using StarConcessionaria.Domain.Entities;

namespace StarConcessionaria.Infrastructure.Security;

/// <summary>
/// Implementação do serviço de geração e validação de tokens JWT.
/// Inclui claims de identidade e Perfil de acesso (RBAC).
/// </summary>
public class TokenService : ITokenService
{
    private readonly string _jwtSecret;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expiraHoras;

    public TokenService(IConfiguration configuration)
    {
        _jwtSecret = configuration["Jwt:SecretKey"] ?? "StarConcessionaria_ChaveSuperSecreta_FullCycle_2026_Minimo32Caracteres!";
        _issuer = configuration["Jwt:Issuer"] ?? "StarConcessionariaApi";
        _audience = configuration["Jwt:Audience"] ?? "StarConcessionariaApp";
        _expiraHoras = int.TryParse(configuration["Jwt:ExpiresInHours"], out var h) ? h : 8;
    }

    public (string Token, DateTime ExpiraEm) GerarToken(Usuario usuario)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSecret);
        var expiraEm = DateTime.UtcNow.AddHours(_expiraHoras);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Nome),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Role, usuario.Perfil.ToString())
            }),
            Expires = expiraEm,
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return (tokenHandler.WriteToken(token), expiraEm);
    }
}

/// <summary>
/// Implementação robusta de hashing de senhas utilizando PBKDF2 (Rfc2898DeriveBytes com HMAC-SHA256).
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 10000;

    public string HashSenha(string senhaPlana)
    {
        using var algorithm = new Rfc2898DeriveBytes(
            senhaPlana, 
            SaltSize, 
            Iterations, 
            HashAlgorithmName.SHA256);

        var salt = algorithm.Salt;
        var key = algorithm.GetBytes(KeySize);

        return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(key)}";
    }

    public bool VerificarSenha(string senhaPlana, string hashArmazenado)
    {
        var parts = hashArmazenado.Split('.');
        if (parts.Length != 2)
            return false;

        var salt = Convert.FromBase64String(parts[0]);
        var key = Convert.FromBase64String(parts[1]);

        using var algorithm = new Rfc2898DeriveBytes(
            senhaPlana, 
            salt, 
            Iterations, 
            HashAlgorithmName.SHA256);

        var keyToCheck = algorithm.GetBytes(KeySize);
        return CryptographicOperations.FixedTimeEquals(key, keyToCheck);
    }
}
