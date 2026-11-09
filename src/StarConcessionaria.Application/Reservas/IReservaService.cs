using StarConcessionaria.Application.Common;
using StarConcessionaria.Application.Veiculos;

namespace StarConcessionaria.Application.Reservas;

public interface IReservaRepository
{
    /// <summary>Executa a criacao de reserva em transacao com FOR UPDATE no veiculo (RN01, RN03).</summary>
    Task<Resultado<ReservaResponse>> SolicitarComBloqueioAtomicoAsync(SolicitarReservaRequest request, CancellationToken ct = default);
    Task<ReservaResponse?> ObterPorIdAsync(int id, CancellationToken ct = default);
    Task<(IReadOnlyList<ReservaResponse> Itens, int Total)> ListarAsync(ReservaQuery filtro, CancellationToken ct = default);
    Task<Resultado<ReservaResponse>> AtualizarStatusAsync(int id, string novoStatus, string? motivo, UsuarioAtual usuario, CancellationToken ct = default);
}

public interface IReservaService
{
    Task<Resultado<ReservaResponse>> SolicitarAsync(SolicitarReservaRequest request, CancellationToken ct = default);
    Task<ReservaResponse?> ObterAsync(int id, CancellationToken ct = default);
    Task<PagedResult<ReservaResponse>> ListarAsync(ReservaQuery filtro, CancellationToken ct = default);
    Task<Resultado<ReservaResponse>> AtualizarStatusAsync(int id, AtualizarStatusReservaRequest request, UsuarioAtual usuario, CancellationToken ct = default);
}
