using System.Data;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using StarConcessionaria.Application.Agendamentos;
using StarConcessionaria.Application.Common;
using StarConcessionaria.Domain.Entities;
using StarConcessionaria.Infrastructure.Persistence;

namespace StarConcessionaria.Infrastructure.Repositories;

public class AgendamentoRepository : IAgendamentoRepository
{
    public const int CapacidadeMaximaLojaPorHorario = 5;
    private static readonly int[] HorasExpediente = { 9, 10, 11, 13, 14, 15, 16, 17 };

    private readonly ApplicationDbContext _db;

    public AgendamentoRepository(ApplicationDbContext db) => _db = db;

    public async Task<Resultado<IReadOnlyList<string>>> ObterHorariosDisponiveisAsync(int unidadeId, DateOnly data, int? veiculoId, CancellationToken ct = default)
    {
        if (!await _db.Lojas.AnyAsync(l => l.Id == unidadeId, ct))
            return Resultado<IReadOnlyList<string>>.Falha(TipoErro.NaoEncontrado, "Unidade informada nao existe.");

        if (veiculoId.HasValue)
        {
            var veiculo = await _db.Veiculos.AsNoTracking().FirstOrDefaultAsync(v => v.Id == veiculoId.Value, ct);
            if (veiculo is null)
                return Resultado<IReadOnlyList<string>>.Falha(TipoErro.NaoEncontrado, "Veiculo nao encontrado.");
            if (veiculo.LojaId != unidadeId)
                return Resultado<IReadOnlyList<string>>.Falha(TipoErro.Invalido, "O veiculo nao pertence a unidade informada.");
            if (veiculo.Situacao != SituacaoVeiculo.Disponivel)
                return Resultado<IReadOnlyList<string>>.Ok(Array.Empty<string>());
        }

        var inicioDia = new DateTimeOffset(data.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var fimDia = inicioDia.AddDays(1);

        var agendamentosDia = await _db.Agendamentos.AsNoTracking()
            .Where(a => a.LojaId == unidadeId
                        && a.Situacao != SituacaoAgendamento.Cancelado
                        && a.DataHoraInicio < fimDia
                        && a.DataHoraFim > inicioDia)
            .ToListAsync(ct);

        var disponiveis = new List<string>();
        foreach (var h in HorasExpediente)
        {
            var slotInicio = new DateTimeOffset(data.Year, data.Month, data.Day, h, 0, 0, TimeSpan.Zero);
            var slotFim = slotInicio.AddHours(1);

            var simultaneosLoja = agendamentosDia.Count(a => a.DataHoraInicio < slotFim && a.DataHoraFim > slotInicio);
            if (simultaneosLoja >= CapacidadeMaximaLojaPorHorario) continue;

            if (veiculoId.HasValue)
            {
                var conflitoVeiculo = agendamentosDia.Any(a => a.VeiculoId == veiculoId.Value && a.DataHoraInicio < slotFim && a.DataHoraFim > slotInicio);
                if (conflitoVeiculo) continue;
            }

            disponiveis.Add($"{h:D2}:00");
        }

        return Resultado<IReadOnlyList<string>>.Ok(disponiveis);
    }

    public async Task<Resultado<AgendamentoResponse>> AgendarVisitaAsync(AgendarVisitaRequest r, CancellationToken ct = default)
    {
        await using var tx = await _db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);

        var loja = await _db.Lojas.FirstOrDefaultAsync(l => l.Id == r.UnidadeId, ct);
        if (loja is null)
            return Resultado<AgendamentoResponse>.Falha(TipoErro.NaoEncontrado, "Unidade informada nao existe.");

        var inicio = r.DataHoraInicio.ToUniversalTime();
        var fim = inicio.AddHours(1);

        // RN08: Capacidade maxima da unidade fisica no mesmo intervalo
        var simultaneos = await _db.Agendamentos.CountAsync(
            a => a.LojaId == loja.Id
                 && a.Situacao != SituacaoAgendamento.Cancelado
                 && a.DataHoraInicio < fim
                 && a.DataHoraFim > inicio, ct);

        if (simultaneos >= CapacidadeMaximaLojaPorHorario)
            return Resultado<AgendamentoResponse>.Falha(TipoErro.Conflito, "Capacidade maxima de agendamentos simultaneos da unidade atingida para este horario.");

        var cliente = await ObterOuCriarClienteAsync(r.NomeCliente, r.EmailCliente, r.TelefoneCliente, ct);
        var agendamento = new Agendamento
        {
            ClienteId = cliente.Id,
            LojaId = loja.Id,
            VeiculoId = null,
            Tipo = TipoAgendamento.Visita,
            DataHoraInicio = inicio,
            DataHoraFim = fim,
            Situacao = SituacaoAgendamento.Solicitado
        };

        _db.Agendamentos.Add(agendamento);
        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return Resultado<AgendamentoResponse>.Ok(Mapear(agendamento, loja, null, cliente));
    }

    public async Task<Resultado<AgendamentoResponse>> AgendarTestDriveAsync(AgendarTestDriveRequest r, CancellationToken ct = default)
    {
        await using var tx = await _db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);

        var loja = await _db.Lojas.AsNoTracking().FirstOrDefaultAsync(l => l.Id == r.UnidadeId, ct);
        if (loja is null)
            return Resultado<AgendamentoResponse>.Falha(TipoErro.NaoEncontrado, "Unidade informada nao existe.");

        // Bloqueia a linha do veiculo para evitar race condition de test-drives simultaneos no mesmo carro
        var veiculo = await _db.Veiculos
            .FromSqlInterpolated($"SELECT * FROM veiculo WHERE id_vei = {r.VeiculoId} FOR UPDATE")
            .FirstOrDefaultAsync(ct);

        if (veiculo is null)
            return Resultado<AgendamentoResponse>.Falha(TipoErro.NaoEncontrado, "Veiculo nao encontrado.");

        if (veiculo.LojaId != loja.Id)
            return Resultado<AgendamentoResponse>.Falha(TipoErro.Invalido, "O veiculo selecionado nao pertence a unidade informada.");

        // RN02 & RN04: Veiculo vendido ou reservado nao pode receber test-drive
        if (veiculo.Situacao == SituacaoVeiculo.Vendido)
            return Resultado<AgendamentoResponse>.Falha(TipoErro.Conflito, "Veiculo ja comercializado. Test-drives e reservas estao desabilitados.");

        if (veiculo.Situacao != SituacaoVeiculo.Disponivel)
            return Resultado<AgendamentoResponse>.Falha(TipoErro.Conflito, "O veiculo nao esta disponivel para test-drive no momento.");

        var inicio = r.DataHoraInicio.ToUniversalTime();
        var fim = inicio.AddHours(1);

        // RN04: Conflito de horario para o mesmo veiculo (relacao 1:1 no intervalo)
        var conflitoVeiculo = await _db.Agendamentos.AnyAsync(
            a => a.VeiculoId == veiculo.Id
                 && a.Situacao != SituacaoAgendamento.Cancelado
                 && a.DataHoraInicio < fim
                 && a.DataHoraFim > inicio, ct);

        if (conflitoVeiculo)
            return Resultado<AgendamentoResponse>.Falha(TipoErro.Conflito, "Ja existe um agendamento de test-drive para este veiculo no horario selecionado.");

        // RN08: Capacidade maxima da loja no horario
        var simultaneosLoja = await _db.Agendamentos.CountAsync(
            a => a.LojaId == loja.Id
                 && a.Situacao != SituacaoAgendamento.Cancelado
                 && a.DataHoraInicio < fim
                 && a.DataHoraFim > inicio, ct);

        if (simultaneosLoja >= CapacidadeMaximaLojaPorHorario)
            return Resultado<AgendamentoResponse>.Falha(TipoErro.Conflito, "Capacidade maxima de atendimento da unidade atingida para este horario.");

        var cliente = await ObterOuCriarClienteAsync(r.NomeCliente, r.EmailCliente, r.TelefoneCliente, ct);
        var agendamento = new Agendamento
        {
            ClienteId = cliente.Id,
            LojaId = loja.Id,
            VeiculoId = veiculo.Id,
            Tipo = TipoAgendamento.TestDrive,
            DataHoraInicio = inicio,
            DataHoraFim = fim,
            Situacao = SituacaoAgendamento.Solicitado
        };

        _db.Agendamentos.Add(agendamento);
        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return Resultado<AgendamentoResponse>.Ok(Mapear(agendamento, loja, veiculo, cliente));
    }

    public async Task<(IReadOnlyList<AgendamentoResponse> Itens, int Total)> ListarAsync(AgendamentoQuery f, CancellationToken ct = default)
    {
        var query = _db.Agendamentos.AsNoTracking()
            .Include(a => a.Loja).Include(a => a.Veiculo).Include(a => a.Cliente)
            .AsQueryable();

        if (f.UnidadeId.HasValue)
            query = query.Where(a => a.LojaId == f.UnidadeId.Value);
        if (!string.IsNullOrWhiteSpace(f.Status))
            query = query.Where(a => a.Situacao.ToLower() == f.Status.ToLower());
        if (!string.IsNullOrWhiteSpace(f.Tipo))
            query = query.Where(a => a.Tipo.ToLower() == f.Tipo.ToLower());
        if (f.Data.HasValue)
        {
            var inicioDia = new DateTimeOffset(f.Data.Value.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
            var fimDia = inicioDia.AddDays(1);
            query = query.Where(a => a.DataHoraInicio >= inicioDia && a.DataHoraInicio < fimDia);
        }

        var total = await query.CountAsync(ct);
        var itens = await query
            .OrderByDescending(a => a.DataHoraInicio)
            .Skip((f.Pagina - 1) * f.ItensPorPagina)
            .Take(f.ItensPorPagina)
            .ToListAsync(ct);

        return (itens.Select(a => Mapear(a, a.Loja!, a.Veiculo, a.Cliente!)).ToList(), total);
    }

    public async Task<Resultado<AgendamentoResponse>> AtualizarStatusAsync(int id, string novoStatus, CancellationToken ct = default)
    {
        var agendamento = await _db.Agendamentos
            .Include(a => a.Loja).Include(a => a.Veiculo).Include(a => a.Cliente)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        if (agendamento is null)
            return Resultado<AgendamentoResponse>.Falha(TipoErro.NaoEncontrado, "Agendamento nao encontrado.");

        if (agendamento.Situacao == SituacaoAgendamento.Cancelado || agendamento.Situacao == SituacaoAgendamento.Realizado)
            return Resultado<AgendamentoResponse>.Falha(TipoErro.Conflito, $"Agendamento ja esta {agendamento.Situacao} e nao pode ser alterado.");

        agendamento.Situacao = novoStatus;
        await _db.SaveChangesAsync(ct);

        return Resultado<AgendamentoResponse>.Ok(Mapear(agendamento, agendamento.Loja!, agendamento.Veiculo, agendamento.Cliente!));
    }

    private async Task<Cliente> ObterOuCriarClienteAsync(string nome, string emailRaw, string telefoneRaw, CancellationToken ct)
    {
        var email = emailRaw.Trim().ToLowerInvariant();
        var telefone = telefoneRaw.Trim();
        var cliente = await _db.Clientes.FirstOrDefaultAsync(c => c.Email.ToLower() == email && c.Telefone == telefone, ct);
        if (cliente is not null) return cliente;

        cliente = new Cliente { Nome = nome.Trim(), Email = email, Telefone = telefone };
        _db.Clientes.Add(cliente);
        await _db.SaveChangesAsync(ct);
        return cliente;
    }

    private static AgendamentoResponse Mapear(Agendamento a, Loja l, Veiculo? v, Cliente c)
    {
        var codigo = $"AGE-{a.DataHoraInicio.Year}-{a.Id.ToString("D6", CultureInfo.InvariantCulture)}";
        var vDesc = v is null ? null : $"{v.Marca} {v.Modelo} ({v.Ano})";
        return new AgendamentoResponse(
            a.Id, codigo, a.Tipo, l.Id, l.Nome, v?.Id, vDesc,
            c.Id, c.Nome, c.Email, c.Telefone, a.DataHoraInicio, a.DataHoraFim, a.Situacao);
    }
}
