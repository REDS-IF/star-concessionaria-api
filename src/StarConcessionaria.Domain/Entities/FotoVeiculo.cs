namespace StarConcessionaria.Domain.Entities;

public class FotoVeiculo
{
    public int Id { get; set; }
    public int VeiculoId { get; set; }
    public string Url { get; set; } = string.Empty;
    /// <summary>Foto de capa. No maximo uma por veiculo (indice unico parcial).</summary>
    public bool Principal { get; set; }
    public int Ordem { get; set; }

    public Veiculo? Veiculo { get; set; }
}
