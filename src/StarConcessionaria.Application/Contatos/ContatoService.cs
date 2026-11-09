using StarConcessionaria.Application.Common;
using StarConcessionaria.Application.Veiculos;

namespace StarConcessionaria.Application.Contatos;

public class ContatoService : IContatoService
{
    private readonly IContatoRepository _repository;

    public ContatoService(IContatoRepository repository) => _repository = repository;

    public Task<Resultado<ContatoResponse>> CriarContatoAsync(NovoContatoRequest r, CancellationToken ct = default)
    {
        var msg = $"Assunto: {r.Assunto.Trim()}\nMensagem: {r.Mensagem.Trim()}";
        return _repository.RegistrarContatoAsync(TipoContato.Informacao, r.Nome, r.Email, r.Telefone, msg, r.VeiculoId, r.UnidadeId, ct);
    }

    public Task<Resultado<ContatoResponse>> EncaminharVendedorAsync(ContatoVendedorRequest r, CancellationToken ct = default) =>
        _repository.RegistrarContatoAsync(TipoContato.ContatoVendedor, r.Nome, r.Email, r.Telefone, r.Mensagem.Trim(), r.VeiculoId, r.UnidadeId, ct);

    public async Task<PagedResult<ContatoResponse>> ListarAsync(ContatoQuery filtro, CancellationToken ct = default)
    {
        var pagina = Math.Max(1, filtro.Pagina);
        var itens = Math.Clamp(filtro.ItensPorPagina, 1, 50);
        var normalizado = new ContatoQuery
        {
            Tipo = filtro.Tipo?.Trim(),
            Respondido = filtro.Respondido,
            UnidadeId = filtro.UnidadeId,
            Pagina = pagina,
            ItensPorPagina = itens
        };
        var (lista, total) = await _repository.ListarAsync(normalizado, ct);
        return new PagedResult<ContatoResponse>(lista, pagina, itens, total);
    }

    public Task<Resultado<ContatoResponse>> ResponderAsync(int id, ResponderContatoRequest request, UsuarioAtual usuario, CancellationToken ct = default) =>
        _repository.ResponderAsync(id, request.NotasResposta.Trim(), usuario, ct);
}
