using Microsoft.EntityFrameworkCore;
using StarConcessionaria.Application.Unidades;
using StarConcessionaria.Domain.Entities;
using StarConcessionaria.Infrastructure.Persistence;

namespace StarConcessionaria.Infrastructure.Repositories;

public class LojaRepository : ILojaRepository
{
    private readonly ApplicationDbContext _db;

    public LojaRepository(ApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<Loja>> ListarAsync(string? cidade, string? estado, CancellationToken ct = default)
    {
        var query = _db.Lojas.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(cidade))
            query = query.Where(l => l.Cidade != null && l.Cidade.ToLower() == cidade.ToLower());
        if (!string.IsNullOrWhiteSpace(estado))
            query = query.Where(l => l.Estado != null && l.Estado.ToLower() == estado.ToLower());
        return await query.OrderBy(l => l.Nome).ToListAsync(ct);
    }

    public Task<Loja?> ObterPorIdAsync(int id, CancellationToken ct = default) =>
        _db.Lojas.AsNoTracking().FirstOrDefaultAsync(l => l.Id == id, ct);

    public Task<Loja?> ObterParaAtualizacaoAsync(int id, CancellationToken ct = default) =>
        _db.Lojas.FirstOrDefaultAsync(l => l.Id == id, ct);

    public async Task AdicionarAsync(Loja loja, CancellationToken ct = default) =>
        await _db.Lojas.AddAsync(loja, ct);

    public Task SalvarAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
