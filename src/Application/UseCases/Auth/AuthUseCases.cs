using StarConcessionaria.Application.DTOs;
using StarConcessionaria.Application.Services;
using StarConcessionaria.Domain.Exceptions;
using StarConcessionaria.Domain.Repositories;

namespace StarConcessionaria.Application.UseCases.Auth;

/// <summary>
/// Caso de Uso: Autenticação de Usuário e Emissão de Token JWT com RBAC.
/// </summary>
public class AutenticarUsuarioUseCase
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public AutenticarUsuarioUseCase(
        IUsuarioRepository usuarioRepository, 
        IPasswordHasher passwordHasher, 
        ITokenService tokenService)
    {
        _usuarioRepository = usuarioRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<LoginResponseDto> ExecutarAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var usuario = await _usuarioRepository.ObterPorEmailAsync(request.Email, cancellationToken);
        if (usuario == null)
            throw new DomainValidationException("Credenciais inválidas. Verifique seu e-mail e senha.");

        var senhaValida = _passwordHasher.VerificarSenha(request.Senha, usuario.SenhaHash);
        if (!senhaValida)
            throw new DomainValidationException("Credenciais inválidas. Verifique seu e-mail e senha.");

        var (token, expiraEm) = _tokenService.GerarToken(usuario);

        return new LoginResponseDto(
            Id: usuario.Id,
            Nome: usuario.Nome,
            Email: usuario.Email,
            Perfil: usuario.Perfil.ToString(),
            Token: token,
            ExpiraEm: expiraEm
        );
    }
}
