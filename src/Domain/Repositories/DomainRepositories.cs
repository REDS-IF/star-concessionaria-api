using StarConcessionaria.Domain.Entities;
using StarConcessionaria.Domain.Enums;

namespace StarConcessionaria.Domain.Repositories;

/// <summary>
/// Contrato do repositório de Veículos no domínio.
/// Permite consultas catalogadas, filtros combinados e persistência desacoplada.
/// </summary>
public interface IVeiculoRepository
{
    Task<Veiculo?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Veiculo>> ListarAsync(
        string? marca = null,
        string? modelo = null,
        int? anoMin = null,
        int? anoMax = null,
        decimal? precoMin = null,
        decimal? precoMax = null,
        SituacaoVeiculo? situacao = null,
        Guid? unidadeId = null,
        CancellationToken cancellationToken = default);
    Task AdicionarAsync(Veiculo veiculo, CancellationToken cancellationToken = default);
    Task AtualizarAsync(Veiculo veiculo, CancellationToken cancellationToken = default);
    Task<bool> ExisteAsync(Guid id, CancellationToken cancellationToken = default);
}

/// <summary>
/// Contrato do repositório de Lojas/Unidades físicas.
/// </summary>
public interface IUnidadeRepository
{
    Task<Unidade?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Unidade>> ListarTodasAsync(CancellationToken cancellationToken = default);
    Task AdicionarAsync(Unidade unidade, CancellationToken cancellationToken = default);
    Task AtualizarAsync(Unidade unidade, CancellationToken cancellationToken = default);
    Task<bool> ExisteAsync(Guid id, CancellationToken cancellationToken = default);
}

/// <summary>
/// Contrato do repositório de Reservas.
/// Dá suporte à checagem de reservas ativas para garantir as regras RN01, RN03 e RN05.
/// </summary>
public interface IReservaRepository
{
    Task<Reserva?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Reserva>> ListarTodasAsync(Guid? veiculoId = null, StatusReserva? status = null, CancellationToken cancellationToken = default);
    Task<bool> ExisteReservaAtivaParaVeiculoAsync(Guid veiculoId, CancellationToken cancellationToken = default);
    Task AdicionarAsync(Reserva reserva, CancellationToken cancellationToken = default);
    Task AtualizarAsync(Reserva reserva, CancellationToken cancellationToken = default);
}

/// <summary>
/// Contrato do repositório de Agendamentos (Visitas e Test-drives).
/// Dá suporte à validação de conflitos de horário (RN04/RN08).
/// </summary>
public interface IAgendamentoRepository
{
    Task<Agendamento?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Agendamento>> ListarTodosAsync(
        Guid? unidadeId = null,
        Guid? veiculoId = null,
        StatusAgendamento? status = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verifica se há conflito de agendamento em janela de tempo (ex: 60 minutos)
    /// para a mesma unidade ou para o mesmo veículo selecionado.
    /// </summary>
    Task<bool> ExisteConflitoHorarioAsync(
        Guid unidadeId, 
        Guid? veiculoId, 
        DateTime dataHora, 
        int duracaoMinutos = 60, 
        CancellationToken cancellationToken = default);

    Task AdicionarAsync(Agendamento agendamento, CancellationToken cancellationToken = default);
    Task AtualizarAsync(Agendamento agendamento, CancellationToken cancellationToken = default);
}

/// <summary>
/// Contrato do repositório de Propostas Comerciais e Mensagens de Contato (RN06).
/// </summary>
public interface IPropostaContatoRepository
{
    Task<PropostaContato?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<PropostaContato>> ListarAsync(
        TipoPropostaContato? tipo = null,
        StatusAtendimento? status = null,
        Guid? veiculoId = null,
        CancellationToken cancellationToken = default);
    Task AdicionarAsync(PropostaContato propostaContato, CancellationToken cancellationToken = default);
    Task AtualizarAsync(PropostaContato propostaContato, CancellationToken cancellationToken = default);
}

/// <summary>
/// Contrato do repositório de Peças e Acessórios automotivos.
/// </summary>
public interface IPecaRepository
{
    Task<Peca?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Peca?> ObterPorCodigoAsync(string codigo, CancellationToken cancellationToken = default);
    Task<IEnumerable<Peca>> ListarAsync(bool? apenasDisponiveis = null, string? termoBusca = null, CancellationToken cancellationToken = default);
    Task AdicionarAsync(Peca peca, CancellationToken cancellationToken = default);
    Task AtualizarAsync(Peca peca, CancellationToken cancellationToken = default);
    Task RemoverAsync(Guid id, CancellationToken cancellationToken = default);
}

/// <summary>
/// Contrato do repositório de Usuários.
/// </summary>
public interface IUsuarioRepository
{
    Task<Usuario?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default);
    Task AdicionarAsync(Usuario usuario, CancellationToken cancellationToken = default);
    Task AtualizarAsync(Usuario usuario, CancellationToken cancellationToken = default);
    Task<bool> EmailJaCadastradoAsync(string email, CancellationToken cancellationToken = default);
}

/// <summary>
/// Contrato do repositório de FAQs.
/// </summary>
public interface IFaqRepository
{
    Task<IEnumerable<Faq>> ListarTodasAsync(CancellationToken cancellationToken = default);
    Task AdicionarAsync(Faq faq, CancellationToken cancellationToken = default);
}
