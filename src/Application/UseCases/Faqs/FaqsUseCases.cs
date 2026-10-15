using StarConcessionaria.Application.DTOs;
using StarConcessionaria.Domain.Repositories;

namespace StarConcessionaria.Application.UseCases.Faqs;

/// <summary>
/// Caso de Uso: Central de Dúvidas Frequentes (FAQ) para os clientes.
/// </summary>
public class ListarFaqsUseCase
{
    private readonly IFaqRepository _faqRepository;

    public ListarFaqsUseCase(IFaqRepository faqRepository)
    {
        _faqRepository = faqRepository;
    }

    public async Task<IEnumerable<FaqResponseDto>> ExecutarAsync(CancellationToken cancellationToken = default)
    {
        var faqs = await _faqRepository.ListarTodasAsync(cancellationToken);
        return faqs.OrderBy(f => f.OrdemExibicao).Select(FaqResponseDto.FromEntity);
    }
}
