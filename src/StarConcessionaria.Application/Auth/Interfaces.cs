using StarConcessionaria.Domain.Entities;

namespace StarConcessionaria.Application.Auth;

public interface IUsuarioRepository
{
    Task<UsuarioSistema?> ObterPorEmailAsync(string email, CancellationToken ct = default);
    Task<UsuarioSistema?> ObterPorIdAsync(int id, CancellationToken ct = default);
}

public interface IPasswordHasher
{
    string Hash(string senha);
    bool Verificar(string senha, string hash);
}

public interface ITokenService
{
    (string Token, DateTimeOffset ExpiraEm) Gerar(UsuarioSistema usuario);
}

public interface IAuthService
{
    /// <summary>Retorna null quando as credenciais sao invalidas.</summary>
    Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<UsuarioResponse?> ObterUsuarioAsync(int id, CancellationToken ct = default);
}
