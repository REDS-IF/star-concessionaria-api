using System.ComponentModel.DataAnnotations;

namespace StarConcessionaria.Application.Veiculos;

public record SalvarVeiculoRequest(
    [Range(1, int.MaxValue)] int LojaId,
    [Required, RegularExpression("^(?i)(Novo|Seminovo)$", ErrorMessage = "Tipo deve ser Novo ou Seminovo.")] string Tipo,
    [Required, StringLength(100)] string Marca,
    [Required, StringLength(100)] string Modelo,
    [Range(1900, 2100)] int Ano,
    [Range(0.01, 9999999999.99)] decimal Preco,
    [Range(0, int.MaxValue)] int Quilometragem,
    [Required, StringLength(50)] string Cor,
    string? Descricao);

public record AlterarSituacaoRequest(
    [Required] string Situacao,
    [StringLength(500)] string? Justificativa);

public record VeiculoFotoResponse(int Id, string Url, bool Principal, int Ordem);

public record AdicionarFotoRequest(
    [Required, StringLength(500), Url] string Url,
    bool Principal,
    [Range(0, 1000)] int Ordem);

public record VeiculoDetalheResponse(
    int Id, int LojaId, string? LojaNome, string Tipo, string Marca, string Modelo, int Ano,
    decimal Preco, int Quilometragem, string Cor, string? Descricao, string Situacao,
    IReadOnlyList<VeiculoFotoResponse> Fotos);

public record VeiculoHistoricoResponse(int Id, string Descricao);

public record UsuarioAtual(int Id, string Nome);

/// <summary>Filtros do catalogo publico (todos opcionais e combinaveis).</summary>
public class CatalogoQuery
{
    public string? Marca { get; init; }
    public string? Modelo { get; init; }
    public string? Tipo { get; init; }
    public decimal? PrecoMin { get; init; }
    public decimal? PrecoMax { get; init; }
    public int? AnoMin { get; init; }
    public int? AnoMax { get; init; }
    public int? UnidadeId { get; init; }
    /// <summary>Quando true, oculta veiculos Reservado/Vendido (RN05).</summary>
    public bool SomenteDisponiveis { get; init; }
    public int Pagina { get; init; } = 1;
    public int ItensPorPagina { get; init; } = 12;
}

public record VeiculoCatalogoResponse(
    int Id, int LojaId, string? LojaNome, string Tipo, string Marca, string Modelo, int Ano,
    decimal Preco, int Quilometragem, string Cor, string Situacao, string? FotoCapaUrl);
