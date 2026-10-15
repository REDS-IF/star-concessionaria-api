using StarConcessionaria.Application.DTOs;
using StarConcessionaria.Domain.Enums;
using StarConcessionaria.Domain.Repositories;

namespace StarConcessionaria.Application.UseCases.Dashboard;

/// <summary>
/// Caso de Uso: Consolidação de Métricas e Indicadores da Área Gerencial.
/// Atende ao endpoint GET /gerencial/dashboard.
/// </summary>
public class ObterDashboardGerencialUseCase
{
    private readonly IVeiculoRepository _veiculoRepository;
    private readonly IUnidadeRepository _unidadeRepository;
    private readonly IReservaRepository _reservaRepository;
    private readonly IPropostaContatoRepository _propostaRepository;
    private readonly IPecaRepository _pecaRepository;

    public ObterDashboardGerencialUseCase(
        IVeiculoRepository veiculoRepository,
        IUnidadeRepository unidadeRepository,
        IReservaRepository reservaRepository,
        IPropostaContatoRepository propostaRepository,
        IPecaRepository pecaRepository)
    {
        _veiculoRepository = veiculoRepository;
        _unidadeRepository = unidadeRepository;
        _reservaRepository = reservaRepository;
        _propostaRepository = propostaRepository;
        _pecaRepository = pecaRepository;
    }

    public async Task<DashboardGerencialDto> ExecutarAsync(CancellationToken cancellationToken = default)
    {
        // 1. Veículos por situação
        var todosVeiculos = (await _veiculoRepository.ListarAsync(cancellationToken: cancellationToken)).ToList();
        var disponiveis = todosVeiculos.Count(v => v.Situacao == SituacaoVeiculo.DISPONIVEL);
        var reservados = todosVeiculos.Count(v => v.Situacao == SituacaoVeiculo.RESERVADO);
        var vendidos = todosVeiculos.Count(v => v.Situacao == SituacaoVeiculo.VENDIDO);
        var totalVeiculos = todosVeiculos.Count;

        var veiculosPorSituacao = new VeiculosPorSituacaoDto(disponiveis, reservados, vendidos, totalVeiculos);

        // 2. Veículos por loja física
        var unidades = await _unidadeRepository.ListarTodasAsync(cancellationToken);
        var veiculosPorLoja = unidades.Select(u => new VeiculosPorLojaDto(
            u.Id,
            u.Nome,
            todosVeiculos.Count(v => v.UnidadeId == u.Id)
        )).ToList();

        // 3. Contatos recebidos
        var contatos = (await _propostaRepository.ListarAsync(cancellationToken: cancellationToken)).ToList();
        var totalContatos = contatos.Count;
        var totalContatosNovos = contatos.Count(c => c.StatusAtendimento == StatusAtendimento.NOVO);

        // 4. Reservas ativas
        var reservas = await _reservaRepository.ListarTodasAsync(cancellationToken: cancellationToken);
        var totalReservasAtivas = reservas.Count(r => r.IsAtiva());

        // 5. Histórico de vendas
        var totalHistoricoVendas = vendidos;

        // 6. Peças em destaque
        var pecas = (await _pecaRepository.ListarAsync(cancellationToken: cancellationToken))
            .OrderBy(p => p.QuantidadeEstoque)
            .Take(10)
            .Select(p => new PecasMaisVendidasDto(p.Nome, p.Codigo, p.Preco, p.QuantidadeEstoque))
            .ToList();

        return new DashboardGerencialDto(
            VeiculosPorSituacao: veiculosPorSituacao,
            VeiculosPorLoja: veiculosPorLoja,
            TotalContatosRecebidos: totalContatos,
            TotalContatosNovos: totalContatosNovos,
            TotalReservasAtivas: totalReservasAtivas,
            TotalHistoricoVendas: totalHistoricoVendas,
            PecasDestaque: pecas
        );
    }
}
