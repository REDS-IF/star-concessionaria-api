namespace StarConcessionaria.Infrastructure.Persistence.Models;

/// <summary>
/// Modelo de persistência para a tabela/coleção de Veículos.
/// Totalmente isolado da Entidade de Domínio Veiculo.
/// </summary>
public class VeiculoModel
{
    public Guid Id { get; set; }
    public string Marca { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public int Ano { get; set; }
    public decimal Preco { get; set; }
    public int Quilometragem { get; set; }
    public string Cor { get; set; } = string.Empty;
    public string FotografiasJson { get; set; } = "[]";
    public string? Alteracoes { get; set; }
    public int Situacao { get; set; }
    public Guid UnidadeId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Modelo de persistência para Unidades / Lojas Físicas.
/// </summary>
public class UnidadeModel
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Endereco { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string HorarioFuncionamento { get; set; } = string.Empty;
}

/// <summary>
/// Modelo de persistência para Reservas.
/// </summary>
public class ReservaModel
{
    public Guid Id { get; set; }
    public Guid VeiculoId { get; set; }
    public Guid? ClienteId { get; set; }
    public string ClienteNome { get; set; } = string.Empty;
    public string ClienteEmail { get; set; } = string.Empty;
    public string ClienteTelefone { get; set; } = string.Empty;
    public DateTime DataReserva { get; set; }
    public int Status { get; set; }
    public string? MotivoCancelamento { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Modelo de persistência para Agendamentos.
/// </summary>
public class AgendamentoModel
{
    public Guid Id { get; set; }
    public int Tipo { get; set; }
    public Guid? VeiculoId { get; set; }
    public Guid UnidadeId { get; set; }
    public string ClienteNome { get; set; } = string.Empty;
    public string ClienteEmail { get; set; } = string.Empty;
    public string ClienteTelefone { get; set; } = string.Empty;
    public DateTime DataHora { get; set; }
    public int Status { get; set; }
    public string? Observacoes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Modelo de persistência para Propostas e Contatos.
/// </summary>
public class PropostaContatoModel
{
    public Guid Id { get; set; }
    public int Tipo { get; set; }
    public Guid? VeiculoId { get; set; }
    public string ClienteNome { get; set; } = string.Empty;
    public string ClienteEmail { get; set; } = string.Empty;
    public string ClienteTelefone { get; set; } = string.Empty;
    public int? FormaPagamentoDesejada { get; set; }
    public string Mensagem { get; set; } = string.Empty;
    public int StatusAtendimento { get; set; }
    public string? RespostaAtendente { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Modelo de persistência para Peças.
/// </summary>
public class PecaModel
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    public int QuantidadeEstoque { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Modelo de persistência para Usuários.
/// </summary>
public class UsuarioModel
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;
    public int Perfil { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Modelo de persistência para FAQ.
/// </summary>
public class FaqModel
{
    public Guid Id { get; set; }
    public string Pergunta { get; set; } = string.Empty;
    public string Resposta { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public int OrdemExibicao { get; set; }
}
