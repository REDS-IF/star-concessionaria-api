using StarConcessionaria.Application.Common;
using StarConcessionaria.Application.Veiculos;

namespace StarConcessionaria.Application.Vendas;

public interface IVendaRepository
{
    Task<Resultado<VendaResponse>> RegistrarVendaAsync(RegistrarVendaRequest request, UsuarioAtual vendedor, CancellationToken ct = default);
    Task<(IReadOnlyList<VendaResponse> Itens, int Total)> ListarAsync(VendaQuery filtro, CancellationToken ct = default);
}

public interface IVendaService
{
    Task<Resultado<VendaResponse>> RegistrarAsync(RegistrarVendaRequest request, UsuarioAtual vendedor, CancellationToken ct = default);
    Task<PagedResult<VendaResponse>> ListarAsync(VendaQuery filtro, CancellationToken ct = default);
}
