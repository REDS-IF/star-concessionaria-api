using StarConcessionaria.Domain.Enums;
using StarConcessionaria.Domain.Exceptions;
using StarConcessionaria.Domain.ValueObjects;

namespace StarConcessionaria.Domain.Entities;

/// <summary>
/// Entidade de Domínio Pura: PropostaContato (Lead / Commercial Proposal)
/// Implementa a regra RN06: Registra a intenção de compra ou dúvida do cliente e gera
/// um lead qualificado para a equipe de vendas. A conclusão da negociação ocorre
/// presencialmente com o vendedor e nunca diretamente online.
/// </summary>
public class PropostaContato
{
    public Guid Id { get; private set; }
    public TipoPropostaContato Tipo { get; private set; }
    public Guid? VeiculoId { get; private set; }
    public DadosCliente DadosCliente { get; private set; }
    public FormaPagamento? FormaPagamentoDesejada { get; private set; }
    public string Mensagem { get; private set; }
    public StatusAtendimento StatusAtendimento { get; private set; }
    public string? RespostaAtendente { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public PropostaContato(
        Guid id,
        TipoPropostaContato tipo,
        DadosCliente dadosCliente,
        string mensagem,
        Guid? veiculoId = null,
        FormaPagamento? formaPagamentoDesejada = null,
        StatusAtendimento statusAtendimento = StatusAtendimento.NOVO,
        string? respostaAtendente = null,
        DateTime? createdAt = null,
        DateTime? updatedAt = null)
    {
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        DadosCliente = dadosCliente ?? throw new DomainValidationException("Os dados de contato do cliente são obrigatórios.");

        if (string.IsNullOrWhiteSpace(mensagem))
            throw new DomainValidationException("A mensagem ou observação da proposta/contato é obrigatória.");

        // Se for Proposta de Compra, o veículo de interesse deve estar vinculado
        if (tipo == TipoPropostaContato.PROPOSTA_COMPRA && (!veiculoId.HasValue || veiculoId == Guid.Empty))
            throw new DomainValidationException("Para registrar proposta de compra, o veículo pretendido deve ser informado.");

        Tipo = tipo;
        VeiculoId = veiculoId;
        FormaPagamentoDesejada = formaPagamentoDesejada;
        Mensagem = mensagem.Trim();
        StatusAtendimento = statusAtendimento;
        RespostaAtendente = respostaAtendente?.Trim();
        CreatedAt = createdAt ?? DateTime.UtcNow;
        UpdatedAt = updatedAt ?? DateTime.UtcNow;
    }

    /// <summary>
    /// Marca o lead como capturado pela equipe comercial para início de negociação.
    /// </summary>
    public void IniciarAtendimento()
    {
        StatusAtendimento = StatusAtendimento.EM_ATENDIMENTO;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Finaliza o atendimento do lead após contato com o cliente.
    /// </summary>
    public void FinalizarAtendimento(string? resposta)
    {
        StatusAtendimento = StatusAtendimento.FINALIZADO;
        if (!string.IsNullOrWhiteSpace(resposta))
            RespostaAtendente = resposta.Trim();

        UpdatedAt = DateTime.UtcNow;
    }
}
