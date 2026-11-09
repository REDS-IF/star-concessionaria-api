using StarConcessionaria.Domain.Entities;

namespace StarConcessionaria.Application.Auth;

public class AuthService : IAuthService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    // Hash valido (BCrypt de valor aleatorio) para igualar o tempo de resposta quando o e-mail nao existe.
    private readonly string _hashFalso;

    public AuthService(IUsuarioRepository usuarioRepository, IPasswordHasher passwordHasher, ITokenService tokenService)
    {
        _usuarioRepository = usuarioRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _hashFalso = passwordHasher.Hash(Guid.NewGuid().ToString("N"));
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var usuario = await _usuarioRepository.ObterPorEmailAsync(request.Email.Trim().ToLowerInvariant(), ct);

        var senhaOk = _passwordHasher.Verificar(request.Senha, usuario?.SenhaHash ?? _hashFalso);
        if (usuario is null || !senhaOk || Perfis.Normalizar(usuario.Perfil) is null)
            return null;

        var (token, expiraEm) = _tokenService.Gerar(usuario);
        return new LoginResponse(token, expiraEm, Mapear(usuario));
    }

    public async Task<UsuarioResponse?> ObterUsuarioAsync(int id, CancellationToken ct = default)
    {
        var usuario = await _usuarioRepository.ObterPorIdAsync(id, ct);
        return usuario is null ? null : Mapear(usuario);
    }

    private static UsuarioResponse Mapear(UsuarioSistema u) =>
        new(u.Id, u.Nome, u.Email, Perfis.Normalizar(u.Perfil) ?? u.Perfil);
}
