using StarConcessionaria.Application.Common;
using StarConcessionaria.Application.Veiculos;

namespace StarConcessionaria.Application.Reservas;

public class ReservaService : IReservaService
{
    private readonly IReservaRepository _repository;

    public ReservaService(IReservaRepository repository) => _repository = repository;

    public Task<Resultado<ReservaResponse>> SolicitarAsync(SolicitarReservaRequest request, CancellationToken ct = default) =>
        _repository.SolicitarComBloqueioAtomicoAsync(request, ct);

    public Task<ReservaResponse?> ObterAsync(int id, CancellationToken ct = default) =>
        _repository.ObterPorIdAsync(id, ct);

    public async Task<PagedResult<ReservaResponse>> ListarAsync(ReservaQuery filtro, CancellationToken ct = default)
    {
        var pagina = Math.Max(1, filtro.Pagina);
        var itens = Math.Clamp(filtro.ItensPorPagina, 1, 50);
        var normalizado = new ReservaQuery
        {
            Status = filtro.Status?.Trim(),
            VeiculoId = filtro.VeiculoId,
            DataInicio = filtro.DataInicio,
            Pagina = pagina,
            ItensPorPagina = itens
        };
        var (lista, total) = await _repository.ListarAsync(normalizado, ct);
        return new PagedResult<ReservaResponse>(lista, pagina, itens, total);
    }

    public Task<Resultado<ReservaResponse>> AtualizarStatusAsync(int id, AtualizarStatusReservaRequest request, UsuarioAtual usuario, CancellationToken ct = default)
    {
        var status = SituacaoReserva.Normalizar(request.NovoStatus);
        if (status is null || status == SituacaoReserva.Pendente)
            return Task.FromResult(Resultado<ReservaResponse>.Falha(TipoErro.Invalido, "Status invalido. Use: Confirmada, Cancelada ou Concluida."));

        return _repository.AtualizarStatusAsync(id, status, request.Motivo, usuario, ct);
    }
}
