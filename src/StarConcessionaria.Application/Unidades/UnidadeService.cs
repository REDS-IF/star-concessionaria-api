using StarConcessionaria.Domain.Entities;

namespace StarConcessionaria.Application.Unidades;

public class UnidadeService : IUnidadeService
{
    private readonly ILojaRepository _repository;

    public UnidadeService(ILojaRepository repository) => _repository = repository;

    public async Task<IReadOnlyList<UnidadeResponse>> ListarAsync(string? cidade, string? estado, CancellationToken ct = default)
    {
        var lojas = await _repository.ListarAsync(cidade?.Trim(), estado?.Trim(), ct);
        return lojas.Select(Mapear).ToList();
    }

    public async Task<UnidadeResponse?> ObterAsync(int id, CancellationToken ct = default)
    {
        var loja = await _repository.ObterPorIdAsync(id, ct);
        return loja is null ? null : Mapear(loja);
    }

    public async Task<UnidadeResponse> CriarAsync(SalvarUnidadeRequest request, CancellationToken ct = default)
    {
        var loja = new Loja();
        Aplicar(loja, request);
        await _repository.AdicionarAsync(loja, ct);
        await _repository.SalvarAsync(ct);
        return Mapear(loja);
    }

    public async Task<UnidadeResponse?> AtualizarAsync(int id, SalvarUnidadeRequest request, CancellationToken ct = default)
    {
        var loja = await _repository.ObterParaAtualizacaoAsync(id, ct);
        if (loja is null) return null;
        Aplicar(loja, request);
        await _repository.SalvarAsync(ct);
        return Mapear(loja);
    }

    private static void Aplicar(Loja l, SalvarUnidadeRequest r)
    {
        l.Nome = r.Nome.Trim();
        l.Telefone = r.Telefone?.Trim();
        l.Cep = r.Cep?.Trim();
        l.Endereco = r.Endereco?.Trim();
        l.Numero = r.Numero?.Trim();
        l.Complemento = r.Complemento?.Trim();
        l.Bairro = r.Bairro?.Trim();
        l.Cidade = r.Cidade?.Trim();
        l.Estado = r.Estado?.Trim().ToUpperInvariant();
        l.Latitude = r.Latitude;
        l.Longitude = r.Longitude;
    }

    private static UnidadeResponse Mapear(Loja l) => new(
        l.Id, l.Nome, l.Telefone, l.Cep, l.Endereco, l.Numero, l.Complemento,
        l.Bairro, l.Cidade, l.Estado, l.Latitude, l.Longitude);
}
