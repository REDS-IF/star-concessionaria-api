using StarConcessionaria.Domain.Entities;

namespace StarConcessionaria.Application.Services;

/// <summary>
/// Contrato de geração e validação de tokens JWT para autenticação e RBAC.
/// </summary>
public interface ITokenService
{
    (string Token, DateTime ExpiraEm) GerarToken(Usuario usuario);
}

/// <summary>
/// Contrato de hashing seguro de senhas.
/// </summary>
public interface IPasswordHasher
{
    string HashSenha(string senhaPlana);
    bool VerificarSenha(string senhaPlana, string hashArmazenado);
}
