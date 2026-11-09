namespace StarConcessionaria.Domain.Entities;

public class VendaPeca
{
    public int Id { get; set; }
    public int PecaId { get; set; }
    public int? ClienteId { get; set; }
    public int Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }
    public DateTimeOffset? DataVenda { get; set; }

    public Peca? Peca { get; set; }
    public Cliente? Cliente { get; set; }
}
