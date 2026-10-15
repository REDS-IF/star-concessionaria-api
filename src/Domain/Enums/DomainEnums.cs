namespace StarConcessionaria.Domain.Enums;

/// <summary>
/// Representa a situação do veículo no estoque da concessionária.
/// </summary>
public enum SituacaoVeiculo
{
    DISPONIVEL = 1,
    RESERVADO = 2,
    VENDIDO = 3
}

/// <summary>
/// Status do ciclo de vida da reserva do veículo.
/// </summary>
public enum StatusReserva
{
    PENDENTE = 1,
    CONFIRMADA = 2,
    CANCELADA = 3,
    EXPIRADA = 4
}

/// <summary>
/// Modalidade do agendamento presencial.
/// </summary>
public enum TipoAgendamento
{
    VISITA = 1,
    TEST_DRIVE = 2
}

/// <summary>
/// Status do agendamento presencial.
/// </summary>
public enum StatusAgendamento
{
    AGENDADO = 1,
    REALIZADO = 2,
    CANCELADO = 3
}

/// <summary>
/// Tipo de mensagem comercial recebida pelo cliente.
/// </summary>
public enum TipoPropostaContato
{
    SOLICITACAO_INFORMACOES = 1,
    PROPOSTA_COMPRA = 2
}

/// <summary>
/// Opções de pagamento disponíveis para propostas comerciais.
/// </summary>
public enum FormaPagamento
{
    A_VISTA = 1,
    CONSORCIO = 2,
    FINANCIAMENTO = 3
}

/// <summary>
/// Status do pipeline de atendimento comercial do lead.
/// </summary>
public enum StatusAtendimento
{
    NOVO = 1,
    EM_ATENDIMENTO = 2,
    FINALIZADO = 3
}

/// <summary>
/// Níveis de controle de acesso de usuários (RBAC).
/// </summary>
public enum PerfilUsuario
{
    CLIENTE = 1,
    COMERCIAL = 2,
    LOGISTICA = 3,
    ADMINISTRADOR = 4
}
