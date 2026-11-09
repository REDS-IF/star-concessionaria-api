using System.ComponentModel.DataAnnotations;

namespace StarConcessionaria.Application.Unidades;

public record SalvarUnidadeRequest(
    [Required, StringLength(300)] string Nome,
    [StringLength(30)] string? Telefone,
    [StringLength(15)] string? Cep,
    [StringLength(300)] string? Endereco,
    [StringLength(30)] string? Numero,
    [StringLength(100)] string? Complemento,
    [StringLength(100)] string? Bairro,
    [StringLength(100)] string? Cidade,
    [StringLength(2, MinimumLength = 2)] string? Estado,
    [Range(-90, 90)] decimal? Latitude,
    [Range(-180, 180)] decimal? Longitude);

public record UnidadeResponse(
    int Id, string Nome, string? Telefone, string? Cep, string? Endereco, string? Numero,
    string? Complemento, string? Bairro, string? Cidade, string? Estado,
    decimal? Latitude, decimal? Longitude);
