namespace StarConcessionaria.Domain.Entities;

public class AlteracaoVeiculo
{
    public int Id { get; set; }
    public int VeiculoId { get; set; }
    public string Descricao { get; set; } = string.Empty;

    public Veiculo? Veiculo { get; set; }
}
