using StarConcessionaria.Domain.Exceptions;

namespace StarConcessionaria.Domain.ValueObjects;

/// <summary>
/// Objeto de Valor (Value Object) que encapsula as informações de contato do cliente,
/// garantindo imutabilidade e integridade dos dados fornecidos em reservas, propostas e agendamentos.
/// </summary>
public record DadosCliente
{
    public string Nome { get; }
    public string Email { get; }
    public string Telefone { get; }

    public DadosCliente(string nome, string email, string telefone)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainValidationException("O nome do cliente é obrigatório.");

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            throw new DomainValidationException("E-mail do cliente inválido ou não informado.");

        if (string.IsNullOrWhiteSpace(telefone) || telefone.Trim().Length < 8)
            throw new DomainValidationException("Telefone do cliente inválido ou com formato incompleto.");

        Nome = nome.Trim();
        Email = email.Trim().ToLowerInvariant();
        Telefone = telefone.Trim();
    }
}
