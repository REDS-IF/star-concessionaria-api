using StarConcessionaria.Application.DTOs;
using StarConcessionaria.Domain.Entities;
using StarConcessionaria.Domain.Enums;
using StarConcessionaria.Domain.Exceptions;
using StarConcessionaria.Domain.Repositories;

namespace StarConcessionaria.Application.UseCases.Veiculos;

/// <summary>
/// Caso de Uso: Catálogo e Detalhes de Veículos (Área Pública do Cliente e Administrativa).
/// Permite filtragem combinada e detalhamento completo de fotos e especificações.
/// </summary>
public class ConsultarVeiculosUseCase
{
    private readonly IVeiculoRepository _veiculoRepository;
    private readonly IUnidadeRepository _unidadeRepository;

    public ConsultarVeiculosUseCase(IVeiculoRepository veiculoRepository, IUnidadeRepository unidadeRepository)
    {
        _veiculoRepository = veiculoRepository;
        _unidadeRepository = unidadeRepository;
    }

    public async Task<IEnumerable<VeiculoResponseDto>> ListarCatalogoAsync(
        string? marca = null,
        string? modelo = null,
        int? anoMin = null,
        int? anoMax = null,
        decimal? precoMin = null,
        decimal? precoMax = null,
        SituacaoVeiculo? situacao = null,
        Guid? unidadeId = null,
        CancellationToken cancellationToken = default)
    {
        var veiculos = await _veiculoRepository.ListarAsync(
            marca, modelo, anoMin, anoMax, precoMin, precoMax, situacao, unidadeId, cancellationToken);

        var unidades = (await _unidadeRepository.ListarTodasAsync(cancellationToken)).ToDictionary(u => u.Id, u => u.Nome);

        return veiculos.Select(v => 
            VeiculoResponseDto.FromEntity(v, unidades.TryGetValue(v.UnidadeId, out var nomeLoja) ? nomeLoja : null));
    }

    public async Task<VeiculoResponseDto> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var veiculo = await _veiculoRepository.ObterPorIdAsync(id, cancellationToken)
            ?? throw new EntityNotFoundException("Veículo", id);

        var unidade = await _unidadeRepository.ObterPorIdAsync(veiculo.UnidadeId, cancellationToken);

        return VeiculoResponseDto.FromEntity(veiculo, unidade?.Nome);
    }
}

/// <summary>
/// Caso de Uso: Gestão de Veículos (Cadastro, Atualização e Alteração de Situação).
/// Acesso restrito via RBAC (Comercial, Logística, Administrador).
/// </summary>
public class GerenciarVeiculosUseCase
{
    private readonly IVeiculoRepository _veiculoRepository;
    private readonly IUnidadeRepository _unidadeRepository;

    public GerenciarVeiculosUseCase(IVeiculoRepository veiculoRepository, IUnidadeRepository unidadeRepository)
    {
        _veiculoRepository = veiculoRepository;
        _unidadeRepository = unidadeRepository;
    }

    public async Task<VeiculoResponseDto> CadastrarAsync(CadastrarVeiculoRequestDto request, CancellationToken cancellationToken = default)
    {
        var unidade = await _unidadeRepository.ObterPorIdAsync(request.UnidadeId, cancellationToken)
            ?? throw new EntityNotFoundException("Unidade da concessionária", request.UnidadeId);

        var veiculo = new Veiculo(
            id: Guid.NewGuid(),
            marca: request.Marca,
            modelo: request.Modelo,
            ano: request.Ano,
            preco: request.Preco,
            quilometragem: request.Quilometragem,
            cor: request.Cor,
            fotografias: request.Fotografias,
            alteracoes: request.Alteracoes,
            unidadeId: request.UnidadeId,
            situacao: SituacaoVeiculo.DISPONIVEL
        );

        await _veiculoRepository.AdicionarAsync(veiculo, cancellationToken);

        return VeiculoResponseDto.FromEntity(veiculo, unidade.Nome);
    }

    public async Task<VeiculoResponseDto> AtualizarAsync(Guid id, AtualizarVeiculoRequestDto request, CancellationToken cancellationToken = default)
    {
        var veiculo = await _veiculoRepository.ObterPorIdAsync(id, cancellationToken)
            ?? throw new EntityNotFoundException("Veículo", id);

        var unidade = await _unidadeRepository.ObterPorIdAsync(request.UnidadeId, cancellationToken)
            ?? throw new EntityNotFoundException("Unidade da concessionária", request.UnidadeId);

        veiculo.AtualizarDados(
            marca: request.Marca,
            modelo: request.Modelo,
            ano: request.Ano,
            preco: request.Preco,
            quilometragem: request.Quilometragem,
            cor: request.Cor,
            fotografias: request.Fotografias,
            alteracoes: request.Alteracoes,
            unidadeId: request.UnidadeId
        );

        await _veiculoRepository.AtualizarAsync(veiculo, cancellationToken);

        return VeiculoResponseDto.FromEntity(veiculo, unidade.Nome);
    }

    public async Task<VeiculoResponseDto> AlterarSituacaoAsync(Guid id, AlterarSituacaoVeiculoRequestDto request, CancellationToken cancellationToken = default)
    {
        var veiculo = await _veiculoRepository.ObterPorIdAsync(id, cancellationToken)
            ?? throw new EntityNotFoundException("Veículo", id);

        // Aplica validação de domínio para transição de status (ex: RN02)
        veiculo.AlterarSituacao(request.Situacao);

        await _veiculoRepository.AtualizarAsync(veiculo, cancellationToken);

        var unidade = await _unidadeRepository.ObterPorIdAsync(veiculo.UnidadeId, cancellationToken);
        return VeiculoResponseDto.FromEntity(veiculo, unidade?.Nome);
    }
}
