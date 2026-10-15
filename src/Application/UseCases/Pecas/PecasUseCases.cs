using StarConcessionaria.Application.DTOs;
using StarConcessionaria.Domain.Entities;
using StarConcessionaria.Domain.Exceptions;
using StarConcessionaria.Domain.Repositories;

namespace StarConcessionaria.Application.UseCases.Pecas;

/// <summary>
/// Caso de Uso: Catálogo público e Gestão de Peças e Acessórios.
/// </summary>
public class GerenciarPecasUseCase
{
    private readonly IPecaRepository _pecaRepository;

    public GerenciarPecasUseCase(IPecaRepository pecaRepository)
    {
        _pecaRepository = pecaRepository;
    }

    public async Task<IEnumerable<PecaResponseDto>> ListarCatalogoAsync(
        bool? apenasDisponiveis = null, 
        string? termoBusca = null, 
        CancellationToken cancellationToken = default)
    {
        var pecas = await _pecaRepository.ListarAsync(apenasDisponiveis, termoBusca, cancellationToken);
        return pecas.Select(PecaResponseDto.FromEntity);
    }

    public async Task<PecaResponseDto> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var peca = await _pecaRepository.ObterPorIdAsync(id, cancellationToken)
            ?? throw new EntityNotFoundException("Peça", id);

        return PecaResponseDto.FromEntity(peca);
    }

    public async Task<PecaResponseDto> CadastrarAsync(CadastrarOuAtualizarPecaRequestDto request, CancellationToken cancellationToken = default)
    {
        var pecaExistente = await _pecaRepository.ObterPorCodigoAsync(request.Codigo, cancellationToken);
        if (pecaExistente != null)
            throw new DomainValidationException($"Já existe uma peça cadastrada com o código '{request.Codigo}'.");

        var peca = new Peca(
            id: Guid.NewGuid(),
            nome: request.Nome,
            codigo: request.Codigo,
            preco: request.Preco,
            quantidadeEstoque: request.QuantidadeEstoque,
            descricao: request.Descricao
        );

        await _pecaRepository.AdicionarAsync(peca, cancellationToken);
        return PecaResponseDto.FromEntity(peca);
    }

    public async Task<PecaResponseDto> AtualizarAsync(Guid id, CadastrarOuAtualizarPecaRequestDto request, CancellationToken cancellationToken = default)
    {
        var peca = await _pecaRepository.ObterPorIdAsync(id, cancellationToken)
            ?? throw new EntityNotFoundException("Peça", id);

        var outraPeca = await _pecaRepository.ObterPorCodigoAsync(request.Codigo, cancellationToken);
        if (outraPeca != null && outraPeca.Id != id)
            throw new DomainValidationException($"O código '{request.Codigo}' já pertence a outra peça.");

        peca.AtualizarDados(
            nome: request.Nome,
            codigo: request.Codigo,
            preco: request.Preco,
            quantidadeEstoque: request.QuantidadeEstoque,
            descricao: request.Descricao
        );

        await _pecaRepository.AtualizarAsync(peca, cancellationToken);
        return PecaResponseDto.FromEntity(peca);
    }

    public async Task<PecaResponseDto> AjustarEstoqueAsync(Guid id, AjustarEstoquePecaRequestDto request, CancellationToken cancellationToken = default)
    {
        var peca = await _pecaRepository.ObterPorIdAsync(id, cancellationToken)
            ?? throw new EntityNotFoundException("Peça", id);

        if (request.IsBaixa)
            peca.BaixarEstoque(request.Quantidade);
        else
            peca.ReporEstoque(request.Quantidade);

        await _pecaRepository.AtualizarAsync(peca, cancellationToken);
        return PecaResponseDto.FromEntity(peca);
    }

    public async Task RemoverAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var existe = await _pecaRepository.ObterPorIdAsync(id, cancellationToken);
        if (existe == null)
            throw new EntityNotFoundException("Peça", id);

        await _pecaRepository.RemoverAsync(id, cancellationToken);
    }
}
