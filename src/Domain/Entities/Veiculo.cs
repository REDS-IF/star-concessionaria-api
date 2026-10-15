using StarConcessionaria.Domain.Enums;
using StarConcessionaria.Domain.Exceptions;

namespace StarConcessionaria.Domain.Entities;

/// <summary>
/// Entidade de Domínio Pura: Veiculo (Vehicle)
/// Representa o ativo principal da concessionária no estoque de veículos novos e seminovos.
/// Não possui acoplamento com decorators de banco de dados ou ORMs.
/// </summary>
public class Veiculo
{
    public Guid Id { get; private set; }
    public string Marca { get; private set; }
    public string Modelo { get; private set; }
    public int Ano { get; private set; }
    public decimal Preco { get; private set; }
    public int Quilometragem { get; private set; }
    public string Cor { get; private set; }
    public List<string> Fotografias { get; private set; }
    public string? Alteracoes { get; private set; }
    public SituacaoVeiculo Situacao { get; private set; }
    public Guid UnidadeId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    /// <summary>
    /// Construtor privado para fabricação interna e recomposição controlada
    /// </summary>
    private Veiculo() 
    { 
        Fotografias = new List<string>();
    }

    /// <summary>
    /// Cria uma nova instância de Veiculo com validações de integridade de domínio.
    /// </summary>
    public Veiculo(
        Guid id,
        string marca,
        string modelo,
        int ano,
        decimal preco,
        int quilometragem,
        string cor,
        List<string>? fotografias,
        string? alteracoes,
        Guid unidadeId,
        SituacaoVeiculo situacao = SituacaoVeiculo.DISPONIVEL,
        DateTime? createdAt = null,
        DateTime? updatedAt = null)
    {
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        ValidarParametros(marca, modelo, ano, preco, quilometragem, cor, unidadeId);

        Marca = marca.Trim();
        Modelo = modelo.Trim();
        Ano = ano;
        Preco = preco;
        Quilometragem = quilometragem;
        Cor = cor.Trim();
        Fotografias = fotografias != null ? new List<string>(fotografias) : new List<string>();
        Alteracoes = alteracoes?.Trim();
        UnidadeId = unidadeId;
        Situacao = situacao;
        CreatedAt = createdAt ?? DateTime.UtcNow;
        UpdatedAt = updatedAt ?? DateTime.UtcNow;
    }

    /// <summary>
    /// Verifica se o veículo está elegível para reserva, visita ou test-drive.
    /// </summary>
    public bool IsDisponivel() => Situacao == SituacaoVeiculo.DISPONIVEL;

    /// <summary>
    /// Aplica a regra de negócio RN01/RN03/RN05:
    /// Apenas veículos com situação DISPONIVEL podem ser reservados.
    /// Se já estiver VENDIDO (RN02), bloqueia com exceção estrita.
    /// </summary>
    public void Reservar()
    {
        if (Situacao == SituacaoVeiculo.VENDIDO)
            throw new VehicleSoldException("Veículo já vendido. Impossível realizar nova reserva.");

        if (Situacao == SituacaoVeiculo.RESERVADO)
            throw new VehicleNotAvailableException("Veículo já se encontra reservado por outro cliente.");

        Situacao = SituacaoVeiculo.RESERVADO;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Libera a reserva do veículo caso tenha expirado ou tenha sido cancelada,
    /// retornando seu status para DISPONIVEL no estoque.
    /// </summary>
    public void LiberarReserva()
    {
        if (Situacao == SituacaoVeiculo.VENDIDO)
            throw new VehicleSoldException("Veículo já foi vendido e não pode ter reserva reaberta.");

        Situacao = SituacaoVeiculo.DISPONIVEL;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Aplica a regra de negócio RN02:
    /// Conclusão da venda do veículo. Uma vez VENDIDO, fica permanentemente indisponível
    /// para reservas, visitas e test-drives na plataforma.
    /// </summary>
    public void MarcarComoVendido()
    {
        Situacao = SituacaoVeiculo.VENDIDO;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Altera a situação do veículo diretamente através de ação gerencial/administrativa.
    /// </summary>
    public void AlterarSituacao(SituacaoVeiculo novaSituacao)
    {
        if (Situacao == SituacaoVeiculo.VENDIDO && novaSituacao != SituacaoVeiculo.VENDIDO)
            throw new VehicleSoldException("Não é permitido reativar um veículo já vendido sem procedimento de estorno formal.");

        Situacao = novaSituacao;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Atualiza os atributos comerciais e descritivos do veículo.
    /// </summary>
    public void AtualizarDados(
        string marca,
        string modelo,
        int ano,
        decimal preco,
        int quilometragem,
        string cor,
        List<string>? fotografias,
        string? alteracoes,
        Guid unidadeId)
    {
        if (Situacao == SituacaoVeiculo.VENDIDO)
            throw new VehicleSoldException("Não é permitido alterar os dados cadastrais de um veículo já vendido.");

        ValidarParametros(marca, modelo, ano, preco, quilometragem, cor, unidadeId);

        Marca = marca.Trim();
        Modelo = modelo.Trim();
        Ano = ano;
        Preco = preco;
        Quilometragem = quilometragem;
        Cor = cor.Trim();
        if (fotografias != null)
            Fotografias = new List<string>(fotografias);
        Alteracoes = alteracoes?.Trim();
        UnidadeId = unidadeId;
        UpdatedAt = DateTime.UtcNow;
    }

    private static void ValidarParametros(
        string marca, 
        string modelo, 
        int ano, 
        decimal preco, 
        int quilometragem, 
        string cor, 
        Guid unidadeId)
    {
        if (string.IsNullOrWhiteSpace(marca))
            throw new DomainValidationException("A marca do veículo é obrigatória.");

        if (string.IsNullOrWhiteSpace(modelo))
            throw new DomainValidationException("O modelo do veículo é obrigatório.");

        if (ano < 1900 || ano > DateTime.UtcNow.Year + 2)
            throw new DomainValidationException($"Ano do veículo ({ano}) fora da faixa permitida.");

        if (preco <= 0)
            throw new DomainValidationException("O preço do veículo deve ser maior que zero.");

        if (quilometragem < 0)
            throw new DomainValidationException("A quilometragem não pode ser negativa.");

        if (string.IsNullOrWhiteSpace(cor))
            throw new DomainValidationException("A cor do veículo é obrigatória.");

        if (unidadeId == Guid.Empty)
            throw new DomainValidationException("A unidade da concessionária deve ser informada.");
    }
}
