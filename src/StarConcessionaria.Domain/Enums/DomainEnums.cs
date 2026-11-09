namespace StarConcessionaria.Domain.Enums;

public static class SituacaoVeiculo
{
    public const string Disponivel = "Disponivel";
    public const string Reservado = "Reservado";
    public const string Vendido = "Vendido";
}

public static class TipoVeiculo
{
    public const string Novo = "Novo";
    public const string Seminovo = "Seminovo";
}

public static class SituacaoReserva
{
    public const string Pendente = "Pendente";
    public const string Confirmada = "Confirmada";
    public const string Cancelada = "Cancelada";
    public const string Expirada = "Expirada";
    public const string Concluida = "Concluida";
}

public static class TipoAgendamento
{
    public const string Visita = "Visita";
    public const string TestDrive = "TestDrive";
}

public static class SituacaoAgendamento
{
    public const string Solicitado = "Solicitado";
    public const string Confirmado = "Confirmado";
    public const string Realizado = "Realizado";
    public const string Cancelado = "Cancelado";
}

public static class PerfilUsuario
{
    public const string Administrador = "administrador";
    public const string Comercial = "comercial";
    public const string Logistica = "logistica";
}

public static class TipoContato
{
    public const string Informacao = "Informacao";
    public const string Proposta = "Proposta";
    public const string InicioCompra = "InicioCompra";
    public const string ContatoVendedor = "ContatoVendedor";
}
