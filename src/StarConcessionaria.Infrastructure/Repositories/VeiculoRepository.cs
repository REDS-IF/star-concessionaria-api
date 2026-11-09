using Microsoft.EntityFrameworkCore;
using StarConcessionaria.Application.Veiculos;
using StarConcessionaria.Domain.Entities;
using StarConcessionaria.Infrastructure.Persistence;

namespace StarConcessionaria.Infrastructure.Repositories;

public class VeiculoRepository : IVeiculoRepository
{
    private readonly ApplicationDbContext _db;

    public VeiculoRepository(ApplicationDbContext db) => _db = db;

    public async Task<(IReadOnlyList<VeiculoCatalogoResponse> Itens, int Total)> ListarCatalogoAsync(CatalogoQuery f, CancellationToken ct = default)
    {
        var query = _db.Veiculos.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(f.Marca))
            query = query.Where(v => EF.Functions.ILike(v.Marca, "%" + EscaparLike(f.Marca) + "%"));
        if (!string.IsNullOrWhiteSpace(f.Modelo))
            query = query.Where(v => EF.Functions.ILike(v.Modelo, "%" + EscaparLike(f.Modelo) + "%"));
        if (!string.IsNullOrWhiteSpace(f.Tipo))
            query = query.Where(v => EF.Functions.ILike(v.Tipo, EscaparLike(f.Tipo)));
        if (f.PrecoMin.HasValue) query = query.Where(v => v.Preco >= f.PrecoMin.Value);
        if (f.PrecoMax.HasValue) query = query.Where(v => v.Preco <= f.PrecoMax.Value);
        if (f.AnoMin.HasValue) query = query.Where(v => v.Ano >= f.AnoMin.Value);
        if (f.AnoMax.HasValue) query = query.Where(v => v.Ano <= f.AnoMax.Value);
        if (f.UnidadeId.HasValue) query = query.Where(v => v.LojaId == f.UnidadeId.Value);
        if (f.SomenteDisponiveis) query = query.Where(v => v.Situacao == SituacaoVeiculo.Disponivel);

        var total = await query.CountAsync(ct);

        var itens = await query
            .OrderByDescending(v => v.Id)
            .Skip((f.Pagina - 1) * f.ItensPorPagina)
            .Take(f.ItensPorPagina)
            .Select(v => new VeiculoCatalogoResponse(
                v.Id, v.LojaId, v.Loja!.Nome, v.Tipo, v.Marca, v.Modelo, v.Ano, v.Preco, v.Quilometragem,
                v.Cor, v.Situacao,
                v.Fotos.OrderByDescending(x => x.Principal).ThenBy(x => x.Ordem).ThenBy(x => x.Id).Select(x => x.Url).FirstOrDefault()))
            .ToListAsync(ct);

        return (itens, total);
    }

    private static string EscaparLike(string valor) =>
        valor.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");

    public Task<Veiculo?> ObterDetalheAsync(int id, CancellationToken ct = default) =>
        _db.Veiculos.AsNoTracking().Include(v => v.Loja).Include(v => v.Fotos)
            .FirstOrDefaultAsync(v => v.Id == id, ct);

    public Task<Veiculo?> ObterParaAtualizacaoAsync(int id, CancellationToken ct = default) =>
        _db.Veiculos.FirstOrDefaultAsync(v => v.Id == id, ct);

    public Task<FotoVeiculo?> ObterFotoAsync(int veiculoId, int fotoId, CancellationToken ct = default) =>
        _db.FotosVeiculo.FirstOrDefaultAsync(f => f.Id == fotoId && f.VeiculoId == veiculoId, ct);

    public Task<int> ContarFotosAsync(int veiculoId, CancellationToken ct = default) =>
        _db.FotosVeiculo.CountAsync(f => f.VeiculoId == veiculoId, ct);

    public async Task AdicionarFotoAsync(FotoVeiculo foto, CancellationToken ct = default)
    {
        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        if (foto.Principal) await DesmarcarPrincipaisAsync(foto.VeiculoId, ct);
        _db.FotosVeiculo.Add(foto);
        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }

    public async Task RemoverFotoAsync(FotoVeiculo foto, CancellationToken ct = default)
    {
        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        var eraPrincipal = foto.Principal;
        _db.FotosVeiculo.Remove(foto);
        await _db.SaveChangesAsync(ct);

        if (eraPrincipal)
        {
            var proximaId = await _db.FotosVeiculo.Where(f => f.VeiculoId == foto.VeiculoId)
                .OrderBy(f => f.Ordem).ThenBy(f => f.Id).Select(f => (int?)f.Id).FirstOrDefaultAsync(ct);
            if (proximaId is not null)
                await _db.FotosVeiculo.Where(f => f.Id == proximaId)
                    .ExecuteUpdateAsync(s => s.SetProperty(f => f.Principal, true), ct);
        }
        await tx.CommitAsync(ct);
    }

    public async Task DefinirFotoPrincipalAsync(FotoVeiculo foto, CancellationToken ct = default)
    {
        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        await DesmarcarPrincipaisAsync(foto.VeiculoId, ct);
        await _db.FotosVeiculo.Where(f => f.Id == foto.Id)
            .ExecuteUpdateAsync(s => s.SetProperty(f => f.Principal, true), ct);
        foto.Principal = true;
        await tx.CommitAsync(ct);
    }

    private Task DesmarcarPrincipaisAsync(int veiculoId, CancellationToken ct) =>
        _db.FotosVeiculo.Where(f => f.VeiculoId == veiculoId && f.Principal)
            .ExecuteUpdateAsync(s => s.SetProperty(f => f.Principal, false), ct);

    public Task<bool> VeiculoExisteAsync(int id, CancellationToken ct = default) =>
        _db.Veiculos.AnyAsync(v => v.Id == id, ct);

    public Task<bool> LojaExisteAsync(int lojaId, CancellationToken ct = default) =>
        _db.Lojas.AnyAsync(l => l.Id == lojaId, ct);

    public async Task<IReadOnlyList<AlteracaoVeiculo>> ListarHistoricoAsync(int veiculoId, CancellationToken ct = default) =>
        await _db.AlteracoesVeiculo.AsNoTracking().Where(a => a.VeiculoId == veiculoId)
            .OrderBy(a => a.Id).ToListAsync(ct);

    public async Task AdicionarAsync(Veiculo veiculo, CancellationToken ct = default) =>
        await _db.Veiculos.AddAsync(veiculo, ct);

    public async Task AdicionarAlteracaoAsync(AlteracaoVeiculo alteracao, CancellationToken ct = default) =>
        await _db.AlteracoesVeiculo.AddAsync(alteracao, ct);

    public Task SalvarAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
