using System.Text.Json;
using StarConcessionaria.Domain.Entities;
using StarConcessionaria.Domain.Enums;
using StarConcessionaria.Domain.ValueObjects;
using StarConcessionaria.Infrastructure.Persistence.Models;

namespace StarConcessionaria.Infrastructure.Persistence.Mappers;

/// <summary>
/// Mappers de Isolamento Arquitetural:
/// Converte entre Entidades de Domínio puras e Modelos de Persistência/Banco de Dados.
/// Garante que o Domínio nunca conheça o formato físico de persistência.
/// </summary>
public static class DomainPersistenceMappers
{
    #region Veiculo Mapper
    public static Veiculo ToDomain(this VeiculoModel model)
    {
        var fotografias = string.IsNullOrWhiteSpace(model.FotografiasJson)
            ? new List<string>()
            : JsonSerializer.Deserialize<List<string>>(model.FotografiasJson) ?? new List<string>();

        return new Veiculo(
            id: model.Id,
            marca: model.Marca,
            modelo: model.Modelo,
            ano: model.Ano,
            preco: model.Preco,
            quilometragem: model.Quilometragem,
            cor: model.Cor,
            fotografias: fotografias,
            alteracoes: model.Alteracoes,
            unidadeId: model.UnidadeId,
            situacao: (SituacaoVeiculo)model.Situacao,
            createdAt: model.CreatedAt,
            updatedAt: model.UpdatedAt
        );
    }

    public static VeiculoModel ToModel(this Veiculo entity)
    {
        return new VeiculoModel
        {
            Id = entity.Id,
            Marca = entity.Marca,
            Modelo = entity.Modelo,
            Ano = entity.Ano,
            Preco = entity.Preco,
            Quilometragem = entity.Quilometragem,
            Cor = entity.Cor,
            FotografiasJson = JsonSerializer.Serialize(entity.Fotografias),
            Alteracoes = entity.Alteracoes,
            Situacao = (int)entity.Situacao,
            UnidadeId = entity.UnidadeId,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
    #endregion

    #region Unidade Mapper
    public static Unidade ToDomain(this UnidadeModel model)
    {
        return new Unidade(
            id: model.Id,
            nome: model.Nome,
            endereco: model.Endereco,
            cidade: model.Cidade,
            estado: model.Estado,
            telefone: model.Telefone,
            horarioFuncionamento: model.HorarioFuncionamento
        );
    }

    public static UnidadeModel ToModel(this Unidade entity)
    {
        return new UnidadeModel
        {
            Id = entity.Id,
            Nome = entity.Nome,
            Endereco = entity.Endereco,
            Cidade = entity.Cidade,
            Estado = entity.Estado,
            Telefone = entity.Telefone,
            HorarioFuncionamento = entity.HorarioFuncionamento
        };
    }
    #endregion

    #region Reserva Mapper
    public static Reserva ToDomain(this ReservaModel model)
    {
        var dadosCliente = new DadosCliente(model.ClienteNome, model.ClienteEmail, model.ClienteTelefone);

        return new Reserva(
            id: model.Id,
            veiculoId: model.VeiculoId,
            dadosCliente: dadosCliente,
            clienteId: model.ClienteId,
            dataReserva: model.DataReserva,
            status: (StatusReserva)model.Status,
            motivoCancelamento: model.MotivoCancelamento,
            createdAt: model.CreatedAt,
            updatedAt: model.UpdatedAt
        );
    }

    public static ReservaModel ToModel(this Reserva entity)
    {
        return new ReservaModel
        {
            Id = entity.Id,
            VeiculoId = entity.VeiculoId,
            ClienteId = entity.ClienteId,
            ClienteNome = entity.DadosCliente.Nome,
            ClienteEmail = entity.DadosCliente.Email,
            ClienteTelefone = entity.DadosCliente.Telefone,
            DataReserva = entity.DataReserva,
            Status = (int)entity.Status,
            MotivoCancelamento = entity.MotivoCancelamento,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
    #endregion

    #region Agendamento Mapper
    public static Agendamento ToDomain(this AgendamentoModel model)
    {
        var dadosCliente = new DadosCliente(model.ClienteNome, model.ClienteEmail, model.ClienteTelefone);

        return new Agendamento(
            id: model.Id,
            tipo: (TipoAgendamento)model.Tipo,
            veiculoId: model.VeiculoId,
            unidadeId: model.UnidadeId,
            dadosCliente: dadosCliente,
            dataHora: model.DataHora,
            status: (StatusAgendamento)model.Status,
            observacoes: model.Observacoes,
            createdAt: model.CreatedAt,
            updatedAt: model.UpdatedAt
        );
    }

    public static AgendamentoModel ToModel(this Agendamento entity)
    {
        return new AgendamentoModel
        {
            Id = entity.Id,
            Tipo = (int)entity.Tipo,
            VeiculoId = entity.VeiculoId,
            UnidadeId = entity.UnidadeId,
            ClienteNome = entity.DadosCliente.Nome,
            ClienteEmail = entity.DadosCliente.Email,
            ClienteTelefone = entity.DadosCliente.Telefone,
            DataHora = entity.DataHora,
            Status = (int)entity.Status,
            Observacoes = entity.Observacoes,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
    #endregion

    #region PropostaContato Mapper
    public static PropostaContato ToDomain(this PropostaContatoModel model)
    {
        var dadosCliente = new DadosCliente(model.ClienteNome, model.ClienteEmail, model.ClienteTelefone);

        return new PropostaContato(
            id: model.Id,
            tipo: (TipoPropostaContato)model.Tipo,
            dadosCliente: dadosCliente,
            mensagem: model.Mensagem,
            veiculoId: model.VeiculoId,
            formaPagamentoDesejada: model.FormaPagamentoDesejada.HasValue ? (FormaPagamento)model.FormaPagamentoDesejada.Value : null,
            statusAtendimento: (StatusAtendimento)model.StatusAtendimento,
            respostaAtendente: model.RespostaAtendente,
            createdAt: model.CreatedAt,
            updatedAt: model.UpdatedAt
        );
    }

    public static PropostaContatoModel ToModel(this PropostaContato entity)
    {
        return new PropostaContatoModel
        {
            Id = entity.Id,
            Tipo = (int)entity.Tipo,
            VeiculoId = entity.VeiculoId,
            ClienteNome = entity.DadosCliente.Nome,
            ClienteEmail = entity.DadosCliente.Email,
            ClienteTelefone = entity.DadosCliente.Telefone,
            FormaPagamentoDesejada = entity.FormaPagamentoDesejada.HasValue ? (int)entity.FormaPagamentoDesejada.Value : null,
            Mensagem = entity.Mensagem,
            StatusAtendimento = (int)entity.StatusAtendimento,
            RespostaAtendente = entity.RespostaAtendente,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
    #endregion

    #region Peca Mapper
    public static Peca ToDomain(this PecaModel model)
    {
        return new Peca(
            id: model.Id,
            nome: model.Nome,
            codigo: model.Codigo,
            preco: model.Preco,
            quantidadeEstoque: model.QuantidadeEstoque,
            descricao: model.Descricao,
            createdAt: model.CreatedAt,
            updatedAt: model.UpdatedAt
        );
    }

    public static PecaModel ToModel(this Peca entity)
    {
        return new PecaModel
        {
            Id = entity.Id,
            Nome = entity.Nome,
            Codigo = entity.Codigo,
            Preco = entity.Preco,
            QuantidadeEstoque = entity.QuantidadeEstoque,
            Descricao = entity.Descricao,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
    #endregion

    #region Usuario Mapper
    public static Usuario ToDomain(this UsuarioModel model)
    {
        return new Usuario(
            id: model.Id,
            nome: model.Nome,
            email: model.Email,
            senhaHash: model.SenhaHash,
            perfil: (PerfilUsuario)model.Perfil,
            createdAt: model.CreatedAt
        );
    }

    public static UsuarioModel ToModel(this Usuario entity)
    {
        return new UsuarioModel
        {
            Id = entity.Id,
            Nome = entity.Nome,
            Email = entity.Email,
            SenhaHash = entity.SenhaHash,
            Perfil = (int)entity.Perfil,
            CreatedAt = entity.CreatedAt
        };
    }
    #endregion

    #region Faq Mapper
    public static Faq ToDomain(this FaqModel model)
    {
        return new Faq(
            id: model.Id,
            pergunta: model.Pergunta,
            resposta: model.Resposta,
            categoria: model.Categoria,
            ordemExibicao: model.OrdemExibicao
        );
    }

    public static FaqModel ToModel(this Faq entity)
    {
        return new FaqModel
        {
            Id = entity.Id,
            Pergunta = entity.Pergunta,
            Resposta = entity.Resposta,
            Categoria = entity.Categoria,
            OrdemExibicao = entity.OrdemExibicao
        };
    }
    #endregion
}
