using StarConcessionaria.Domain.Enums;
using StarConcessionaria.Domain.Exceptions;

namespace StarConcessionaria.Domain.Entities;

public class Loja
{
    public int IdLoj { get; set; }
    public string NomeLoj { get; set; } = string.Empty;
    public string? TelefoneLoj { get; set; }
    public string? CepLoj { get; set; }
    public string? EnderecoLoj { get; set; }
    public string? NumeroLoj { get; set; }
    public string? ComplementoLoj { get; set; }
    public string? BairroLoj { get; set; }
    public string? CidadeLoj { get; set; }
    public string? EstadoLoj { get; set; }
    public decimal? LatitudeLoj { get; set; }
    public decimal? LongitudeLoj { get; set; }

    public ICollection<Veiculo> Veiculos { get; set; } = new List<Veiculo>();
    public ICollection<Agendamento> Agendamentos { get; set; } = new List<Agendamento>();
    public ICollection<Contato> Contatos { get; set; } = new List<Contato>();
    public ICollection<VendaVeiculo> Vendas { get; set; } = new List<VendaVeiculo>();
}

public class UsuarioSistema
{
    public int IdUsu { get; set; }
    public string NomeUsu { get; set; } = string.Empty;
    public string EmailUsu { get; set; } = string.Empty;
    public string SenhaUsu { get; set; } = string.Empty;
    public string PerfilUsu { get; set; } = PerfilUsuario.Comercial;

    public ICollection<Contato> ContatosAtendidos { get; set; } = new List<Contato>();
    public ICollection<VendaVeiculo> VendasRealizadas { get; set; } = new List<VendaVeiculo>();
}

public class Cliente
{
    public int IdCli { get; set; }
    public string NomeCli { get; set; } = string.Empty;
    public string EmailCli { get; set; } = string.Empty;
    public string TelefoneCli { get; set; } = string.Empty;

    public ICollection<Contato> Contatos { get; set; } = new List<Contato>();
    public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
    public ICollection<Agendamento> Agendamentos { get; set; } = new List<Agendamento>();
    public ICollection<VendaVeiculo> ComprasVeiculo { get; set; } = new List<VendaVeiculo>();
    public ICollection<VendaPeca> ComprasPeca { get; set; } = new List<VendaPeca>();
}

public class FormaPagamento
{
    public int IdFp { get; set; }
    public string NomeFp { get; set; } = string.Empty;

    public ICollection<VendaVeiculo> Vendas { get; set; } = new List<VendaVeiculo>();
}

public class Veiculo
{
    public int IdVei { get; set; }
    public int IdLojFk { get; set; }
    public string TipoVei { get; set; } = TipoVeiculo.Seminovo;
    public string MarcaVei { get; set; } = string.Empty;
    public string ModeloVei { get; set; } = string.Empty;
    public int AnoVei { get; set; }
    public decimal PrecoVei { get; set; }
    public int QuilometragemVei { get; set; }
    public string CorVei { get; set; } = string.Empty;
    public string? DescricaoVei { get; set; }
    public string SituacaoVei { get; set; } = SituacaoVeiculo.Disponivel;

    public Loja? Loja { get; set; }
    public ICollection<FotoVeiculo> Fotos { get; set; } = new List<FotoVeiculo>();
    public ICollection<AlteracaoVeiculo> Alteracoes { get; set; } = new List<AlteracaoVeiculo>();
    public ICollection<Contato> Contatos { get; set; } = new List<Contato>();
    public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
    public ICollection<Agendamento> Agendamentos { get; set; } = new List<Agendamento>();
    public VendaVeiculo? Venda { get; set; }

    public void Reservar()
    {
        if (SituacaoVei != SituacaoVeiculo.Disponivel)
            throw new VeiculoIndisponivelException($"Veículo com status '{SituacaoVei}' não aceita reservas (RN01).");

        SituacaoVei = SituacaoVeiculo.Reservado;
    }

    public void MarcarComoVendido()
    {
        SituacaoVei = SituacaoVeiculo.Vendido;
    }

    public void LiberarReserva()
    {
        if (SituacaoVei == SituacaoVeiculo.Reservado)
        {
            SituacaoVei = SituacaoVeiculo.Disponivel;
        }
    }
}

public class FotoVeiculo
{
    public int IdFot { get; set; }
    public int IdVeiFk { get; set; }
    public string UrlFot { get; set; } = string.Empty;

    public Veiculo? Veiculo { get; set; }
}

public class AlteracaoVeiculo
{
    public int IdAlt { get; set; }
    public int IdVeiFk { get; set; }
    public string DescricaoAlt { get; set; } = string.Empty;

    public Veiculo? Veiculo { get; set; }
}

public class Contato
{
    public int IdCon { get; set; }
    public int? IdCliFk { get; set; }
    public int? IdVeiFk { get; set; }
    public int? IdLojFk { get; set; }
    public int? IdUsuFk { get; set; }
    public string TipoCon { get; set; } = TipoContato.Informacao;
    public string MensagemCon { get; set; } = string.Empty;
    public DateTime DataCon { get; set; } = DateTime.UtcNow;

    public Cliente? Cliente { get; set; }
    public Veiculo? Veiculo { get; set; }
    public Loja? Loja { get; set; }
    public UsuarioSistema? Usuario { get; set; }
}

public class Reserva
{
    public int IdRes { get; set; }
    public int IdCliFk { get; set; }
    public int IdVeiFk { get; set; }
    public DateTime DataSolicitacaoRes { get; set; } = DateTime.UtcNow;
    public string SituacaoRes { get; set; } = SituacaoReserva.Pendente;

    public Cliente? Cliente { get; set; }
    public Veiculo? Veiculo { get; set; }
}

public class Agendamento
{
    public int IdAge { get; set; }
    public int IdCliFk { get; set; }
    public int IdLojFk { get; set; }
    public int? IdVeiFk { get; set; }
    public string TipoAge { get; set; } = TipoAgendamento.Visita;
    public DateTime DataHoraInicioAge { get; set; }
    public DateTime DataHoraFimAge { get; set; }
    public string SituacaoAge { get; set; } = SituacaoAgendamento.Solicitado;

    public Cliente? Cliente { get; set; }
    public Loja? Loja { get; set; }
    public Veiculo? Veiculo { get; set; }
}

public class Peca
{
    public int IdPec { get; set; }
    public string NomePec { get; set; } = string.Empty;
    public string? DescricaoPec { get; set; }
    public decimal PrecoPec { get; set; }
    public int QuantidadeEstoquePec { get; set; }

    public ICollection<VendaPeca> Vendas { get; set; } = new List<VendaPeca>();
}

public class VendaVeiculo
{
    public int IdVv { get; set; }
    public int IdVeiFk { get; set; }
    public int IdCliFk { get; set; }
    public int IdVendedorFk { get; set; }
    public int IdLojFk { get; set; }
    public int IdFpFk { get; set; }
    public DateTime DataVendaVv { get; set; } = DateTime.UtcNow;
    public decimal ValorFinalVv { get; set; }

    public Veiculo? Veiculo { get; set; }
    public Cliente? Cliente { get; set; }
    public UsuarioSistema? Vendedor { get; set; }
    public Loja? Loja { get; set; }
    public FormaPagamento? FormaPagamento { get; set; }
}

public class VendaPeca
{
    public int IdVp { get; set; }
    public int IdPecFk { get; set; }
    public int? IdCliFk { get; set; }
    public int QuantidadeVp { get; set; }
    public decimal ValorUnitarioVp { get; set; }
    public DateTime DataVendaVp { get; set; } = DateTime.UtcNow;

    public Peca? Peca { get; set; }
    public Cliente? Cliente { get; set; }
}

public class DuvidaFrequente
{
    public int IdDf { get; set; }
    public string PerguntaDf { get; set; } = string.Empty;
    public string RespostaDf { get; set; } = string.Empty;
}
