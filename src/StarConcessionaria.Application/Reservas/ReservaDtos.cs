using System.ComponentModel.DataAnnotations;

namespace StarConcessionaria.Application.Reservas;

public static class SituacaoReserva
{
    public const string Pendente = "Pendente";
    public const string Confirmada = "Confirmada";
    public const string Cancelada = "Cancelada";
    public const string Concluida = "Concluida";

    public static readonly string[] Validas = { Pendente, Confirmada, Cancelada, Concluida };
    public static readonly string[] Ativas = { Pendente, Confirmada };

    public static string? Normalizar(string? valor) =>
        Validas.FirstOrDefault(v => string.Equals(v, valor?.Trim(), StringComparison.OrdinalIgnoreCase));
}

public record SolicitarReservaRequest(
    [Range(1, int.MaxValue)] int VeiculoId,
    [Required, StringLength(300)] string NomeCliente,
    [Required, EmailAddress, StringLength(300)] string EmailCliente,
    [Required, StringLength(30, MinimumLength = 8)] string TelefoneCliente,
    [StringLength(500)] string? Observacoes);

public record AtualizarStatusReservaRequest(
    [Required] string NovoStatus,
    [StringLength(500)] string? Motivo);

public class ReservaQuery
{
    public string? Status { get; init; }
    public int? VeiculoId { get; init; }
    public DateTimeOffset? DataInicio { get; init; }
    public int Pagina { get; init; } = 1;
    public int ItensPorPagina { get; init; } = 12;
}

public record ReservaResponse(
    int Id, string Codigo, int VeiculoId, string VeiculoDescricao, int ClienteId,
    string NomeCliente, string EmailCliente, string TelefoneCliente,
    DateTimeOffset DataSolicitacao, string Situacao);
