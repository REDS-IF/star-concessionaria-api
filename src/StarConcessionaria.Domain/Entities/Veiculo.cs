namespace StarConcessionaria.Domain.Entities;

public class Veiculo
{
    public int Id { get; set; }
    public int LojaId { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public int Ano { get; set; }
    public decimal Preco { get; set; }
    public int Quilometragem { get; set; }
    public string Cor { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string Situacao { get; set; } = SituacaoVeiculo.Disponivel;

    public Loja? Loja { get; set; }
    public ICollection<FotoVeiculo> Fotos { get; set; } = new List<FotoVeiculo>();
    public ICollection<AlteracaoVeiculo> Alteracoes { get; set; } = new List<AlteracaoVeiculo>();
}

public static class SituacaoVeiculo
{
    public const string Disponivel = "Disponivel";
    public const string Reservado = "Reservado";
    public const string Vendido = "Vendido";

    public static readonly string[] Validas = { Disponivel, Reservado, Vendido };

    public static string? Normalizar(string? valor) =>
        Validas.FirstOrDefault(v => string.Equals(v, valor?.Trim(), StringComparison.OrdinalIgnoreCase));
}
