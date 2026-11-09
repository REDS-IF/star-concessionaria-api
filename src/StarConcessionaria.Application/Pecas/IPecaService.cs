using StarConcessionaria.Application.Common;
using StarConcessionaria.Domain.Entities;

namespace StarConcessionaria.Application.Pecas;

public interface IPecaRepository
{
    Task<(IReadOnlyList<Peca> Itens, int Total)> ListarAsync(PecaQuery filtro, CancellationToken ct = default);
    Task<Peca?> ObterPorIdAsync(int id, CancellationToken ct = default);
    Task<Peca?> ObterParaAtualizacaoAsync(int id, CancellationToken ct = default);
    Task AdicionarAsync(Peca peca, CancellationToken ct = default);
    Task SalvarAsync(CancellationToken ct = default);
    Task<Resultado<VendaPecaResponse>> RegistrarVendaAsync(int pecaId, RegistrarVendaPecaRequest request, CancellationToken ct = default);
}

public interface IPecaService
{
    Task<PagedResult<PecaResponse>> ListarAsync(PecaQuery filtro, CancellationToken ct = default);
    Task<PecaResponse?> ObterAsync(int id, CancellationToken ct = default);
    Task<PecaResponse> CriarAsync(SalvarPecaRequest request, CancellationToken ct = default);
    Task<PecaResponse?> AtualizarAsync(int id, SalvarPecaRequest request, CancellationToken ct = default);
    Task<Resultado<VendaPecaResponse>> RegistrarVendaAsync(int id, RegistrarVendaPecaRequest request, CancellationToken ct = default);
}
