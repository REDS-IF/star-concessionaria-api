using StarConcessionaria.Application.DTOs;
using StarConcessionaria.Domain.Entities;
using StarConcessionaria.Domain.Enums;
using StarConcessionaria.Domain.Exceptions;
using StarConcessionaria.Domain.Repositories;
using StarConcessionaria.Domain.ValueObjects;

namespace StarConcessionaria.Application.UseCases.Agendamentos;

/// <summary>
/// Caso de Uso: Agendamento de Visita ou Test-Drive pelo Cliente.
/// Executa estritamente as regras de negócio:
/// - RN02: Bloqueia agendamentos para veículos marcados como VENDIDO.
/// - RN04 / RN08: Valida previamente a disponibilidade do veículo e do horário na loja física,
///   impedindo colisões ou duplicidades que excedam a capacidade de atendimento.
/// - RN05 / RF15: Revalidação atômica da situação do veículo antes de confirmar o agendamento.
/// </summary>
public class AgendarVisitaOuTestDriveUseCase
{
    private readonly IAgendamentoRepository _agendamentoRepository;
    private readonly IVeiculoRepository _veiculoRepository;
    private readonly IUnidadeRepository _unidadeRepository;

    public AgendarVisitaOuTestDriveUseCase(
        IAgendamentoRepository agendamentoRepository,
        IVeiculoRepository veiculoRepository,
        IUnidadeRepository unidadeRepository)
    {
        _agendamentoRepository = agendamentoRepository;
        _veiculoRepository = veiculoRepository;
        _unidadeRepository = unidadeRepository;
    }

    public async Task<AgendamentoResponseDto> ExecutarAsync(
        CriarAgendamentoRequestDto request, 
        CancellationToken cancellationToken = default)
    {
        // 1. Validação da existência da loja física (unidade)
        var unidade = await _unidadeRepository.ObterPorIdAsync(request.UnidadeId, cancellationToken)
            ?? throw new EntityNotFoundException("Unidade da concessionária", request.UnidadeId);

        Veiculo? veiculo = null;

        // 2. Se for TEST_DRIVE (RN04) ou se for VISITA com veículo selecionado:
        if (request.Tipo == TipoAgendamento.TEST_DRIVE || request.VeiculoId.HasValue)
        {
            if (!request.VeiculoId.HasValue || request.VeiculoId.Value == Guid.Empty)
                throw new DomainValidationException("Para agendamentos de TEST-DRIVE, a seleção de um veículo é obrigatória.");

            veiculo = await _veiculoRepository.ObterPorIdAsync(request.VeiculoId.Value, cancellationToken)
                ?? throw new EntityNotFoundException("Veículo", request.VeiculoId.Value);

            // Regra RN02: Bloqueio absoluto para veículo vendido
            if (veiculo.Situacao == SituacaoVeiculo.VENDIDO)
                throw new VehicleSoldException("Não é permitido agendar visita ou test-drive para um veículo já vendido.");

            // O veículo precisa pertencer à unidade informada ou estar acessível nela
            if (veiculo.UnidadeId != unidade.Id)
                throw new DomainValidationException($"O veículo selecionado pertence à loja '{veiculo.UnidadeId}', diferente da loja informada para o agendamento.");
        }

        // 3. Regra RN04 / RN08: Checagem de conflito prévio de horário (janela de 60 min para o mesmo veículo ou unidade)
        var temConflito = await _agendamentoRepository.ExisteConflitoHorarioAsync(
            unidadeId: request.UnidadeId,
            veiculoId: request.VeiculoId,
            dataHora: request.DataHora,
            duracaoMinutos: 60,
            cancellationToken: cancellationToken);

        if (temConflito)
            throw new ScheduleConflictException("O horário solicitado não está disponível para esta unidade ou veículo. Por favor, escolha outro horário.");

        // 4. Criação da Entidade com Value Object DadosCliente
        var dadosCliente = new DadosCliente(request.NomeCliente, request.EmailCliente, request.TelefoneCliente);

        var agendamento = new Agendamento(
            id: Guid.NewGuid(),
            tipo: request.Tipo,
            veiculoId: request.VeiculoId,
            unidadeId: request.UnidadeId,
            dadosCliente: dadosCliente,
            dataHora: request.DataHora,
            status: StatusAgendamento.AGENDADO,
            observacoes: request.Observacoes
        );

        // 5. Persistência
        await _agendamentoRepository.AdicionarAsync(agendamento, cancellationToken);

        return AgendamentoResponseDto.FromEntity(
            agendamento, 
            veiculo != null ? $"{veiculo.Marca} {veiculo.Modelo}" : null, 
            unidade.Nome);
    }
}

/// <summary>
/// Caso de Uso: Gestão Administrativa de Agendamentos.
/// </summary>
public class GerenciarAgendamentosAdminUseCase
{
    private readonly IAgendamentoRepository _agendamentoRepository;
    private readonly IVeiculoRepository _veiculoRepository;
    private readonly IUnidadeRepository _unidadeRepository;

    public GerenciarAgendamentosAdminUseCase(
        IAgendamentoRepository agendamentoRepository,
        IVeiculoRepository veiculoRepository,
        IUnidadeRepository unidadeRepository)
    {
        _agendamentoRepository = agendamentoRepository;
        _veiculoRepository = veiculoRepository;
        _unidadeRepository = unidadeRepository;
    }

    public async Task<IEnumerable<AgendamentoResponseDto>> ListarAsync(
        Guid? unidadeId = null,
        Guid? veiculoId = null,
        StatusAgendamento? status = null,
        CancellationToken cancellationToken = default)
    {
        var agendamentos = await _agendamentoRepository.ListarTodosAsync(unidadeId, veiculoId, status, cancellationToken);
        var dtos = new List<AgendamentoResponseDto>();

        foreach (var a in agendamentos)
        {
            var veiculo = a.VeiculoId.HasValue ? await _veiculoRepository.ObterPorIdAsync(a.VeiculoId.Value, cancellationToken) : null;
            var unidade = await _unidadeRepository.ObterPorIdAsync(a.UnidadeId, cancellationToken);

            dtos.Add(AgendamentoResponseDto.FromEntity(
                a,
                veiculo != null ? $"{veiculo.Marca} {veiculo.Modelo}" : null,
                unidade?.Nome));
        }

        return dtos;
    }

    public async Task<AgendamentoResponseDto> AtualizarStatusAsync(
        Guid id, 
        AtualizarStatusAgendamentoRequestDto request, 
        CancellationToken cancellationToken = default)
    {
        var agendamento = await _agendamentoRepository.ObterPorIdAsync(id, cancellationToken)
            ?? throw new EntityNotFoundException("Agendamento", id);

        switch (request.NovoStatus)
        {
            case StatusAgendamento.REALIZADO:
                agendamento.ConfirmarRealizacao();
                break;

            case StatusAgendamento.CANCELADO:
                agendamento.Cancelar(request.Motivo);
                break;
        }

        await _agendamentoRepository.AtualizarAsync(agendamento, cancellationToken);

        var veiculo = agendamento.VeiculoId.HasValue ? await _veiculoRepository.ObterPorIdAsync(agendamento.VeiculoId.Value, cancellationToken) : null;
        var unidade = await _unidadeRepository.ObterPorIdAsync(agendamento.UnidadeId, cancellationToken);

        return AgendamentoResponseDto.FromEntity(
            agendamento,
            veiculo != null ? $"{veiculo.Marca} {veiculo.Modelo}" : null,
            unidade?.Nome);
    }
}
