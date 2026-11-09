using StarConcessionaria.Application.Common;
using StarConcessionaria.Domain.Entities;

namespace StarConcessionaria.Application.Veiculos;

public interface IVeiculoRepository
{
    Task<(IReadOnlyList<VeiculoCatalogoResponse> Itens, int Total)> ListarCatalogoAsync(CatalogoQuery filtro, CancellationToken ct = default);
    /// <summary>Retorna o veiculo com loja e fotos, sem rastreamento.</summary>
    Task<Veiculo?> ObterDetalheAsync(int id, CancellationToken ct = default);
    Task<Veiculo?> ObterParaAtualizacaoAsync(int id, CancellationToken ct = default);
    Task<FotoVeiculo?> ObterFotoAsync(int veiculoId, int fotoId, CancellationToken ct = default);
    Task<int> ContarFotosAsync(int veiculoId, CancellationToken ct = default);
    /// <summary>Adiciona a foto; se Principal, desmarca as demais de forma atomica.</summary>
    Task AdicionarFotoAsync(FotoVeiculo foto, CancellationToken ct = default);
    /// <summary>Remove a foto; se era a principal, promove a proxima (menor Ordem/Id).</summary>
    Task RemoverFotoAsync(FotoVeiculo foto, CancellationToken ct = default);
    Task DefinirFotoPrincipalAsync(FotoVeiculo foto, CancellationToken ct = default);
    Task<bool> VeiculoExisteAsync(int id, CancellationToken ct = default);
    Task<bool> LojaExisteAsync(int lojaId, CancellationToken ct = default);
    Task<IReadOnlyList<AlteracaoVeiculo>> ListarHistoricoAsync(int veiculoId, CancellationToken ct = default);
    Task AdicionarAsync(Veiculo veiculo, CancellationToken ct = default);
    Task AdicionarAlteracaoAsync(AlteracaoVeiculo alteracao, CancellationToken ct = default);
    /// <summary>Persiste veiculo e historico na mesma transacao.</summary>
    Task SalvarAsync(CancellationToken ct = default);
}

public interface IVeiculoService
{
    Task<Resultado<PagedResult<VeiculoCatalogoResponse>>> ListarCatalogoAsync(CatalogoQuery filtro, CancellationToken ct = default);
    Task<VeiculoDetalheResponse?> ObterAsync(int id, CancellationToken ct = default);
    Task<Resultado<VeiculoDetalheResponse>> CriarAsync(SalvarVeiculoRequest request, UsuarioAtual usuario, CancellationToken ct = default);
    Task<Resultado<VeiculoDetalheResponse>> AtualizarAsync(int id, SalvarVeiculoRequest request, UsuarioAtual usuario, CancellationToken ct = default);
    Task<Resultado<VeiculoDetalheResponse>> AlterarSituacaoAsync(int id, AlterarSituacaoRequest request, UsuarioAtual usuario, CancellationToken ct = default);
    Task<Resultado<VeiculoFotoResponse>> AdicionarFotoAsync(int veiculoId, AdicionarFotoRequest request, UsuarioAtual usuario, CancellationToken ct = default);
    Task<Resultado<bool>> RemoverFotoAsync(int veiculoId, int fotoId, UsuarioAtual usuario, CancellationToken ct = default);
    Task<Resultado<VeiculoFotoResponse>> DefinirFotoPrincipalAsync(int veiculoId, int fotoId, UsuarioAtual usuario, CancellationToken ct = default);
    Task<Resultado<IReadOnlyList<VeiculoHistoricoResponse>>> ObterHistoricoAsync(int id, CancellationToken ct = default);
}
