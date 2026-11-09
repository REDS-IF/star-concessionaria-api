using StarConcessionaria.Application.Common;
using StarConcessionaria.Domain.Entities;

namespace StarConcessionaria.Application.Pecas;

public class PecaService : IPecaService
{
    private readonly IPecaRepository _repository;

    public PecaService(IPecaRepository repository) => _repository = repository;

    public async Task<PagedResult<PecaResponse>> ListarAsync(PecaQuery filtro, CancellationToken ct = default)
    {
        var pagina = Math.Max(1, filtro.Pagina);
        var itens = Math.Clamp(filtro.ItensPorPagina, 1, 50);
        var normalizado = new PecaQuery
        {
            Busca = filtro.Busca?.Trim(),
            Disponivel = filtro.Disponivel,
            Pagina = pagina,
            ItensPorPagina = itens
        };

        var (lista, total) = await _repository.ListarAsync(normalizado, ct);
        return new PagedResult<PecaResponse>(lista.Select(Mapear).ToList(), pagina, itens, total);
    }

    public async Task<PecaResponse?> ObterAsync(int id, CancellationToken ct = default)
    {
        var peca = await _repository.ObterPorIdAsync(id, ct);
        return peca is null ? null : Mapear(peca);
    }

    public async Task<PecaResponse> CriarAsync(SalvarPecaRequest request, CancellationToken ct = default)
    {
        var peca = new Peca();
        Aplicar(peca, request);
        await _repository.AdicionarAsync(peca, ct);
        await _repository.SalvarAsync(ct);
        return Mapear(peca);
    }

    public async Task<PecaResponse?> AtualizarAsync(int id, SalvarPecaRequest request, CancellationToken ct = default)
    {
        var peca = await _repository.ObterParaAtualizacaoAsync(id, ct);
        if (peca is null) return null;
        Aplicar(peca, request);
        await _repository.SalvarAsync(ct);
        return Mapear(peca);
    }

    public Task<Resultado<VendaPecaResponse>> RegistrarVendaAsync(int id, RegistrarVendaPecaRequest request, CancellationToken ct = default) =>
        _repository.RegistrarVendaAsync(id, request, ct);

    private static void Aplicar(Peca p, SalvarPecaRequest r)
    {
        p.Nome = r.Nome.Trim();
        p.Descricao = r.Descricao?.Trim();
        p.Preco = r.Preco;
        p.QuantidadeEstoque = r.QuantidadeEstoque;
    }

    private static PecaResponse Mapear(Peca p) =>
        new(p.Id, p.Nome, p.Descricao, p.Preco, p.QuantidadeEstoque, p.QuantidadeEstoque > 0);
}
