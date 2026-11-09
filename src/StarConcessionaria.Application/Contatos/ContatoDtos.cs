using System.ComponentModel.DataAnnotations;

namespace StarConcessionaria.Application.Contatos;

public static class TipoContato
{
    public const string Informacao = "Informacao";
    public const string ContatoVendedor = "ContatoVendedor";
    public const string InicioCompra = "InicioCompra";
}

public record NovoContatoRequest(
    [Required, StringLength(100)] string Assunto,
    [Required, StringLength(300)] string Nome,
    [Required, EmailAddress, StringLength(300)] string Email,
    [Required, StringLength(30, MinimumLength = 8)] string Telefone,
    [Required, StringLength(2000)] string Mensagem,
    int? VeiculoId,
    int? UnidadeId);

public record ContatoVendedorRequest(
    int? VeiculoId,
    [Range(1, int.MaxValue)] int UnidadeId,
    [Required, StringLength(300)] string Nome,
    [Required, EmailAddress, StringLength(300)] string Email,
    [Required, StringLength(30, MinimumLength = 8)] string Telefone,
    [Required, StringLength(2000)] string Mensagem);

public record ResponderContatoRequest(
    [Required, StringLength(2000)] string NotasResposta);

public class ContatoQuery
{
    public string? Tipo { get; init; }
    public bool? Respondido { get; init; }
    public int? UnidadeId { get; init; }
    public int Pagina { get; init; } = 1;
    public int ItensPorPagina { get; init; } = 12;
}

public record ContatoResponse(
    int Id, string Protocolo, string Tipo, int? ClienteId, string? NomeCliente,
    string? EmailCliente, string? TelefoneCliente, int? VeiculoId, string? VeiculoDescricao,
    int? UnidadeId, string? UnidadeNome, int? UsuarioAtendimentoId, string? UsuarioAtendimentoNome,
    bool Respondido, string Mensagem, DateTimeOffset Data);
