namespace StarConcessionaria.Application.Common;

public record PagedResult<T>(IReadOnlyList<T> Itens, int Pagina, int ItensPorPagina, int TotalItens)
{
    public int TotalPaginas => ItensPorPagina <= 0 ? 0 : (int)Math.Ceiling(TotalItens / (double)ItensPorPagina);
}
