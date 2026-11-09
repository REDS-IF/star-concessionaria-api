namespace StarConcessionaria.Domain.Entities;

public class Reserva
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public int VeiculoId { get; set; }
    public DateTimeOffset? DataSolicitacao { get; set; }
    public string Situacao { get; set; } = "Pendente";

    public Cliente? Cliente { get; set; }
    public Veiculo? Veiculo { get; set; }
}
