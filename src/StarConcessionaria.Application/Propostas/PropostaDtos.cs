using System.ComponentModel.DataAnnotations;

namespace StarConcessionaria.Application.Propostas;

public record SolicitarPropostaRequest(
    [Range(1, int.MaxValue)] int VeiculoId,
    /// <summary>Opcional; por padrao usa a loja onde o veiculo esta.</summary>
    int? UnidadeId,
    [Required, StringLength(300)] string NomeCliente,
    [Required, EmailAddress, StringLength(300)] string EmailCliente,
    [Required, StringLength(30, MinimumLength = 8)] string TelefoneCliente,
    [Required, StringLength(100)] string FormaPagamento,
    bool PossuiTroca,
    [StringLength(1000)] string? DadosTroca,
    [Range(0, 9999999999.99)] decimal? ValorEntrada);

public record PropostaResponse(
    string Protocolo, int Id, int VeiculoId, int UnidadeId, string NomeCliente, string EmailCliente,
    string TelefoneCliente, string FormaPagamento, bool PossuiTroca, decimal? ValorEntrada,
    DateTimeOffset DataSolicitacao, string Mensagem);
