using System.Data;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using StarConcessionaria.Application.Common;
using StarConcessionaria.Application.Reservas;
using StarConcessionaria.Application.Veiculos;
using StarConcessionaria.Domain.Entities;
using StarConcessionaria.Infrastructure.Persistence;

namespace StarConcessionaria.Infrastructure.Repositories;

public class ReservaRepository : IReservaRepository
{
    private readonly ApplicationDbContext _db;

    public ReservaRepository(ApplicationDbContext db) => _db = db;

    public async Task<Resultado<ReservaResponse>> SolicitarComBloqueioAtomicoAsync(SolicitarReservaRequest r, CancellationToken ct = default)
    {
        await using var tx = await _db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);

        // RN01 & RN03: Bloqueio pessimista (SELECT ... FOR UPDATE) garante exclusao mutua em requisicoes simultaneas.
        var veiculo = await _db.Veiculos
            .FromSqlInterpolated($"SELECT * FROM veiculo WHERE id_vei = {r.VeiculoId} FOR UPDATE")
            .FirstOrDefaultAsync(ct);

        if (veiculo is null)
            return Resultado<ReservaResponse>.Falha(TipoErro.NaoEncontrado, "Veiculo nao encontrado.");

        if (veiculo.Situacao != SituacaoVeiculo.Disponivel)
            return Resultado<ReservaResponse>.Falha(TipoErro.Conflito, "O veiculo selecionado nao esta disponivel para reserva no momento.");

        var possuiAtiva = await _db.Reservas.AnyAsync(
            x => x.VeiculoId == veiculo.Id && (x.Situacao == SituacaoReserva.Pendente || x.Situacao == SituacaoReserva.Confirmada), ct);
        if (possuiAtiva)
            return Resultado<ReservaResponse>.Falha(TipoErro.Conflito, "Ja existe uma reserva ativa para este veiculo.");

        var email = r.EmailCliente.Trim().ToLowerInvariant();
        var telefone = r.TelefoneCliente.Trim();
        var cliente = await _db.Clientes.FirstOrDefaultAsync(c => c.Email.ToLower() == email && c.Telefone == telefone, ct);
        if (cliente is null)
        {
            cliente = new Cliente { Nome = r.NomeCliente.Trim(), Email = email, Telefone = telefone };
            _db.Clientes.Add(cliente);
            await _db.SaveChangesAsync(ct);
        }

        var agora = DateTimeOffset.UtcNow;
        var reserva = new Reserva
        {
            ClienteId = cliente.Id,
            VeiculoId = veiculo.Id,
            DataSolicitacao = agora,
            Situacao = SituacaoReserva.Pendente
        };

        veiculo.Situacao = SituacaoVeiculo.Reservado;
        _db.Reservas.Add(reserva);
        _db.AlteracoesVeiculo.Add(new AlteracaoVeiculo
        {
            VeiculoId = veiculo.Id,
            Descricao = $"Situacao alterada automaticamente para 'Reservado' devido a solicitacao de reserva do cliente {cliente.Nome}."
        });

        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return Resultado<ReservaResponse>.Ok(Mapear(reserva, veiculo, cliente));
    }

    public async Task<ReservaResponse?> ObterPorIdAsync(int id, CancellationToken ct = default)
    {
        var r = await _db.Reservas.AsNoTracking()
            .Include(x => x.Veiculo).Include(x => x.Cliente)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        return r is null ? null : Mapear(r, r.Veiculo!, r.Cliente!);
    }

    public async Task<(IReadOnlyList<ReservaResponse> Itens, int Total)> ListarAsync(ReservaQuery f, CancellationToken ct = default)
    {
        var query = _db.Reservas.AsNoTracking().Include(x => x.Veiculo).Include(x => x.Cliente).AsQueryable();

        if (!string.IsNullOrWhiteSpace(f.Status))
            query = query.Where(x => x.Situacao.ToLower() == f.Status.ToLower());
        if (f.VeiculoId.HasValue)
            query = query.Where(x => x.VeiculoId == f.VeiculoId.Value);
        if (f.DataInicio.HasValue)
            query = query.Where(x => x.DataSolicitacao >= f.DataInicio.Value);

        var total = await query.CountAsync(ct);
        var itens = await query
            .OrderByDescending(x => x.Id)
            .Skip((f.Pagina - 1) * f.ItensPorPagina)
            .Take(f.ItensPorPagina)
            .ToListAsync(ct);

        return (itens.Select(r => Mapear(r, r.Veiculo!, r.Cliente!)).ToList(), total);
    }

    public async Task<Resultado<ReservaResponse>> AtualizarStatusAsync(int id, string novoStatus, string? motivo, UsuarioAtual usuario, CancellationToken ct = default)
    {
        await using var tx = await _db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);

        var reserva = await _db.Reservas.Include(x => x.Cliente).FirstOrDefaultAsync(x => x.Id == id, ct);
        if (reserva is null)
            return Resultado<ReservaResponse>.Falha(TipoErro.NaoEncontrado, "Reserva nao encontrada.");

        if (reserva.Situacao == SituacaoReserva.Cancelada || reserva.Situacao == SituacaoReserva.Concluida)
            return Resultado<ReservaResponse>.Falha(TipoErro.Conflito, $"Reserva ja esta {reserva.Situacao} e nao pode ser alterada.");

        var veiculo = await _db.Veiculos
            .FromSqlInterpolated($"SELECT * FROM veiculo WHERE id_vei = {reserva.VeiculoId} FOR UPDATE")
            .FirstAsync(ct);

        reserva.Situacao = novoStatus;
        var complemento = string.IsNullOrWhiteSpace(motivo) ? "" : $" Motivo: {motivo.Trim()}";

        if (novoStatus == SituacaoReserva.Cancelada && veiculo.Situacao == SituacaoVeiculo.Reservado)
        {
            veiculo.Situacao = SituacaoVeiculo.Disponivel;
            _db.AlteracoesVeiculo.Add(new AlteracaoVeiculo
            {
                VeiculoId = veiculo.Id,
                Descricao = $"Reserva #{reserva.Id} cancelada por {usuario.Nome} (usuario {usuario.Id}). Veiculo voltou para 'Disponivel'.{complemento}"
            });
        }
        else if (novoStatus == SituacaoReserva.Concluida && veiculo.Situacao != SituacaoVeiculo.Vendido)
        {
            veiculo.Situacao = SituacaoVeiculo.Vendido;
            _db.AlteracoesVeiculo.Add(new AlteracaoVeiculo
            {
                VeiculoId = veiculo.Id,
                Descricao = $"Reserva #{reserva.Id} concluida por {usuario.Nome} (usuario {usuario.Id}). Veiculo marcado como 'Vendido'.{complemento}"
            });
        }

        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return Resultado<ReservaResponse>.Ok(Mapear(reserva, veiculo, reserva.Cliente!));
    }

    private static ReservaResponse Mapear(Reserva r, Veiculo v, Cliente c)
    {
        var data = r.DataSolicitacao ?? DateTimeOffset.UtcNow;
        var codigo = $"RES-{data.Year}-{r.Id.ToString("D6", CultureInfo.InvariantCulture)}";
        return new ReservaResponse(
            r.Id, codigo, v.Id, $"{v.Marca} {v.Modelo} ({v.Ano})", c.Id,
            c.Nome, c.Email, c.Telefone, data, r.Situacao);
    }
}
