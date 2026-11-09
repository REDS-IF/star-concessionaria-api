using StarConcessionaria.Application.Common;

namespace StarConcessionaria.Application.Agendamentos;

public class AgendamentoService : IAgendamentoService
{
    private readonly IAgendamentoRepository _repository;

    public AgendamentoService(IAgendamentoRepository repository) => _repository = repository;

    public Task<Resultado<IReadOnlyList<string>>> ObterHorariosDisponiveisAsync(int unidadeId, DateOnly data, int? veiculoId, CancellationToken ct = default)
    {
        if (unidadeId <= 0)
            return Task.FromResult(Resultado<IReadOnlyList<string>>.Falha(TipoErro.Invalido, "UnidadeId invalido."));
        return _repository.ObterHorariosDisponiveisAsync(unidadeId, data, veiculoId, ct);
    }

    public Task<Resultado<AgendamentoResponse>> AgendarVisitaAsync(AgendarVisitaRequest request, CancellationToken ct = default) =>
        _repository.AgendarVisitaAsync(request, ct);

    public Task<Resultado<AgendamentoResponse>> AgendarTestDriveAsync(AgendarTestDriveRequest request, CancellationToken ct = default) =>
        _repository.AgendarTestDriveAsync(request, ct);

    public async Task<PagedResult<AgendamentoResponse>> ListarAsync(AgendamentoQuery filtro, CancellationToken ct = default)
    {
        var pagina = Math.Max(1, filtro.Pagina);
        var itens = Math.Clamp(filtro.ItensPorPagina, 1, 50);
        var normalizado = new AgendamentoQuery
        {
            UnidadeId = filtro.UnidadeId,
            Status = filtro.Status?.Trim(),
            Tipo = filtro.Tipo?.Trim(),
            Data = filtro.Data,
            Pagina = pagina,
            ItensPorPagina = itens
        };
        var (lista, total) = await _repository.ListarAsync(normalizado, ct);
        return new PagedResult<AgendamentoResponse>(lista, pagina, itens, total);
    }

    public Task<Resultado<AgendamentoResponse>> AtualizarStatusAsync(int id, AtualizarStatusAgendamentoRequest request, CancellationToken ct = default)
    {
        var status = SituacaoAgendamento.Normalizar(request.Status);
        if (status is null || status == SituacaoAgendamento.Solicitado)
            return Task.FromResult(Resultado<AgendamentoResponse>.Falha(TipoErro.Invalido, "Status invalido. Use: Confirmado, Realizado ou Cancelado."));

        return _repository.AtualizarStatusAsync(id, status, ct);
    }
}
