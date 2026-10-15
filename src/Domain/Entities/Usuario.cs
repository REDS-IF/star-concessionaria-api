using StarConcessionaria.Domain.Enums;
using StarConcessionaria.Domain.Exceptions;

namespace StarConcessionaria.Domain.Entities;

/// <summary>
/// Entidade de Domínio Pura: Usuario (User)
/// Representa os usuários autenticados no sistema com perfis de permissão (RBAC).
/// </summary>
public class Usuario
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; }
    public string Email { get; private set; }
    public string SenhaHash { get; private set; }
    public PerfilUsuario Perfil { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public Usuario(
        Guid id,
        string nome,
        string email,
        string senhaHash,
        PerfilUsuario perfil,
        DateTime? createdAt = null)
    {
        Id = id == Guid.Empty ? Guid.NewGuid() : id;

        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainValidationException("O nome do usuário é obrigatório.");

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            throw new DomainValidationException("E-mail inválido ou não informado.");

        if (string.IsNullOrWhiteSpace(senhaHash))
            throw new DomainValidationException("O hash de senha é obrigatório.");

        Nome = nome.Trim();
        Email = email.Trim().ToLowerInvariant();
        SenhaHash = senhaHash;
        Perfil = perfil;
        CreatedAt = createdAt ?? DateTime.UtcNow;
    }

    public void AlterarSenha(string novoHash)
    {
        if (string.IsNullOrWhiteSpace(novoHash))
            throw new DomainValidationException("O novo hash de senha é obrigatório.");

        SenhaHash = novoHash;
    }

    public void AlterarPerfil(PerfilUsuario novoPerfil)
    {
        Perfil = novoPerfil;
    }
}
