using System.Data;
using Microsoft.EntityFrameworkCore;
using StarConcessionaria.Application.Common;
using StarConcessionaria.Application.Reservas;
using StarConcessionaria.Application.Veiculos;
using StarConcessionaria.Application.Vendas;
using StarConcessionaria.Domain.Entities;
using StarConcessionaria.Infrastructure.Persistence;

namespace StarConcessionaria.Infrastructure.Repositories;

public class VendaRepository : IVendaRepository
{
    private readonly ApplicationDbContext _db;

    public VendaRepository(ApplicationDbContext db) => _db = db;

    public async Task<Resultado<VendaResponse>> RegistrarVendaAsync(RegistrarVendaRequest r, UsuarioAtual vendedor, CancellationToken ct = default)
    {
        await using var tx = await _db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);

        // Bloqueio do veiculo
        var veiculo = await _db.Veiculos
            .FromSqlInterpolated($"SELECT * FROM veiculo WHERE id_vei = {r.VeiculoId} FOR UPDATE")
            .Include(v => v.Loja)
            .FirstOrDefaultAsync(ct);

        if (veiculo is null)
            return Resultado<VendaResponse>.Falha(TipoErro.NaoEncontrado, "Veiculo nao encontrado.");

        if (veiculo.Situacao == SituacaoVeiculo.Vendido)
            return Resultado<VendaResponse>.Falha(TipoErro.Conflito, "Veiculo ja foi vendido anteriormente.");

        var forma = await _db.FormasPagamento.AsNoTracking().FirstOrDefaultAsync(f => f.Id == r.FormaPagamentoId, ct);
        if (forma is null)
            return Resultado<VendaResponse>.Falha(TipoErro.Invalido, "Forma de pagamento nao existe.");

        var email = r.EmailCliente.Trim().ToLowerInvariant();
        var fone = r.TelefoneCliente.Trim();
        var cliente = await _db.Clientes.FirstOrDefaultAsync(c => c.Email.ToLower() == email && c.Telefone == fone, ct);
        if (cliente is null)
        {
            cliente = new Cliente { Nome = r.NomeCliente.Trim(), Email = email, Telefone = fone };
            _db.Clientes.Add(cliente);
            await _db.SaveChangesAsync(ct);
        }

        var agora = DateTimeOffset.UtcNow;
        var venda = new VendaVeiculo
        {
            VeiculoId = veiculo.Id,
            ClienteId = cliente.Id,
            VendedorId = vendedor.Id,
            LojaId = veiculo.LojaId,
            FormaPagamentoId = forma.Id,
            DataVenda = agora,
            ValorFinal = r.ValorFinal
        };

        veiculo.Situacao = SituacaoVeiculo.Vendido;

        // Cancela reservas pendentes/confirmadas ativas para este veiculo (RN02/TASK-14)
        var reservasAtivas = await _db.Reservas
            .Where(res => res.VeiculoId == veiculo.Id && (res.Situacao == SituacaoReserva.Pendente || res.Situacao == SituacaoReserva.Confirmada))
            .ToListAsync(ct);

        foreach (var res in reservasAtivas)
        {
            res.Situacao = SituacaoReserva.Cancelada;
        }

        _db.AlteracoesVeiculo.Add(new AlteracaoVeiculo
        {
            VeiculoId = veiculo.Id,
            Descricao = $"Venda concluida por {vendedor.Nome} (usuario {vendedor.Id}) no valor de R$ " + r.ValorFinal.ToString("F2") + ". Situacao alterada para 'Vendido'. " + reservasAtivas.Count.ToString() + " reserva(s) cancelada(s)."
        });

        _db.VendasVeiculo.Add(venda);
        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        var lojaNome = veiculo.Loja?.Nome ?? "Loja";
        return Resultado<VendaResponse>.Ok(new VendaResponse(
            venda.Id, veiculo.Id, $"{veiculo.Marca} {veiculo.Modelo} ({veiculo.Ano})",
            cliente.Id, cliente.Nome, vendedor.Id, vendedor.Nome, veiculo.LojaId,
            lojaNome, forma.Id, forma.Nome, venda.ValorFinal, agora));
    }

    public async Task<(IReadOnlyList<VendaResponse> Itens, int Total)> ListarAsync(VendaQuery f, CancellationToken ct = default)
    {
        var query = _db.VendasVeiculo.AsNoTracking()
            .Include(v => v.Veiculo).Include(v => v.Cliente).Include(v => v.Vendedor)
            .Include(v => v.Loja).Include(v => v.FormaPagamento)
            .AsQueryable();

        if (f.UnidadeId.HasValue) query = query.Where(v => v.LojaId == f.UnidadeId.Value);
        if (f.VendedorId.HasValue) query = query.Where(v => v.VendedorId == f.VendedorId.Value);
        if (f.PeriodoInicio.HasValue) query = query.Where(v => v.DataVenda >= f.PeriodoInicio.Value);
        if (f.PeriodoFim.HasValue) query = query.Where(v => v.DataVenda <= f.PeriodoFim.Value);

        var total = await query.CountAsync(ct);
        var itens = await query
            .OrderByDescending(v => v.DataVenda)
            .Skip((f.Pagina - 1) * f.ItensPorPagina)
            .Take(f.ItensPorPagina)
            .ToListAsync(ct);

        return (itens.Select(v => new VendaResponse(
            v.Id, v.VeiculoId, $"{v.Veiculo!.Marca} {v.Veiculo.Modelo} ({v.Veiculo.Ano})",
            v.ClienteId, v.Cliente!.Nome, v.VendedorId, v.Vendedor!.Nome,
            v.LojaId, v.Loja!.Nome, v.FormaPagamentoId, v.FormaPagamento!.Nome,
            v.ValorFinal, v.DataVenda ?? DateTimeOffset.UtcNow)).ToList(), total);
    }
}
