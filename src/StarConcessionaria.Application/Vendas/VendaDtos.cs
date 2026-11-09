using System.ComponentModel.DataAnnotations;

namespace StarConcessionaria.Application.Vendas;

public record RegistrarVendaRequest(
    [Range(1, int.MaxValue)] int VeiculoId,
    [Required, StringLength(300)] string NomeCliente,
    [Required, EmailAddress, StringLength(300)] string EmailCliente,
    [Required, StringLength(30, MinimumLength = 8)] string TelefoneCliente,
    [Range(1, int.MaxValue)] int FormaPagamentoId,
    [Range(0.01, 9999999999.99)] decimal ValorFinal);

public class VendaQuery
{
    public int? UnidadeId { get; init; }
    public int? VendedorId { get; init; }
    public DateTimeOffset? PeriodoInicio { get; init; }
    public DateTimeOffset? PeriodoFim { get; init; }
    public int Pagina { get; init; } = 1;
    public int ItensPorPagina { get; init; } = 12;
}

public record VendaResponse(
    int Id, int VeiculoId, string VeiculoDescricao, int ClienteId, string NomeCliente,
    int VendedorId, string NomeVendedor, int LojaId, string NomeLoja,
    int FormaPagamentoId, string FormaPagamentoNome, decimal ValorFinal, DateTimeOffset DataVenda);
