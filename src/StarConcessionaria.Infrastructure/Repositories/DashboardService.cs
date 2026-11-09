using Microsoft.EntityFrameworkCore;
using StarConcessionaria.Application.Agendamentos;
using StarConcessionaria.Application.Dashboard;
using StarConcessionaria.Application.Reservas;
using StarConcessionaria.Domain.Entities;
using StarConcessionaria.Infrastructure.Persistence;

namespace StarConcessionaria.Infrastructure.Repositories;

public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _db;

    public DashboardService(ApplicationDbContext db) => _db = db;

    public async Task<IndicadoresGerenciaisResponse> ObterIndicadoresAsync(int? unidadeId, CancellationToken ct = default)
    {
        var veiculosQ = _db.Veiculos.AsNoTracking().AsQueryable();
        var reservasQ = _db.Reservas.AsNoTracking().AsQueryable();
        var agendamentosQ = _db.Agendamentos.AsNoTracking().AsQueryable();
        var contatosQ = _db.Contatos.AsNoTracking().AsQueryable();
        var vendasQ = _db.VendasVeiculo.AsNoTracking().AsQueryable();

        if (unidadeId.HasValue)
        {
            veiculosQ = veiculosQ.Where(v => v.LojaId == unidadeId.Value);
            reservasQ = reservasQ.Where(r => r.Veiculo != null && r.Veiculo.LojaId == unidadeId.Value);
            agendamentosQ = agendamentosQ.Where(a => a.LojaId == unidadeId.Value);
            contatosQ = contatosQ.Where(c => c.LojaId == unidadeId.Value);
            vendasQ = vendasQ.Where(v => v.LojaId == unidadeId.Value);
        }

        var disponiveis = await veiculosQ.CountAsync(v => v.Situacao == SituacaoVeiculo.Disponivel, ct);
        var reservados = await veiculosQ.CountAsync(v => v.Situacao == SituacaoVeiculo.Reservado, ct);
        var vendidos = await veiculosQ.CountAsync(v => v.Situacao == SituacaoVeiculo.Vendido, ct);

        var reservasAtivas = await reservasQ.CountAsync(
            r => r.Situacao == SituacaoReserva.Pendente || r.Situacao == SituacaoReserva.Confirmada, ct);

        var agendamentosPendentes = await agendamentosQ.CountAsync(
            a => a.Situacao == SituacaoAgendamento.Solicitado, ct);

        var contatosNaoRespondidos = await contatosQ.CountAsync(c => c.UsuarioId == null, ct);

        var totalVendas = await vendasQ.CountAsync(ct);
        var valorTotalVendas = await vendasQ.SumAsync(v => (decimal?)v.ValorFinal, ct) ?? 0m;

        var lojas = await _db.Lojas.AsNoTracking()
            .Where(l => !unidadeId.HasValue || l.Id == unidadeId.Value)
            .Include(l => l.Veiculos)
            .ToListAsync(ct);

        var porLoja = lojas.Select(l => new VeiculosPorLojaDto(
            l.Id, l.Nome,
            l.Veiculos.Count(v => v.Situacao == SituacaoVeiculo.Disponivel))).ToList();

        var vendasPecas = await _db.VendasPeca.AsNoTracking()
            .Include(vp => vp.Peca)
            .ToListAsync(ct);

        var topPecas = vendasPecas
            .GroupBy(vp => new { vp.PecaId, Nome = vp.Peca?.Nome ?? "Peça" })
            .Select(g => new PecaMaisVendidaDto(
                g.Key.PecaId,
                g.Key.Nome,
                g.Sum(x => x.Quantidade),
                g.Sum(x => x.Quantidade * x.ValorUnitario)))
            .OrderByDescending(x => x.QuantidadeVendida)
            .Take(5)
            .ToList();

        return new IndicadoresGerenciaisResponse(
            disponiveis, reservados, vendidos, reservasAtivas, agendamentosPendentes,
            contatosNaoRespondidos, totalVendas, valorTotalVendas, porLoja, topPecas);
    }
}
