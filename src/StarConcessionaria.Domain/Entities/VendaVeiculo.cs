namespace StarConcessionaria.Domain.Entities;

public class VendaVeiculo
{
    public int Id { get; set; }
    public int VeiculoId { get; set; }
    public int ClienteId { get; set; }
    public int VendedorId { get; set; }
    public int LojaId { get; set; }
    public int FormaPagamentoId { get; set; }
    public DateTimeOffset? DataVenda { get; set; }
    public decimal ValorFinal { get; set; }

    public Veiculo? Veiculo { get; set; }
    public Cliente? Cliente { get; set; }
    public UsuarioSistema? Vendedor { get; set; }
    public Loja? Loja { get; set; }
    public FormaPagamento? FormaPagamento { get; set; }
}
