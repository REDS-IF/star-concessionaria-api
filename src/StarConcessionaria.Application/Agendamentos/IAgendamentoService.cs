using StarConcessionaria.Application.Common;

namespace StarConcessionaria.Application.Agendamentos;

public interface IAgendamentoRepository
{
    Task<Resultado<IReadOnlyList<string>>> ObterHorariosDisponiveisAsync(int unidadeId, DateOnly data, int? veiculoId, CancellationToken ct = default);
    Task<Resultado<AgendamentoResponse>> AgendarVisitaAsync(AgendarVisitaRequest request, CancellationToken ct = default);
    Task<Resultado<AgendamentoResponse>> AgendarTestDriveAsync(AgendarTestDriveRequest request, CancellationToken ct = default);
    Task<(IReadOnlyList<AgendamentoResponse> Itens, int Total)> ListarAsync(AgendamentoQuery filtro, CancellationToken ct = default);
    Task<Resultado<AgendamentoResponse>> AtualizarStatusAsync(int id, string novoStatus, CancellationToken ct = default);
}

public interface IAgendamentoService
{
    Task<Resultado<IReadOnlyList<string>>> ObterHorariosDisponiveisAsync(int unidadeId, DateOnly data, int? veiculoId, CancellationToken ct = default);
    Task<Resultado<AgendamentoResponse>> AgendarVisitaAsync(AgendarVisitaRequest request, CancellationToken ct = default);
    Task<Resultado<AgendamentoResponse>> AgendarTestDriveAsync(AgendarTestDriveRequest request, CancellationToken ct = default);
    Task<PagedResult<AgendamentoResponse>> ListarAsync(AgendamentoQuery filtro, CancellationToken ct = default);
    Task<Resultado<AgendamentoResponse>> AtualizarStatusAsync(int id, AtualizarStatusAgendamentoRequest request, CancellationToken ct = default);
}
