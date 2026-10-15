using StarConcessionaria.Domain.Enums;
using StarConcessionaria.Domain.Exceptions;
using StarConcessionaria.Domain.ValueObjects;

namespace StarConcessionaria.Domain.Entities;

/// <summary>
/// Entidade de Domínio Pura: Reserva (Reservation)
/// Representa a intenção formal e temporária de compra de um veículo por um cliente.
/// Bloqueia o veículo contra novas reservas enquanto permanecer ativa.
/// </summary>
public class Reserva
{
    public Guid Id { get; private set; }
    public Guid VeiculoId { get; private set; }
    public Guid? ClienteId { get; private set; }
    public DadosCliente DadosCliente { get; private set; }
    public DateTime DataReserva { get; private set; }
    public StatusReserva Status { get; private set; }
    public string? MotivoCancelamento { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public Reserva(
        Guid id,
        Guid veiculoId,
        DadosCliente dadosCliente,
        Guid? clienteId = null,
        DateTime? dataReserva = null,
        StatusReserva status = StatusReserva.PENDENTE,
        string? motivoCancelamento = null,
        DateTime? createdAt = null,
        DateTime? updatedAt = null)
    {
        if (veiculoId == Guid.Empty)
            throw new DomainValidationException("O identificador do veículo para reserva é obrigatório.");

        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        VeiculoId = veiculoId;
        ClienteId = clienteId;
        DadosCliente = dadosCliente ?? throw new DomainValidationException("Os dados do cliente são obrigatórios.");
        DataReserva = dataReserva ?? DateTime.UtcNow;
        Status = status;
        MotivoCancelamento = motivoCancelamento;
        CreatedAt = createdAt ?? DateTime.UtcNow;
        UpdatedAt = updatedAt ?? DateTime.UtcNow;
    }

    /// <summary>
    /// Verifica se a reserva está ativa (impactando a disponibilidade do veículo).
    /// </summary>
    public bool IsAtiva() => Status == StatusReserva.PENDENTE || Status == StatusReserva.CONFIRMADA;

    /// <summary>
    /// Confirma a reserva após aprovação cadastral ou contato do vendedor.
    /// </summary>
    public void Confirmar()
    {
        if (Status == StatusReserva.CANCELADA || Status == StatusReserva.EXPIRADA)
            throw new DomainValidationException($"Não é possível confirmar uma reserva que já está {Status}.");

        Status = StatusReserva.CONFIRMADA;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Cancela a reserva (liberando o veículo no estoque da concessionária).
    /// </summary>
    public void Cancelar(string motivo)
    {
        if (string.IsNullOrWhiteSpace(motivo))
            throw new DomainValidationException("O motivo do cancelamento deve ser informado.");

        Status = StatusReserva.CANCELADA;
        MotivoCancelamento = motivo.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marca a reserva como expirada pelo término do prazo de validade sem fechamento.
    /// </summary>
    public void Expirar()
    {
        Status = StatusReserva.EXPIRADA;
        UpdatedAt = DateTime.UtcNow;
    }
}
