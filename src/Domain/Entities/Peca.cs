using StarConcessionaria.Domain.Exceptions;

namespace StarConcessionaria.Domain.Entities;

/// <summary>
/// Entidade de Domínio Pura: Peca (AutoPart)
/// Representa peças de reposição e acessórios automotivos comercializados pela Star Concessionária.
/// </summary>
public class Peca
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; }
    public string Codigo { get; private set; }
    public decimal Preco { get; private set; }
    public int QuantidadeEstoque { get; private set; }
    public bool Disponivel => QuantidadeEstoque > 0;
    public string Descricao { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public Peca(
        Guid id,
        string nome,
        string codigo,
        decimal preco,
        int quantidadeEstoque,
        string descricao,
        DateTime? createdAt = null,
        DateTime? updatedAt = null)
    {
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        ValidarParametros(nome, codigo, preco, quantidadeEstoque, descricao);

        Nome = nome.Trim();
        Codigo = codigo.Trim().ToUpperInvariant();
        Preco = preco;
        QuantidadeEstoque = quantidadeEstoque;
        Descricao = descricao.Trim();
        CreatedAt = createdAt ?? DateTime.UtcNow;
        UpdatedAt = updatedAt ?? DateTime.UtcNow;
    }

    /// <summary>
    /// Dá baixa em unidades vendidas ou utilizadas no estoque.
    /// </summary>
    public void BaixarEstoque(int quantidade)
    {
        if (quantidade <= 0)
            throw new DomainValidationException("A quantidade para baixa de estoque deve ser positiva.");

        if (quantidade > QuantidadeEstoque)
            throw new InsufficientStockException($"Estoque insuficiente para a peça '{Nome}'. Disponível: {QuantidadeEstoque}, Solicitado: {quantidade}.");

        QuantidadeEstoque -= quantidade;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Repõe o estoque da peça no almoxarifado/loja.
    /// </summary>
    public void ReporEstoque(int quantidade)
    {
        if (quantidade <= 0)
            throw new DomainValidationException("A quantidade para reposição deve ser positiva.");

        QuantidadeEstoque += quantidade;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Atualiza dados cadastrais da peça.
    /// </summary>
    public void AtualizarDados(string nome, string codigo, decimal preco, int quantidadeEstoque, string descricao)
    {
        ValidarParametros(nome, codigo, preco, quantidadeEstoque, descricao);

        Nome = nome.Trim();
        Codigo = codigo.Trim().ToUpperInvariant();
        Preco = preco;
        QuantidadeEstoque = quantidadeEstoque;
        Descricao = descricao.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    private static void ValidarParametros(string nome, string codigo, decimal preco, int quantidadeEstoque, string descricao)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainValidationException("O nome da peça é obrigatório.");

        if (string.IsNullOrWhiteSpace(codigo))
            throw new DomainValidationException("O código da peça é obrigatório.");

        if (preco <= 0)
            throw new DomainValidationException("O preço da peça deve ser maior que zero.");

        if (quantidadeEstoque < 0)
            throw new DomainValidationException("A quantidade em estoque não pode ser negativa.");

        if (string.IsNullOrWhiteSpace(descricao))
            throw new DomainValidationException("A descrição da peça é obrigatória.");
    }
}
