namespace StarConcessionaria.Domain.Entities;

public class Loja
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Telefone { get; set; }
    public string? Cep { get; set; }
    public string? Endereco { get; set; }
    public string? Numero { get; set; }
    public string? Complemento { get; set; }
    public string? Bairro { get; set; }
    public string? Cidade { get; set; }
    public string? Estado { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }

    public ICollection<Veiculo> Veiculos { get; set; } = new List<Veiculo>();
}
