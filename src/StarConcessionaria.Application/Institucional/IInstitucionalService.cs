namespace StarConcessionaria.Application.Institucional;

public interface IInstitucionalService
{
    Task<IReadOnlyList<FormaPagamentoDetalheResponse>> ListarFormasPagamentoAsync(CancellationToken ct = default);
    Task<IReadOnlyList<FaqResponse>> ListarFaqsAsync(string? busca, CancellationToken ct = default);
}
