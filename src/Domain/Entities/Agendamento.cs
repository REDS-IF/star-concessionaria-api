using StarConcessionaria.Domain.Enums;
using StarConcessionaria.Domain.Exceptions;
using StarConcessionaria.Domain.ValueObjects;

namespace StarConcessionaria.Domain.Entities;

/// <summary>
/// Entidade de Domínio Pura: Agendamento (Appointment)
/// Gerencia compromissos presenciais de clientes: visitas à loja física ou test-drives.
/// Aplica as regras de negócio RN04 e RN08 (validação de horário e vínculo do veículo).
/// </summary>
public class Agendamento
{
    public Guid Id { get; private set; }
    public TipoAgendamento Tipo { get; private set; }
    public Guid? VeiculoId { get; private set; }
    public Guid UnidadeId { get; private set; }
    public DadosCliente DadosCliente { get; private set; }
    public DateTime DataHora { get; private set; }
    public StatusAgendamento Status { get; private set; }
    public string? Observacoes { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public Agendamento(
        Guid id,
        TipoAgendamento tipo,
        Guid? veiculoId,
        Guid unidadeId,
        DadosCliente dadosCliente,
        DateTime dataHora,
        StatusAgendamento status = StatusAgendamento.AGENDADO,
        string? observacoes = null,
        DateTime? createdAt = null,
        DateTime? updatedAt = null)
    {
        Id = id == Guid.Empty ? Guid.NewGuid() : id;

        // Regra RN04: Se o tipo for TEST_DRIVE, a seleção de um veículo específico é obrigatória
        if (tipo == TipoAgendamento.TEST_DRIVE && (!veiculoId.HasValue || veiculoId == Guid.Empty))
            throw new DomainValidationException("Para a modalidade TEST-DRIVE, a indicação do veículo é obrigatória.");

        if (unidadeId == Guid.Empty)
            throw new DomainValidationException("A loja/unidade física do agendamento é obrigatória.");

        // O agendamento deve ser para um horário futuro (com tolerância de 5 minutos para sincronismo)
        if (dataHora < DateTime.UtcNow.AddMinutes(-5))
            throw new DomainValidationException("A data e hora do agendamento devem ser futuras.");

        Tipo = tipo;
        VeiculoId = veiculoId;
        UnidadeId = unidadeId;
        DadosCliente = dadosCliente ?? throw new DomainValidationException("Os dados do cliente são obrigatórios.");
        DataHora = dataHora;
        Status = status;
        Observacoes = observacoes?.Trim();
        CreatedAt = createdAt ?? DateTime.UtcNow;
        UpdatedAt = updatedAt ?? DateTime.UtcNow;
    }

    /// <summary>
    /// Marca o agendamento como comparecido/realizado na concessionária.
    /// </summary>
    public void ConfirmarRealizacao()
    {
        if (Status == StatusAgendamento.CANCELADO)
            throw new DomainValidationException("Não é possível concluir um agendamento cancelado.");

        Status = StatusAgendamento.REALIZADO;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Cancela o agendamento presencial.
    /// </summary>
    public void Cancelar(string? motivo = null)
    {
        Status = StatusAgendamento.CANCELADO;
        if (!string.IsNullOrWhiteSpace(motivo))
            Observacoes = string.IsNullOrWhiteSpace(Observacoes) 
                ? $"Cancelamento: {motivo.Trim()}" 
                : $"{Observacoes} | Cancelamento: {motivo.Trim()}";

        UpdatedAt = DateTime.UtcNow;
    }
}
