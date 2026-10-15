using StarConcessionaria.Domain.Exceptions;

namespace StarConcessionaria.Domain.Entities;

/// <summary>
/// Entidade de Domínio Pura: FAQ (Frequently Asked Questions)
/// Central de dúvidas frequentes consultadas pelos clientes na plataforma.
/// </summary>
public class Faq
{
    public Guid Id { get; private set; }
    public string Pergunta { get; private set; }
    public string Resposta { get; private set; }
    public string Categoria { get; private set; }
    public int OrdemExibicao { get; private set; }

    public Faq(
        Guid id,
        string pergunta,
        string resposta,
        string categoria,
        int ordemExibicao = 0)
    {
        Id = id == Guid.Empty ? Guid.NewGuid() : id;

        if (string.IsNullOrWhiteSpace(pergunta))
            throw new DomainValidationException("A pergunta é obrigatória.");

        if (string.IsNullOrWhiteSpace(resposta))
            throw new DomainValidationException("A resposta é obrigatória.");

        if (string.IsNullOrWhiteSpace(categoria))
            throw new DomainValidationException("A categoria do FAQ é obrigatória.");

        Pergunta = pergunta.Trim();
        Resposta = resposta.Trim();
        Categoria = categoria.Trim();
        OrdemExibicao = ordemExibicao;
    }
}
