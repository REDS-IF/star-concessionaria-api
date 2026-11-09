using StarConcessionaria.Application.Common;
using StarConcessionaria.Domain.Entities;

namespace StarConcessionaria.Application.Propostas;

public interface IPropostaRepository
{
    Task<Veiculo?> ObterVeiculoAsync(int veiculoId, CancellationToken ct = default);
    Task<bool> LojaExisteAsync(int lojaId, CancellationToken ct = default);
    Task<FormaPagamento?> ObterFormaPagamentoAsync(string nome, CancellationToken ct = default);
    Task<Cliente?> ObterClienteAsync(string email, string telefone, CancellationToken ct = default);
    /// <summary>Adiciona contato (e cliente novo, se rastreado via navegacao) para persistir juntos.</summary>
    Task AdicionarContatoAsync(Contato contato, CancellationToken ct = default);
    Task SalvarAsync(CancellationToken ct = default);
}

public interface IPropostaService
{
    Task<Resultado<PropostaResponse>> SolicitarAsync(SolicitarPropostaRequest request, CancellationToken ct = default);
}
