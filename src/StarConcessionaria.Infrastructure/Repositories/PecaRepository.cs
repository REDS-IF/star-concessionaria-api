using System.Data;
using Microsoft.EntityFrameworkCore;
using StarConcessionaria.Application.Common;
using StarConcessionaria.Application.Pecas;
using StarConcessionaria.Domain.Entities;
using StarConcessionaria.Infrastructure.Persistence;

namespace StarConcessionaria.Infrastructure.Repositories;

public class PecaRepository : IPecaRepository
{
    private readonly ApplicationDbContext _db;

    public PecaRepository(ApplicationDbContext db) => _db = db;

    public async Task<(IReadOnlyList<Peca> Itens, int Total)> ListarAsync(PecaQuery f, CancellationToken ct = default)
    {
        var query = _db.Pecas.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(f.Busca))
        {
            var padrao = "%" + f.Busca.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_") + "%";
            query = query.Where(p => EF.Functions.ILike(p.Nome, padrao) || (p.Descricao != null && EF.Functions.ILike(p.Descricao, padrao)));
        }

        if (f.Disponivel.HasValue)
        {
            query = f.Disponivel.Value
                ? query.Where(p => p.QuantidadeEstoque > 0)
                : query.Where(p => p.QuantidadeEstoque <= 0);
        }

        var total = await query.CountAsync(ct);
        var itens = await query
            .OrderBy(p => p.Nome)
            .Skip((f.Pagina - 1) * f.ItensPorPagina)
            .Take(f.ItensPorPagina)
            .ToListAsync(ct);

        return (itens, total);
    }

    public Task<Peca?> ObterPorIdAsync(int id, CancellationToken ct = default) =>
        _db.Pecas.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<Peca?> ObterParaAtualizacaoAsync(int id, CancellationToken ct = default) =>
        _db.Pecas.FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task AdicionarAsync(Peca peca, CancellationToken ct = default) =>
        await _db.Pecas.AddAsync(peca, ct);

    public Task SalvarAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);

    public async Task<Resultado<VendaPecaResponse>> RegistrarVendaAsync(int pecaId, RegistrarVendaPecaRequest r, CancellationToken ct = default)
    {
        await using var tx = await _db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);

        var peca = await _db.Pecas
            .FromSqlInterpolated($"SELECT * FROM peca WHERE id_pec = {pecaId} FOR UPDATE")
            .FirstOrDefaultAsync(ct);

        if (peca is null)
            return Resultado<VendaPecaResponse>.Falha(TipoErro.NaoEncontrado, "Peca nao encontrada.");

        if (peca.QuantidadeEstoque < r.Quantidade)
            return Resultado<VendaPecaResponse>.Falha(TipoErro.Conflito, $"Estoque insuficiente. Disponivel: {peca.QuantidadeEstoque}.");

        int? clienteId = null;
        if (!string.IsNullOrWhiteSpace(r.EmailCliente) && !string.IsNullOrWhiteSpace(r.TelefoneCliente))
        {
            var email = r.EmailCliente.Trim().ToLowerInvariant();
            var fone = r.TelefoneCliente.Trim();
            var cliente = await _db.Clientes.FirstOrDefaultAsync(c => c.Email.ToLower() == email && c.Telefone == fone, ct);
            if (cliente is null)
            {
                cliente = new Cliente { Nome = string.IsNullOrWhiteSpace(r.NomeCliente) ? "Cliente Balcao" : r.NomeCliente.Trim(), Email = email, Telefone = fone };
                _db.Clientes.Add(cliente);
                await _db.SaveChangesAsync(ct);
            }
            clienteId = cliente.Id;
        }

        peca.QuantidadeEstoque -= r.Quantidade;
        var agora = DateTimeOffset.UtcNow;
        var venda = new VendaPeca
        {
            PecaId = peca.Id,
            ClienteId = clienteId,
            Quantidade = r.Quantidade,
            ValorUnitario = peca.Preco,
            DataVenda = agora
        };

        _db.VendasPeca.Add(venda);
        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return Resultado<VendaPecaResponse>.Ok(new VendaPecaResponse(
            venda.Id, peca.Id, peca.Nome, clienteId, venda.Quantidade,
            venda.ValorUnitario, venda.ValorUnitario * venda.Quantidade, agora, peca.QuantidadeEstoque));
    }
}
