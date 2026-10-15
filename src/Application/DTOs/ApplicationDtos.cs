using StarConcessionaria.Domain.Entities;
using StarConcessionaria.Domain.Enums;

namespace StarConcessionaria.Application.DTOs;

#region Auth DTOs
public record LoginRequestDto(string Email, string Senha);

public record LoginResponseDto(
    Guid Id,
    string Nome,
    string Email,
    string Perfil,
    string Token,
    DateTime ExpiraEm);
#endregion

#region Veiculo DTOs
public record VeiculoResponseDto(
    Guid Id,
    string Marca,
    string Modelo,
    int Ano,
    decimal Preco,
    int Quilometragem,
    string Cor,
    List<string> Fotografias,
    string? Alteracoes,
    string Situacao,
    Guid UnidadeId,
    string? NomeUnidade,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    public static VeiculoResponseDto FromEntity(Veiculo v, string? nomeUnidade = null) =>
        new(
            v.Id,
            v.Marca,
            v.Modelo,
            v.Ano,
            v.Preco,
            v.Quilometragem,
            v.Cor,
            v.Fotografias,
            v.Alteracoes,
            v.Situacao.ToString(),
            v.UnidadeId,
            nomeUnidade,
            v.CreatedAt,
            v.UpdatedAt);
}

public record CadastrarVeiculoRequestDto(
    string Marca,
    string Modelo,
    int Ano,
    decimal Preco,
    int Quilometragem,
    string Cor,
    List<string>? Fotografias,
    string? Alteracoes,
    Guid UnidadeId);

public record AtualizarVeiculoRequestDto(
    string Marca,
    string Modelo,
    int Ano,
    decimal Preco,
    int Quilometragem,
    string Cor,
    List<string>? Fotografias,
    string? Alteracoes,
    Guid UnidadeId);

public record AlterarSituacaoVeiculoRequestDto(SituacaoVeiculo Situacao);
#endregion

#region Unidade DTOs
public record UnidadeResponseDto(
    Guid Id,
    string Nome,
    string Endereco,
    string Cidade,
    string Estado,
    string Telefone,
    string HorarioFuncionamento)
{
    public static UnidadeResponseDto FromEntity(Unidade u) =>
        new(u.Id, u.Nome, u.Endereco, u.Cidade, u.Estado, u.Telefone, u.HorarioFuncionamento);
}
#endregion

#region Reserva DTOs
public record SolicitarReservaRequestDto(
    string NomeCliente,
    string EmailCliente,
    string TelefoneCliente);

public record ReservaResponseDto(
    Guid Id,
    Guid VeiculoId,
    string? ModeloVeiculo,
    Guid? ClienteId,
    string NomeCliente,
    string EmailCliente,
    string TelefoneCliente,
    DateTime DataReserva,
    string Status,
    string? MotivoCancelamento,
    DateTime CreatedAt)
{
    public static ReservaResponseDto FromEntity(Reserva r, string? modeloVeiculo = null) =>
        new(
            r.Id,
            r.VeiculoId,
            modeloVeiculo,
            r.ClienteId,
            r.DadosCliente.Nome,
            r.DadosCliente.Email,
            r.DadosCliente.Telefone,
            r.DataReserva,
            r.Status.ToString(),
            r.MotivoCancelamento,
            r.CreatedAt);
}

public record AtualizarStatusReservaRequestDto(
    StatusReserva NovoStatus,
    string? MotivoCancelamento);
#endregion

#region Agendamento DTOs
public record CriarAgendamentoRequestDto(
    TipoAgendamento Tipo,
    Guid? VeiculoId,
    Guid UnidadeId,
    string NomeCliente,
    string EmailCliente,
    string TelefoneCliente,
    DateTime DataHora,
    string? Observacoes);

public record AgendamentoResponseDto(
    Guid Id,
    string Tipo,
    Guid? VeiculoId,
    string? ModeloVeiculo,
    Guid UnidadeId,
    string? NomeUnidade,
    string NomeCliente,
    string EmailCliente,
    string TelefoneCliente,
    DateTime DataHora,
    string Status,
    string? Observacoes,
    DateTime CreatedAt)
{
    public static AgendamentoResponseDto FromEntity(Agendamento a, string? modeloVeiculo = null, string? nomeUnidade = null) =>
        new(
            a.Id,
            a.Tipo.ToString(),
            a.VeiculoId,
            modeloVeiculo,
            a.UnidadeId,
            nomeUnidade,
            a.DadosCliente.Nome,
            a.DadosCliente.Email,
            a.DadosCliente.Telefone,
            a.DataHora,
            a.Status.ToString(),
            a.Observacoes,
            a.CreatedAt);
}

public record AtualizarStatusAgendamentoRequestDto(
    StatusAgendamento NovoStatus,
    string? Motivo);
#endregion

#region Proposta / Contato DTOs
public record CriarPropostaCompraRequestDto(
    Guid VeiculoId,
    string NomeCliente,
    string EmailCliente,
    string TelefoneCliente,
    FormaPagamento FormaPagamentoDesejada,
    string Mensagem);

public record CriarContatoDuvidaRequestDto(
    string NomeCliente,
    string EmailCliente,
    string TelefoneCliente,
    string Mensagem,
    Guid? VeiculoId);

public record PropostaContatoResponseDto(
    Guid Id,
    string Tipo,
    Guid? VeiculoId,
    string? ModeloVeiculo,
    string NomeCliente,
    string EmailCliente,
    string TelefoneCliente,
    string? FormaPagamentoDesejada,
    string Mensagem,
    string StatusAtendimento,
    string? RespostaAtendente,
    DateTime CreatedAt)
{
    public static PropostaContatoResponseDto FromEntity(PropostaContato p, string? modeloVeiculo = null) =>
        new(
            p.Id,
            p.Tipo.ToString(),
            p.VeiculoId,
            modeloVeiculo,
            p.DadosCliente.Nome,
            p.DadosCliente.Email,
            p.DadosCliente.Telefone,
            p.FormaPagamentoDesejada?.ToString(),
            p.Mensagem,
            p.StatusAtendimento.ToString(),
            p.RespostaAtendente,
            p.CreatedAt);
}

public record ResponderPropostaContatoRequestDto(
    StatusAtendimento NovoStatus,
    string? Resposta);
#endregion

#region Peca DTOs
public record PecaResponseDto(
    Guid Id,
    string Nome,
    string Codigo,
    decimal Preco,
    int QuantidadeEstoque,
    bool Disponivel,
    string Descricao,
    DateTime CreatedAt)
{
    public static PecaResponseDto FromEntity(Peca p) =>
        new(p.Id, p.Nome, p.Codigo, p.Preco, p.QuantidadeEstoque, p.Disponivel, p.Descricao, p.CreatedAt);
}

public record CadastrarOuAtualizarPecaRequestDto(
    string Nome,
    string Codigo,
    decimal Preco,
    int QuantidadeEstoque,
    string Descricao);

public record AjustarEstoquePecaRequestDto(
    int Quantidade,
    bool IsBaixa);
#endregion

#region FAQ DTOs
public record FaqResponseDto(
    Guid Id,
    string Pergunta,
    string Resposta,
    string Categoria,
    int OrdemExibicao)
{
    public static FaqResponseDto FromEntity(Faq f) =>
        new(f.Id, f.Pergunta, f.Resposta, f.Categoria, f.OrdemExibicao);
}
#endregion

#region Gerencial Dashboard DTOs
public record VeiculosPorSituacaoDto(int Disponiveis, int Reservados, int Vendidos, int Total);
public record VeiculosPorLojaDto(Guid UnidadeId, string NomeLoja, int Quantidade);
public record PecasMaisVendidasDto(string Nome, string Codigo, decimal Preco, int EstoqueAtual);

public record DashboardGerencialDto(
    VeiculosPorSituacaoDto VeiculosPorSituacao,
    List<VeiculosPorLojaDto> VeiculosPorLoja,
    int TotalContatosRecebidos,
    int TotalContatosNovos,
    int TotalReservasAtivas,
    int TotalHistoricoVendas,
    List<PecasMaisVendidasDto> PecasDestaque);
#endregion
