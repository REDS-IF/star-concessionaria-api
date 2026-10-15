using StarConcessionaria.Application.DTOs;
using StarConcessionaria.Domain.Repositories;

namespace StarConcessionaria.Application.UseCases.Unidades;

/// <summary>
/// Caso de Uso: Listagem de lojas físicas da Star Concessionária (Área Pública do Cliente).
/// </summary>
public class ListarUnidadesUseCase
{
    private readonly IUnidadeRepository _unidadeRepository;

    public ListarUnidadesUseCase(IUnidadeRepository unidadeRepository)
    {
        _unidadeRepository = unidadeRepository;
    }

    public async Task<IEnumerable<UnidadeResponseDto>> ExecutarAsync(CancellationToken cancellationToken = default)
    {
        var unidades = await _unidadeRepository.ListarTodasAsync(cancellationToken);
        return unidades.Select(UnidadeResponseDto.FromEntity);
    }
}
