using StarConcessionaria.Application.DTOs;
using StarConcessionaria.Domain.Entities;

namespace StarConcessionaria.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
}

public interface IUnidadeService
{
    Task<IEnumerable<LojaResponse>> ObterLojasAsync();
    Task<LojaResponse?> ObterLojaPorIdAsync(int id);
    Task<LojaResponse> CriarLojaAsync(CriarLojaRequest request);
}

public interface IVeiculoService
{
    Task<IEnumerable<VeiculoCatalogoResponse>> ObterCatalogoAsync(FiltroVeiculosRequest filtros);
    Task<Veiculo?> ObterDetalhesAsync(int id);
    Task<Veiculo> CriarVeiculoAsync(CriarVeiculoRequest request);
    Task<bool> AtualizarSituacaoAsync(int id, string novaSituacao, string? justificativa = null);
}

public interface IFaqService
{
    Task<IEnumerable<FaqResponse>> ObterFaqsAsync();
}
