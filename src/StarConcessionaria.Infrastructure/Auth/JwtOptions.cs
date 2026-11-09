namespace StarConcessionaria.Infrastructure.Auth;

public class JwtOptions
{
    public const string Section = "Jwt";

    public string Issuer { get; set; } = "StarConcessionaria";
    public string Audience { get; set; } = "StarConcessionaria.Clients";
    /// <summary>Chave simetrica HMAC-SHA256 (minimo 32 caracteres). Nunca versionar valores de producao.</summary>
    public string Key { get; set; } = string.Empty;
    public int ExpiracaoMinutos { get; set; } = 60;
}
