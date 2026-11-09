using StarConcessionaria.Domain.Entities;

namespace StarConcessionaria.Application.Unidades;

public interface ILojaRepository
{
    Task<IReadOnlyList<Loja>> ListarAsync(string? cidade, string? estado, CancellationToken ct = default);
    Task<Loja?> ObterPorIdAsync(int id, CancellationToken ct = default);
    /// <summary>Retorna a entidade rastreada para atualizacao.</summary>
    Task<Loja?> ObterParaAtualizacaoAsync(int id, CancellationToken ct = default);
    Task AdicionarAsync(Loja loja, CancellationToken ct = default);
    Task SalvarAsync(CancellationToken ct = default);
}

public interface IUnidadeService
{
    Task<IReadOnlyList<UnidadeResponse>> ListarAsync(string? cidade, string? estado, CancellationToken ct = default);
    Task<UnidadeResponse?> ObterAsync(int id, CancellationToken ct = default);
    Task<UnidadeResponse> CriarAsync(SalvarUnidadeRequest request, CancellationToken ct = default);
    Task<UnidadeResponse?> AtualizarAsync(int id, SalvarUnidadeRequest request, CancellationToken ct = default);
}
