using StarConcessionaria.Application.DTOs;
using StarConcessionaria.Domain.Entities;
using StarConcessionaria.Domain.Enums;
using StarConcessionaria.Domain.Exceptions;
using StarConcessionaria.Domain.Repositories;
using StarConcessionaria.Domain.ValueObjects;

namespace StarConcessionaria.Application.UseCases.Reservas;

/// <summary>
/// Caso de Uso: Solicitação de Reserva de Veículo pelo Cliente.
/// Executa estritamente as regras de negócio:
/// - RN01: Apenas veículos DISPONÍVEIS podem ser reservados.
/// - RN02: Veículos VENDIDOS têm qualquer operação de reserva sumariamente bloqueada.
/// - RN03: Veículos com reserva ativa pendente ou confirmada não aceitam nova reserva.
/// - RN05 / RF15: Revalidação atômica de disponibilidade do veículo antes da criação.
/// </summary>
public class ReservarVeiculoUseCase
{
    private readonly IVeiculoRepository _veiculoRepository;
    private readonly IReservaRepository _reservaRepository;

    public ReservarVeiculoUseCase(IVeiculoRepository veiculoRepository, IReservaRepository reservaRepository)
    {
        _veiculoRepository = veiculoRepository;
        _reservaRepository = reservaRepository;
    }

    public async Task<ReservaResponseDto> ExecutarAsync(
        Guid veiculoId, 
        SolicitarReservaRequestDto request, 
        Guid? clienteAutenticadoId = null,
        CancellationToken cancellationToken = default)
    {
        // 1. Localiza o veículo pretendido
        var veiculo = await _veiculoRepository.ObterPorIdAsync(veiculoId, cancellationToken)
            ?? throw new EntityNotFoundException("Veículo", veiculoId);

        // 2. Aplica RN02: Bloqueio estrito se o veículo estiver vendido
        if (veiculo.Situacao == SituacaoVeiculo.VENDIDO)
            throw new VehicleSoldException("O veículo solicitado já foi vendido e não está mais disponível para reservas.");

        // 3. Aplica RN01: Checagem se o veículo está com status DISPONÍVEL
        if (!veiculo.IsDisponivel())
            throw new VehicleNotAvailableException($"O veículo encontra-se atualmente {veiculo.Situacao} e não pode receber nova reserva.");

        // 4. Aplica RN03 e RN05 / RF15: Revalidação atômica de reservas ativas concorrentes
        var temReservaAtiva = await _reservaRepository.ExisteReservaAtivaParaVeiculoAsync(veiculoId, cancellationToken);
        if (temReservaAtiva)
            throw new VehicleNotAvailableException("Já existe uma reserva em andamento para este veículo.");

        // 5. Instancia os Value Objects e a Entidade de Domínio Pura
        var dadosCliente = new DadosCliente(request.NomeCliente, request.EmailCliente, request.TelefoneCliente);
        
        var reserva = new Reserva(
            id: Guid.NewGuid(),
            veiculoId: veiculo.Id,
            dadosCliente: dadosCliente,
            clienteId: clienteAutenticadoId,
            status: StatusReserva.PENDENTE
        );

        // 6. Transição de estado de domínio do Veículo para RESERVADO
        veiculo.Reservar();

        // 7. Persistência coordenada (desacoplada de infraestrutura via Repositórios)
        await _veiculoRepository.AtualizarAsync(veiculo, cancellationToken);
        await _reservaRepository.AdicionarAsync(reserva, cancellationToken);

        return ReservaResponseDto.FromEntity(reserva, $"{veiculo.Marca} {veiculo.Modelo}");
    }
}

/// <summary>
/// Caso de Uso: Gestão Administrativa de Reservas (Aprovação, Cancelamento e Consulta).
/// </summary>
public class GerenciarReservasAdminUseCase
{
    private readonly IReservaRepository _reservaRepository;
    private readonly IVeiculoRepository _veiculoRepository;

    public GerenciarReservasAdminUseCase(IReservaRepository reservaRepository, IVeiculoRepository veiculoRepository)
    {
        _reservaRepository = reservaRepository;
        _veiculoRepository = veiculoRepository;
    }

    public async Task<IEnumerable<ReservaResponseDto>> ListarAsync(
        Guid? veiculoId = null, 
        StatusReserva? status = null, 
        CancellationToken cancellationToken = default)
    {
        var reservas = await _reservaRepository.ListarTodasAsync(veiculoId, status, cancellationToken);
        var dtos = new List<ReservaResponseDto>();

        foreach (var r in reservas)
        {
            var veiculo = await _veiculoRepository.ObterPorIdAsync(r.VeiculoId, cancellationToken);
            dtos.Add(ReservaResponseDto.FromEntity(r, veiculo != null ? $"{veiculo.Marca} {veiculo.Modelo}" : null));
        }

        return dtos;
    }

    public async Task<ReservaResponseDto> AtualizarStatusAsync(
        Guid reservaId, 
        AtualizarStatusReservaRequestDto request, 
        CancellationToken cancellationToken = default)
    {
        var reserva = await _reservaRepository.ObterPorIdAsync(reservaId, cancellationToken)
            ?? throw new EntityNotFoundException("Reserva", reservaId);

        var veiculo = await _veiculoRepository.ObterPorIdAsync(reserva.VeiculoId, cancellationToken)
            ?? throw new EntityNotFoundException("Veículo", reserva.VeiculoId);

        switch (request.NovoStatus)
        {
            case StatusReserva.CONFIRMADA:
                reserva.Confirmar();
                break;

            case StatusReserva.CANCELADA:
                reserva.Cancelar(request.MotivoCancelamento ?? "Cancelamento administrativo.");
                // Se a reserva foi cancelada, o veículo é liberado de volta para DISPONIVEL
                if (veiculo.Situacao == SituacaoVeiculo.RESERVADO)
                {
                    veiculo.LiberarReserva();
                    await _veiculoRepository.AtualizarAsync(veiculo, cancellationToken);
                }
                break;

            case StatusReserva.EXPIRADA:
                reserva.Expirar();
                if (veiculo.Situacao == SituacaoVeiculo.RESERVADO)
                {
                    veiculo.LiberarReserva();
                    await _veiculoRepository.AtualizarAsync(veiculo, cancellationToken);
                }
                break;
        }

        await _reservaRepository.AtualizarAsync(reserva, cancellationToken);
        return ReservaResponseDto.FromEntity(reserva, $"{veiculo.Marca} {veiculo.Modelo}");
    }
}
