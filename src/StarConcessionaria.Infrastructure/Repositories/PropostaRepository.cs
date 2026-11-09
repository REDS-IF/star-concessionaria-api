using Microsoft.EntityFrameworkCore;
using StarConcessionaria.Application.Propostas;
using StarConcessionaria.Domain.Entities;
using StarConcessionaria.Infrastructure.Persistence;

namespace StarConcessionaria.Infrastructure.Repositories;

public class PropostaRepository : IPropostaRepository
{
    private readonly ApplicationDbContext _db;

    public PropostaRepository(ApplicationDbContext db) => _db = db;

    public Task<Veiculo?> ObterVeiculoAsync(int veiculoId, CancellationToken ct = default) =>
        _db.Veiculos.AsNoTracking().FirstOrDefaultAsync(v => v.Id == veiculoId, ct);

    public Task<bool> LojaExisteAsync(int lojaId, CancellationToken ct = default) =>
        _db.Lojas.AnyAsync(l => l.Id == lojaId, ct);

    public Task<FormaPagamento?> ObterFormaPagamentoAsync(string nome, CancellationToken ct = default) =>
        _db.FormasPagamento.AsNoTracking().FirstOrDefaultAsync(f => f.Nome.ToLower() == nome.ToLower(), ct);

    public Task<Cliente?> ObterClienteAsync(string email, string telefone, CancellationToken ct = default) =>
        _db.Clientes.FirstOrDefaultAsync(c => c.Email.ToLower() == email && c.Telefone == telefone, ct);

    public async Task AdicionarContatoAsync(Contato contato, CancellationToken ct = default) =>
        await _db.Contatos.AddAsync(contato, ct);

    public Task SalvarAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
