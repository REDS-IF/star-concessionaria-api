namespace StarConcessionaria.Application.Dashboard;

public interface IDashboardService
{
    Task<IndicadoresGerenciaisResponse> ObterIndicadoresAsync(int? unidadeId, CancellationToken ct = default);
}
