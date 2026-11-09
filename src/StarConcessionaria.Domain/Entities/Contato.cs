namespace StarConcessionaria.Domain.Entities;

public class Contato
{
    public int Id { get; set; }
    public int? ClienteId { get; set; }
    public int? VeiculoId { get; set; }
    public int? LojaId { get; set; }
    public int? UsuarioId { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Mensagem { get; set; } = string.Empty;
    public DateTimeOffset? Data { get; set; }

    public Cliente? Cliente { get; set; }
    public Veiculo? Veiculo { get; set; }
    public Loja? Loja { get; set; }
    public UsuarioSistema? Usuario { get; set; }
}
