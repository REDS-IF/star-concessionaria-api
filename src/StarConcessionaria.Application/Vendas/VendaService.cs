using StarConcessionaria.Application.Common;
using StarConcessionaria.Application.Veiculos;

namespace StarConcessionaria.Application.Vendas;

public class VendaService : IVendaService
{
    private readonly IVendaRepository _repository;

    public VendaService(IVendaRepository repository) => _repository = repository;

    public Task<Resultado<VendaResponse>> RegistrarAsync(RegistrarVendaRequest request, UsuarioAtual vendedor, CancellationToken ct = default) =>
        _repository.RegistrarVendaAsync(request, vendedor, ct);

    public async Task<PagedResult<VendaResponse>> ListarAsync(VendaQuery filtro, CancellationToken ct = default)
    {
        var pagina = Math.Max(1, filtro.Pagina);
        var itens = Math.Clamp(filtro.ItensPorPagina, 1, 50);
        var normalizado = new VendaQuery
        {
            UnidadeId = filtro.UnidadeId,
            VendedorId = filtro.VendedorId,
            PeriodoInicio = filtro.PeriodoInicio,
            PeriodoFim = filtro.PeriodoFim,
            Pagina = pagina,
            ItensPorPagina = itens
        };
        var (lista, total) = await _repository.ListarAsync(normalizado, ct);
        return new PagedResult<VendaResponse>(lista, pagina, itens, total);
    }
}
