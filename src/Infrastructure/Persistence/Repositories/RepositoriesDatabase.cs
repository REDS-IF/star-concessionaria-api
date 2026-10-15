using System.Collections.Concurrent;
using StarConcessionaria.Domain.Entities;
using StarConcessionaria.Domain.Enums;
using StarConcessionaria.Domain.Repositories;
using StarConcessionaria.Infrastructure.Persistence.Mappers;
using StarConcessionaria.Infrastructure.Persistence.Models;

namespace StarConcessionaria.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementação de persistência para Veículos com isolamento total via Mappers.
/// Os dados são armazenados na forma de modelos de persistência (VeiculoModel).
/// </summary>
public class VeiculoRepositoryDatabase : IVeiculoRepository
{
    private static readonly ConcurrentDictionary<Guid, VeiculoModel> _storage = new();
    private static readonly object _syncLock = new();

    public Task<Veiculo?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (_storage.TryGetValue(id, out var model))
        {
            return Task.FromResult<Veiculo?>(model.ToDomain());
        }

        return Task.FromResult<Veiculo?>(null);
    }

    public Task<IEnumerable<Veiculo>> ListarAsync(
        string? marca = null,
        string? modelo = null,
        int? anoMin = null,
        int? anoMax = null,
        decimal? precoMin = null,
        decimal? precoMax = null,
        SituacaoVeiculo? situacao = null,
        Guid? unidadeId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _storage.Values.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(marca))
            query = query.Where(v => v.Marca.Contains(marca, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(modelo))
            query = query.Where(v => v.Modelo.Contains(modelo, StringComparison.OrdinalIgnoreCase));

        if (anoMin.HasValue)
            query = query.Where(v => v.Ano >= anoMin.Value);

        if (anoMax.HasValue)
            query = query.Where(v => v.Ano <= anoMax.Value);

        if (precoMin.HasValue)
            query = query.Where(v => v.Preco >= precoMin.Value);

        if (precoMax.HasValue)
            query = query.Where(v => v.Preco <= precoMax.Value);

        if (situacao.HasValue)
            query = query.Where(v => v.Situacao == (int)situacao.Value);

        if (unidadeId.HasValue && unidadeId.Value != Guid.Empty)
            query = query.Where(v => v.UnidadeId == unidadeId.Value);

        var domainEntities = query.Select(m => m.ToDomain()).ToList();
        return Task.FromResult<IEnumerable<Veiculo>>(domainEntities);
    }

    public Task AdicionarAsync(Veiculo veiculo, CancellationToken cancellationToken = default)
    {
        lock (_syncLock)
        {
            var model = veiculo.ToModel();
            _storage[veiculo.Id] = model;
        }
        return Task.CompletedTask;
    }

    public Task AtualizarAsync(Veiculo veiculo, CancellationToken cancellationToken = default)
    {
        lock (_syncLock)
        {
            var model = veiculo.ToModel();
            _storage[veiculo.Id] = model;
        }
        return Task.CompletedTask;
    }

    public Task<bool> ExisteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_storage.ContainsKey(id));
    }
}

/// <summary>
/// Repositório de persistência de Lojas Físicas / Unidades.
/// </summary>
public class UnidadeRepositoryDatabase : IUnidadeRepository
{
    private static readonly ConcurrentDictionary<Guid, UnidadeModel> _storage = new();

    public Task<Unidade?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_storage.TryGetValue(id, out var model) ? model.ToDomain() : null);
    }

    public Task<IEnumerable<Unidade>> ListarTodasAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IEnumerable<Unidade>>(_storage.Values.Select(m => m.ToDomain()).ToList());
    }

    public Task AdicionarAsync(Unidade unidade, CancellationToken cancellationToken = default)
    {
        _storage[unidade.Id] = unidade.ToModel();
        return Task.CompletedTask;
    }

    public Task AtualizarAsync(Unidade unidade, CancellationToken cancellationToken = default)
    {
        _storage[unidade.Id] = unidade.ToModel();
        return Task.CompletedTask;
    }

    public Task<bool> ExisteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_storage.ContainsKey(id));
    }
}

/// <summary>
/// Repositório de persistência de Reservas com validação atômica de reservas ativas (RN01, RN03, RN05).
/// </summary>
public class ReservaRepositoryDatabase : IReservaRepository
{
    private static readonly ConcurrentDictionary<Guid, ReservaModel> _storage = new();
    private static readonly object _syncLock = new();

    public Task<Reserva?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_storage.TryGetValue(id, out var model) ? model.ToDomain() : null);
    }

    public Task<IEnumerable<Reserva>> ListarTodasAsync(
        Guid? veiculoId = null, 
        StatusReserva? status = null, 
        CancellationToken cancellationToken = default)
    {
        var query = _storage.Values.AsEnumerable();

        if (veiculoId.HasValue && veiculoId.Value != Guid.Empty)
            query = query.Where(r => r.VeiculoId == veiculoId.Value);

        if (status.HasValue)
            query = query.Where(r => r.Status == (int)status.Value);

        return Task.FromResult<IEnumerable<Reserva>>(query.Select(m => m.ToDomain()).ToList());
    }

    public Task<bool> ExisteReservaAtivaParaVeiculoAsync(Guid veiculoId, CancellationToken cancellationToken = default)
    {
        lock (_syncLock)
        {
            var temAtiva = _storage.Values.Any(r => 
                r.VeiculoId == veiculoId && 
                (r.Status == (int)StatusReserva.PENDENTE || r.Status == (int)StatusReserva.CONFIRMADA));

            return Task.FromResult(temAtiva);
        }
    }

    public Task AdicionarAsync(Reserva reserva, CancellationToken cancellationToken = default)
    {
        lock (_syncLock)
        {
            _storage[reserva.Id] = reserva.ToModel();
        }
        return Task.CompletedTask;
    }

    public Task AtualizarAsync(Reserva reserva, CancellationToken cancellationToken = default)
    {
        lock (_syncLock)
        {
            _storage[reserva.Id] = reserva.ToModel();
        }
        return Task.CompletedTask;
    }
}

/// <summary>
/// Repositório de persistência de Agendamentos (RN04/RN08).
/// </summary>
public class AgendamentoRepositoryDatabase : IAgendamentoRepository
{
    private static readonly ConcurrentDictionary<Guid, AgendamentoModel> _storage = new();
    private static readonly object _syncLock = new();

    public Task<Agendamento?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_storage.TryGetValue(id, out var model) ? model.ToDomain() : null);
    }

    public Task<IEnumerable<Agendamento>> ListarTodosAsync(
        Guid? unidadeId = null, 
        Guid? veiculoId = null, 
        StatusAgendamento? status = null, 
        CancellationToken cancellationToken = default)
    {
        var query = _storage.Values.AsEnumerable();

        if (unidadeId.HasValue && unidadeId.Value != Guid.Empty)
            query = query.Where(a => a.UnidadeId == unidadeId.Value);

        if (veiculoId.HasValue && veiculoId.Value != Guid.Empty)
            query = query.Where(a => a.VeiculoId == veiculoId.Value);

        if (status.HasValue)
            query = query.Where(a => a.Status == (int)status.Value);

        return Task.FromResult<IEnumerable<Agendamento>>(query.Select(m => m.ToDomain()).ToList());
    }

    public Task<bool> ExisteConflitoHorarioAsync(
        Guid unidadeId, 
        Guid? veiculoId, 
        DateTime dataHora, 
        int duracaoMinutos = 60, 
        CancellationToken cancellationToken = default)
    {
        lock (_syncLock)
        {
            var inicioJanela = dataHora.AddMinutes(-duracaoMinutos);
            var fimJanela = dataHora.AddMinutes(duracaoMinutos);

            var conflito = _storage.Values.Any(a =>
                a.Status == (int)StatusAgendamento.AGENDADO &&
                a.DataHora > inicioJanela && a.DataHora < fimJanela &&
                (
                    (veiculoId.HasValue && a.VeiculoId == veiculoId.Value) || 
                    (a.UnidadeId == unidadeId && a.DataHora == dataHora)
                ));

            return Task.FromResult(conflito);
        }
    }

    public Task AdicionarAsync(Agendamento agendamento, CancellationToken cancellationToken = default)
    {
        lock (_syncLock)
        {
            _storage[agendamento.Id] = agendamento.ToModel();
        }
        return Task.CompletedTask;
    }

    public Task AtualizarAsync(Agendamento agendamento, CancellationToken cancellationToken = default)
    {
        lock (_syncLock)
        {
            _storage[agendamento.Id] = agendamento.ToModel();
        }
        return Task.CompletedTask;
    }
}

/// <summary>
/// Repositório de persistência de Propostas e Contatos Comerciais (RN06).
/// </summary>
public class PropostaContatoRepositoryDatabase : IPropostaContatoRepository
{
    private static readonly ConcurrentDictionary<Guid, PropostaContatoModel> _storage = new();

    public Task<PropostaContato?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_storage.TryGetValue(id, out var model) ? model.ToDomain() : null);
    }

    public Task<IEnumerable<PropostaContato>> ListarAsync(
        TipoPropostaContato? tipo = null, 
        StatusAtendimento? status = null, 
        Guid? veiculoId = null, 
        CancellationToken cancellationToken = default)
    {
        var query = _storage.Values.AsEnumerable();

        if (tipo.HasValue)
            query = query.Where(p => p.Tipo == (int)tipo.Value);

        if (status.HasValue)
            query = query.Where(p => p.StatusAtendimento == (int)status.Value);

        if (veiculoId.HasValue && veiculoId.Value != Guid.Empty)
            query = query.Where(p => p.VeiculoId == veiculoId.Value);

        return Task.FromResult<IEnumerable<PropostaContato>>(query.Select(m => m.ToDomain()).ToList());
    }

    public Task AdicionarAsync(PropostaContato propostaContato, CancellationToken cancellationToken = default)
    {
        _storage[propostaContato.Id] = propostaContato.ToModel();
        return Task.CompletedTask;
    }

    public Task AtualizarAsync(PropostaContato propostaContato, CancellationToken cancellationToken = default)
    {
        _storage[propostaContato.Id] = propostaContato.ToModel();
        return Task.CompletedTask;
    }
}

/// <summary>
/// Repositório de persistência de Peças e Acessórios.
/// </summary>
public class PecaRepositoryDatabase : IPecaRepository
{
    private static readonly ConcurrentDictionary<Guid, PecaModel> _storage = new();

    public Task<Peca?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_storage.TryGetValue(id, out var model) ? model.ToDomain() : null);
    }

    public Task<Peca?> ObterPorCodigoAsync(string codigo, CancellationToken cancellationToken = default)
    {
        var model = _storage.Values.FirstOrDefault(p => p.Codigo.Equals(codigo, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(model?.ToDomain());
    }

    public Task<IEnumerable<Peca>> ListarAsync(
        bool? apenasDisponiveis = null, 
        string? termoBusca = null, 
        CancellationToken cancellationToken = default)
    {
        var query = _storage.Values.AsEnumerable();

        if (apenasDisponiveis.HasValue && apenasDisponiveis.Value)
            query = query.Where(p => p.QuantidadeEstoque > 0);

        if (!string.IsNullOrWhiteSpace(termoBusca))
            query = query.Where(p => 
                p.Nome.Contains(termoBusca, StringComparison.OrdinalIgnoreCase) || 
                p.Codigo.Contains(termoBusca, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult<IEnumerable<Peca>>(query.Select(m => m.ToDomain()).ToList());
    }

    public Task AdicionarAsync(Peca peca, CancellationToken cancellationToken = default)
    {
        _storage[peca.Id] = peca.ToModel();
        return Task.CompletedTask;
    }

    public Task AtualizarAsync(Peca peca, CancellationToken cancellationToken = default)
    {
        _storage[peca.Id] = peca.ToModel();
        return Task.CompletedTask;
    }

    public Task RemoverAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _storage.TryRemove(id, out _);
        return Task.CompletedTask;
    }
}

/// <summary>
/// Repositório de persistência de Usuários.
/// </summary>
public class UsuarioRepositoryDatabase : IUsuarioRepository
{
    private static readonly ConcurrentDictionary<Guid, UsuarioModel> _storage = new();

    public Task<Usuario?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_storage.TryGetValue(id, out var model) ? model.ToDomain() : null);
    }

    public Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var model = _storage.Values.FirstOrDefault(u => u.Email.Equals(email.Trim(), StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(model?.ToDomain());
    }

    public Task AdicionarAsync(Usuario usuario, CancellationToken cancellationToken = default)
    {
        _storage[usuario.Id] = usuario.ToModel();
        return Task.CompletedTask;
    }

    public Task AtualizarAsync(Usuario usuario, CancellationToken cancellationToken = default)
    {
        _storage[usuario.Id] = usuario.ToModel();
        return Task.CompletedTask;
    }

    public Task<bool> EmailJaCadastradoAsync(string email, CancellationToken cancellationToken = default)
    {
        var existe = _storage.Values.Any(u => u.Email.Equals(email.Trim(), StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(existe);
    }
}

/// <summary>
/// Repositório de persistência de FAQs.
/// </summary>
public class FaqRepositoryDatabase : IFaqRepository
{
    private static readonly ConcurrentDictionary<Guid, FaqModel> _storage = new();

    public Task<IEnumerable<Faq>> ListarTodasAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IEnumerable<Faq>>(_storage.Values.Select(m => m.ToDomain()).ToList());
    }

    public Task AdicionarAsync(Faq faq, CancellationToken cancellationToken = default)
    {
        _storage[faq.Id] = faq.ToModel();
        return Task.CompletedTask;
    }
}
