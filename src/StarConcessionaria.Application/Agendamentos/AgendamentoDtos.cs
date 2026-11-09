using System.ComponentModel.DataAnnotations;

namespace StarConcessionaria.Application.Agendamentos;

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

    public static readonly string[] Validas = { Solicitado, Confirmado, Realizado, Cancelado };

    public static string? Normalizar(string? valor) =>
        Validas.FirstOrDefault(v => string.Equals(v, valor?.Trim(), StringComparison.OrdinalIgnoreCase));
}

public record AgendarVisitaRequest(
    [Range(1, int.MaxValue)] int UnidadeId,
    [Required, StringLength(300)] string NomeCliente,
    [Required, EmailAddress, StringLength(300)] string EmailCliente,
    [Required, StringLength(30, MinimumLength = 8)] string TelefoneCliente,
    [Required] DateTimeOffset DataHoraInicio);

public record AgendarTestDriveRequest(
    [Range(1, int.MaxValue)] int VeiculoId,
    [Range(1, int.MaxValue)] int UnidadeId,
    [Required, StringLength(300)] string NomeCliente,
    [Required, EmailAddress, StringLength(300)] string EmailCliente,
    [Required, StringLength(30, MinimumLength = 8)] string TelefoneCliente,
    [StringLength(30)] string? CnhCliente,
    [Required] DateTimeOffset DataHoraInicio);

public record AtualizarStatusAgendamentoRequest(
    [Required] string Status,
    [StringLength(500)] string? Observacoes);

public class AgendamentoQuery
{
    public int? UnidadeId { get; init; }
    public string? Status { get; init; }
    public string? Tipo { get; init; }
    public DateOnly? Data { get; init; }
    public int Pagina { get; init; } = 1;
    public int ItensPorPagina { get; init; } = 12;
}

public record AgendamentoResponse(
    int Id, string Codigo, string Tipo, int UnidadeId, string UnidadeNome,
    int? VeiculoId, string? VeiculoDescricao, int ClienteId, string NomeCliente,
    string EmailCliente, string TelefoneCliente, DateTimeOffset DataHoraInicio,
    DateTimeOffset DataHoraFim, string Situacao);
