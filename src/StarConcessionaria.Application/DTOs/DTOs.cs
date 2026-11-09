namespace StarConcessionaria.Application.DTOs;

public record LoginRequest(string Email, string Senha);

public record LoginResponse(string Token, string Nome, string Email, string Perfil);

public record LojaResponse(
    int IdLoj,
    string NomeLoj,
    string? TelefoneLoj,
    string? EnderecoLoj,
    string? NumeroLoj,
    string? BairroLoj,
    string? CidadeLoj,
    string? EstadoLoj,
    decimal? LatitudeLoj,
    decimal? LongitudeLoj
);

public record CriarLojaRequest(
    string NomeLoj,
    string? TelefoneLoj,
    string? CepLoj,
    string? EnderecoLoj,
    string? NumeroLoj,
    string? ComplementoLoj,
    string? BairroLoj,
    string? CidadeLoj,
    string? EstadoLoj,
    decimal? LatitudeLoj,
    decimal? LongitudeLoj
);

public record VeiculoCatalogoResponse(
    int IdVei,
    int IdLojFk,
    string NomeLoja,
    string TipoVei,
    string MarcaVei,
    string ModeloVei,
    int AnoVei,
    decimal PrecoVei,
    int QuilometragemVei,
    string CorVei,
    string SituacaoVei,
    string? UrlFotoPrincipal
);

public record CriarVeiculoRequest(
    int IdLojFk,
    string TipoVei,
    string MarcaVei,
    string ModeloVei,
    int AnoVei,
    decimal PrecoVei,
    int QuilometragemVei,
    string CorVei,
    string? DescricaoVei
);

public record FiltroVeiculosRequest(
    string? Marca = null,
    string? Modelo = null,
    decimal? PrecoMin = null,
    decimal? PrecoMax = null,
    int? AnoMin = null,
    int? AnoMax = null,
    string? Tipo = null,
    int? LojaId = null
);

public record FaqResponse(
    int IdDf,
    string PerguntaDf,
    string RespostaDf
);
