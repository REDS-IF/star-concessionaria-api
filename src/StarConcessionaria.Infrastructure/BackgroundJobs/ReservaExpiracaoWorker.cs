using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StarConcessionaria.Application.Reservas;
using StarConcessionaria.Domain.Entities;
using StarConcessionaria.Infrastructure.Persistence;

namespace StarConcessionaria.Infrastructure.BackgroundJobs;

/// <summary>
/// Worker em background que verifica periodicamente reservas Pendentes ha mais de 48h
/// e as marca como 'Cancelada', liberando o veiculo de volta para 'Disponivel'.
/// </summary>
public class ReservaExpiracaoWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ReservaExpiracaoWorker> _logger;
    private static readonly TimeSpan IntervaloExecucao = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan LimiteExpiracao = TimeSpan.FromHours(48);

    public ReservaExpiracaoWorker(IServiceProvider serviceProvider, ILogger<ReservaExpiracaoWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ReservaExpiracaoWorker iniciado.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ExpirarReservasVencidasAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar expiracao de reservas.");
            }

            await Task.Delay(IntervaloExecucao, stoppingToken);
        }
    }

    private async Task ExpirarReservasVencidasAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var limite = DateTimeOffset.UtcNow - LimiteExpiracao;
        var vencidas = await db.Reservas
            .Include(r => r.Veiculo)
            .Where(r => r.Situacao == SituacaoReserva.Pendente && r.DataSolicitacao <= limite)
            .ToListAsync(ct);

        if (vencidas.Count == 0) return;

        foreach (var r in vencidas)
        {
            r.Situacao = SituacaoReserva.Cancelada;
            if (r.Veiculo != null && r.Veiculo.Situacao == SituacaoVeiculo.Reservado)
            {
                r.Veiculo.Situacao = SituacaoVeiculo.Disponivel;
                db.AlteracoesVeiculo.Add(new AlteracaoVeiculo
                {
                    VeiculoId = r.Veiculo.Id,
                    Descricao = $"Reserva #{r.Id} expirada automaticamente (prazo de 48h excedido). Veiculo retornou para 'Disponivel'."
                });
            }
        }

        await db.SaveChangesAsync(ct);
        _logger.LogInformation("{Count} reserva(s) pendente(s) expirada(s) com sucesso.", vencidas.Count);
    }
}
