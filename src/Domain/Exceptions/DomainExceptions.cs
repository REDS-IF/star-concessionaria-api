using StarConcessionaria.Domain.Common;

namespace StarConcessionaria.Domain.Exceptions;

/// <summary>
/// Exceção lançada quando dados obrigatórios ou formatos falham na validação de entidade.
/// </summary>
public class DomainValidationException : DomainException
{
    public DomainValidationException(string message) : base(message) { }
}

/// <summary>
/// Exceção para regras RN01/RN03/RN05: Veículo indisponível para reserva ou test-drive.
/// </summary>
public class VehicleNotAvailableException : DomainException
{
    public VehicleNotAvailableException(string message = "O veículo não está disponível para esta operação.") 
        : base(message) { }
}

/// <summary>
/// Exceção para regra RN02: Bloqueio estrito quando o veículo já foi marcado como VENDIDO.
/// </summary>
public class VehicleSoldException : DomainException
{
    public VehicleSoldException(string message = "Operação bloqueada: o veículo já foi vendido.") 
        : base(message) { }
}

/// <summary>
/// Exceção para regras RN04/RN08: Conflito de data/horário para visitas e test-drives na unidade ou veículo.
/// </summary>
public class ScheduleConflictException : DomainException
{
    public ScheduleConflictException(string message = "Já existe um agendamento conflitante para este veículo ou unidade no horário selecionado.") 
        : base(message) { }
}

/// <summary>
/// Exceção lançada quando uma entidade não é encontrada no repositório.
/// </summary>
public class EntityNotFoundException : DomainException
{
    public EntityNotFoundException(string entityName, object id) 
        : base($"{entityName} com identificador '{id}' não foi encontrado(a).") { }
}

/// <summary>
/// Exceção lançada para inconsistências de estoque de peças.
/// </summary>
public class InsufficientStockException : DomainException
{
    public InsufficientStockException(string message = "Quantidade solicitada excede o estoque disponível da peça.") 
        : base(message) { }
}
