using StarConcessionaria.Application.Common;
using StarConcessionaria.Application.Veiculos;

namespace StarConcessionaria.Application.Contatos;

public interface IContatoRepository
{
    Task<Resultado<ContatoResponse>> RegistrarContatoAsync(string tipo, string nome, string email, string telefone, string mensagem, int? veiculoId, int? unidadeId, CancellationToken ct = default);
    Task<(IReadOnlyList<ContatoResponse> Itens, int Total)> ListarAsync(ContatoQuery filtro, CancellationToken ct = default);
    Task<Resultado<ContatoResponse>> ResponderAsync(int id, string notasResposta, UsuarioAtual usuario, CancellationToken ct = default);
}

public interface IContatoService
{
    Task<Resultado<ContatoResponse>> CriarContatoAsync(NovoContatoRequest request, CancellationToken ct = default);
    Task<Resultado<ContatoResponse>> EncaminharVendedorAsync(ContatoVendedorRequest request, CancellationToken ct = default);
    Task<PagedResult<ContatoResponse>> ListarAsync(ContatoQuery filtro, CancellationToken ct = default);
    Task<Resultado<ContatoResponse>> ResponderAsync(int id, ResponderContatoRequest request, UsuarioAtual usuario, CancellationToken ct = default);
}
