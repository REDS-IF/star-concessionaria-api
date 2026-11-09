namespace StarConcessionaria.Application.Dashboard;

public record VeiculosPorLojaDto(int LojaId, string LojaNome, int QuantidadeDisponivel);
public record PecaMaisVendidaDto(int PecaId, string Nome, int QuantidadeVendida, decimal TotalFaturado);

public record IndicadoresGerenciaisResponse(
    int TotalVeiculosDisponiveis,
    int TotalVeiculosReservados,
    int TotalVeiculosVendidos,
    int TotalReservasAtivas,
    int TotalAgendamentosPendentes,
    int TotalContatosNaoRespondidos,
    int TotalVendasVeiculos,
    decimal ValorTotalVendasVeiculos,
    IReadOnlyList<VeiculosPorLojaDto> VeiculosDisponiveisPorLoja,
    IReadOnlyList<PecaMaisVendidaDto> TopPecasMaisVendidas);
