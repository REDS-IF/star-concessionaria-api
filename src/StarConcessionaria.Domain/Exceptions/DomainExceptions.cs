namespace StarConcessionaria.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}

public class VeiculoIndisponivelException : DomainException
{
    public VeiculoIndisponivelException(string message = "O veículo não está disponível para reservas ou agendamentos.")
        : base(message) { }
}

public class ConflitoAgendamentoException : DomainException
{
    public ConflitoAgendamentoException(string message = "Horário indisponível para test-drive neste veículo ou loja.")
        : base(message) { }
}
