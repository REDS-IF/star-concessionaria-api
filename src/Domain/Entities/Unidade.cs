using StarConcessionaria.Domain.Exceptions;

namespace StarConcessionaria.Domain.Entities;

/// <summary>
/// Entidade de Domínio Pura: Unidade (Branch / Dealership Unit)
/// Representa uma loja física da Star Concessionária responsável pela guarda dos veículos,
/// atendimento aos clientes e realização de visitas/test-drives.
/// </summary>
public class Unidade
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; }
    public string Endereco { get; private set; }
    public string Cidade { get; private set; }
    public string Estado { get; private set; }
    public string Telefone { get; private set; }
    public string HorarioFuncionamento { get; private set; }

    public Unidade(
        Guid id,
        string nome,
        string endereco,
        string cidade,
        string estado,
        string telefone,
        string horarioFuncionamento)
    {
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        ValidarParametros(nome, endereco, cidade, estado, telefone, horarioFuncionamento);

        Nome = nome.Trim();
        Endereco = endereco.Trim();
        Cidade = cidade.Trim();
        Estado = estado.Trim().ToUpperInvariant();
        Telefone = telefone.Trim();
        HorarioFuncionamento = horarioFuncionamento.Trim();
    }

    public void AtualizarDados(
        string nome,
        string endereco,
        string cidade,
        string estado,
        string telefone,
        string horarioFuncionamento)
    {
        ValidarParametros(nome, endereco, cidade, estado, telefone, horarioFuncionamento);

        Nome = nome.Trim();
        Endereco = endereco.Trim();
        Cidade = cidade.Trim();
        Estado = estado.Trim().ToUpperInvariant();
        Telefone = telefone.Trim();
        HorarioFuncionamento = horarioFuncionamento.Trim();
    }

    private static void ValidarParametros(
        string nome,
        string endereco,
        string cidade,
        string estado,
        string telefone,
        string horarioFuncionamento)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainValidationException("O nome da unidade é obrigatório.");

        if (string.IsNullOrWhiteSpace(endereco))
            throw new DomainValidationException("O endereço da unidade é obrigatório.");

        if (string.IsNullOrWhiteSpace(cidade))
            throw new DomainValidationException("A cidade da unidade é obrigatória.");

        if (string.IsNullOrWhiteSpace(estado) || estado.Trim().Length != 2)
            throw new DomainValidationException("A sigla do estado deve conter exatamente 2 caracteres (UF).");

        if (string.IsNullOrWhiteSpace(telefone))
            throw new DomainValidationException("O telefone de contato da unidade é obrigatório.");

        if (string.IsNullOrWhiteSpace(horarioFuncionamento))
            throw new DomainValidationException("O horário de funcionamento é obrigatório.");
    }
}
