using System.ComponentModel.DataAnnotations;

namespace StarConcessionaria.Application.Pecas;

public record SalvarPecaRequest(
    [Required, StringLength(300)] string Nome,
    string? Descricao,
    [Range(0.01, 99999999.99)] decimal Preco,
    [Range(0, int.MaxValue)] int QuantidadeEstoque);

public record RegistrarVendaPecaRequest(
    [Range(1, int.MaxValue)] int Quantidade,
    [StringLength(300)] string? NomeCliente,
    [EmailAddress, StringLength(300)] string? EmailCliente,
    [StringLength(30)] string? TelefoneCliente);

public class PecaQuery
{
    public string? Busca { get; init; }
    public bool? Disponivel { get; init; }
    public int Pagina { get; init; } = 1;
    public int ItensPorPagina { get; init; } = 12;
}

public record PecaResponse(
    int Id, string Nome, string? Descricao, decimal Preco, int QuantidadeEstoque, bool Disponivel);

public record VendaPecaResponse(
    int Id, int PecaId, string PecaNome, int? ClienteId, int Quantidade,
    decimal ValorUnitario, decimal ValorTotal, DateTimeOffset DataVenda, int EstoqueRestante);
