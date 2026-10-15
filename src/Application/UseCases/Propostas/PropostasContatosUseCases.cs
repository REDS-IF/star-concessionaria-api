using StarConcessionaria.Application.DTOs;
using StarConcessionaria.Domain.Entities;
using StarConcessionaria.Domain.Enums;
using StarConcessionaria.Domain.Exceptions;
using StarConcessionaria.Domain.Repositories;
using StarConcessionaria.Domain.ValueObjects;

namespace StarConcessionaria.Application.UseCases.Propostas;

/// <summary>
/// Caso de Uso: Registro de Proposta de Compra de Veículo (Lead Comercial).
/// Aplica a regra RN06: Início do processo de compra. A API registra a proposta e
/// gera um lead para o vendedor na concessionária. A negociação e a formalização do contrato
/// ocorrem de maneira presencial fora do sistema web.
/// </summary>
public class CriarPropostaCompraUseCase
{
    private readonly IPropostaContatoRepository _propostaRepository;
    private readonly IVeiculoRepository _veiculoRepository;

    public CriarPropostaCompraUseCase(
        IPropostaContatoRepository propostaRepository, 
        IVeiculoRepository veiculoRepository)
    {
        _propostaRepository = propostaRepository;
        _veiculoRepository = veiculoRepository;
    }

    public async Task<PropostaContatoResponseDto> ExecutarAsync(
        CriarPropostaCompraRequestDto request, 
        CancellationToken cancellationToken = default)
    {
        // 1. Valida existência do veículo
        var veiculo = await _veiculoRepository.ObterPorIdAsync(request.VeiculoId, cancellationToken)
            ?? throw new EntityNotFoundException("Veículo", request.VeiculoId);

        // Se o veículo estiver VENDIDO, bloqueia proposta (RN02)
        if (veiculo.Situacao == SituacaoVeiculo.VENDIDO)
            throw new VehicleSoldException("O veículo solicitado já se encontra vendido.");

        // 2. Constrói o objeto de valor DadosCliente
        var dadosCliente = new DadosCliente(request.NomeCliente, request.EmailCliente, request.TelefoneCliente);

        // 3. Cria a proposta com forma de pagamento pretendida
        var proposta = new PropostaContato(
            id: Guid.NewGuid(),
            tipo: TipoPropostaContato.PROPOSTA_COMPRA,
            dadosCliente: dadosCliente,
            mensagem: request.Mensagem,
            veiculoId: veiculo.Id,
            formaPagamentoDesejada: request.FormaPagamentoDesejada,
            statusAtendimento: StatusAtendimento.NOVO
        );

        // 4. Persiste no repositório desacoplado
        await _propostaRepository.AdicionarAsync(proposta, cancellationToken);

        return PropostaContatoResponseDto.FromEntity(proposta, $"{veiculo.Marca} {veiculo.Modelo}");
    }
}

/// <summary>
/// Caso de Uso: Envio de Mensagem de Contato / Dúvidas Gerais do Cliente.
/// </summary>
public class CriarContatoDuvidaUseCase
{
    private readonly IPropostaContatoRepository _propostaRepository;
    private readonly IVeiculoRepository _veiculoRepository;

    public CriarContatoDuvidaUseCase(
        IPropostaContatoRepository propostaRepository, 
        IVeiculoRepository veiculoRepository)
    {
        _propostaRepository = propostaRepository;
        _veiculoRepository = veiculoRepository;
    }

    public async Task<PropostaContatoResponseDto> ExecutarAsync(
        CriarContatoDuvidaRequestDto request, 
        CancellationToken cancellationToken = default)
    {
        Veiculo? veiculo = null;
        if (request.VeiculoId.HasValue && request.VeiculoId != Guid.Empty)
        {
            veiculo = await _veiculoRepository.ObterPorIdAsync(request.VeiculoId.Value, cancellationToken);
        }

        var dadosCliente = new DadosCliente(request.NomeCliente, request.EmailCliente, request.TelefoneCliente);

        var contato = new PropostaContato(
            id: Guid.NewGuid(),
            tipo: TipoPropostaContato.SOLICITACAO_INFORMACOES,
            dadosCliente: dadosCliente,
            mensagem: request.Mensagem,
            veiculoId: request.VeiculoId,
            formaPagamentoDesejada: null,
            statusAtendimento: StatusAtendimento.NOVO
        );

        await _propostaRepository.AdicionarAsync(contato, cancellationToken);

        return PropostaContatoResponseDto.FromEntity(
            contato, 
            veiculo != null ? $"{veiculo.Marca} {veiculo.Modelo}" : null);
    }
}

/// <summary>
/// Caso de Uso: Gestão Administrativa de Leads, Propostas e Contatos Comerciais.
/// </summary>
public class GerenciarContatosAdminUseCase
{
    private readonly IPropostaContatoRepository _propostaRepository;
    private readonly IVeiculoRepository _veiculoRepository;

    public GerenciarContatosAdminUseCase(
        IPropostaContatoRepository propostaRepository, 
        IVeiculoRepository veiculoRepository)
    {
        _propostaRepository = propostaRepository;
        _veiculoRepository = veiculoRepository;
    }

    public async Task<IEnumerable<PropostaContatoResponseDto>> ListarAsync(
        TipoPropostaContato? tipo = null,
        StatusAtendimento? status = null,
        Guid? veiculoId = null,
        CancellationToken cancellationToken = default)
    {
        var lista = await _propostaRepository.ListarAsync(tipo, status, veiculoId, cancellationToken);
        var dtos = new List<PropostaContatoResponseDto>();

        foreach (var item in lista)
        {
            var veiculo = item.VeiculoId.HasValue ? await _veiculoRepository.ObterPorIdAsync(item.VeiculoId.Value, cancellationToken) : null;
            dtos.Add(PropostaContatoResponseDto.FromEntity(
                item, 
                veiculo != null ? $"{veiculo.Marca} {veiculo.Modelo}" : null));
        }

        return dtos;
    }

    public async Task<PropostaContatoResponseDto> ResponderAsync(
        Guid id, 
        ResponderPropostaContatoRequestDto request, 
        CancellationToken cancellationToken = default)
    {
        var proposta = await _propostaRepository.ObterPorIdAsync(id, cancellationToken)
            ?? throw new EntityNotFoundException("Proposta/Contato", id);

        switch (request.NovoStatus)
        {
            case StatusAtendimento.EM_ATENDIMENTO:
                proposta.IniciarAtendimento();
                break;
            case StatusAtendimento.FINALIZADO:
                proposta.FinalizarAtendimento(request.Resposta);
                break;
        }

        await _propostaRepository.AtualizarAsync(proposta, cancellationToken);

        var veiculo = proposta.VeiculoId.HasValue ? await _veiculoRepository.ObterPorIdAsync(proposta.VeiculoId.Value, cancellationToken) : null;
        return PropostaContatoResponseDto.FromEntity(proposta, veiculo != null ? $"{veiculo.Marca} {veiculo.Modelo}" : null);
    }
}
