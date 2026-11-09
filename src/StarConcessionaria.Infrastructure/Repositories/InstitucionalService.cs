using Microsoft.EntityFrameworkCore;
using StarConcessionaria.Application.Institucional;
using StarConcessionaria.Infrastructure.Persistence;

namespace StarConcessionaria.Infrastructure.Repositories;

public class InstitucionalService : IInstitucionalService
{
    private readonly ApplicationDbContext _db;

    public InstitucionalService(ApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<FormaPagamentoDetalheResponse>> ListarFormasPagamentoAsync(CancellationToken ct = default)
    {
        var formas = await _db.FormasPagamento.AsNoTracking().OrderBy(f => f.Id).ToListAsync(ct);
        return formas.Select(f => new FormaPagamentoDetalheResponse(
            f.Id, f.Nome,
            f.Nome.ToLowerInvariant() switch
            {
                "a vista" => "Pagamento integral via TED/Pix na entrega do veículo.",
                "financiamento" => "Planos em até 60x através de bancos parceiros credenciados.",
                "consorcio" => "Aceitamos cartas de crédito contempladas de qualquer administradora.",
                "cartao de credito" => "Parcelamento da entrada em até 12x no cartão de crédito.",
                "pix" => "Pagamento instantâneo para reserva ou sinal.",
                _ => "Condição de pagamento aceita mediante análise comercial."
            },
            f.Nome.ToLowerInvariant() == "pix")).ToList();
    }

    public async Task<IReadOnlyList<FaqResponse>> ListarFaqsAsync(string? busca, CancellationToken ct = default)
    {
        var query = _db.DuvidasFrequentes.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(busca))
        {
            var p = "%" + busca.Trim().Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_") + "%";
            query = query.Where(f => EF.Functions.ILike(f.Pergunta, p) || EF.Functions.ILike(f.Resposta, p));
        }

        var faqs = await query.OrderBy(f => f.Id).ToListAsync(ct);
        return faqs.Select(f => new FaqResponse(f.Id, f.Pergunta, f.Resposta)).ToList();
    }
}
