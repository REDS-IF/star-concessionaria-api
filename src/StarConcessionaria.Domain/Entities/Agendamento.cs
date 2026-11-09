namespace StarConcessionaria.Domain.Entities;

public class Agendamento
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public int LojaId { get; set; }
    public int? VeiculoId { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public DateTimeOffset DataHoraInicio { get; set; }
    public DateTimeOffset DataHoraFim { get; set; }
    public string Situacao { get; set; } = "Solicitado";

    public Cliente? Cliente { get; set; }
    public Loja? Loja { get; set; }
    public Veiculo? Veiculo { get; set; }
}
