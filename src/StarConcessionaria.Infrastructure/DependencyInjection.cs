using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StarConcessionaria.Application.Auth;
using StarConcessionaria.Application.Contatos;
using StarConcessionaria.Application.Pecas;
using StarConcessionaria.Application.Propostas;
using StarConcessionaria.Application.Agendamentos;
using StarConcessionaria.Application.Reservas;
using StarConcessionaria.Application.Unidades;
using StarConcessionaria.Application.Veiculos;
using StarConcessionaria.Application.Vendas;
using StarConcessionaria.Application.Dashboard;
using StarConcessionaria.Application.Institucional;
using StarConcessionaria.Infrastructure.BackgroundJobs;
using StarConcessionaria.Infrastructure.Auth;
using StarConcessionaria.Infrastructure.Persistence;
using StarConcessionaria.Infrastructure.Repositories;

namespace StarConcessionaria.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection nao configurada.");

        services.AddDbContext<ApplicationDbContext>(o => o.UseNpgsql(connectionString));

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.Section));
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddSingleton<ITokenService, TokenService>();
        services.AddScoped<ILojaRepository, LojaRepository>();
        services.AddScoped<IVeiculoRepository, VeiculoRepository>();
        services.AddScoped<IPropostaRepository, PropostaRepository>();
        services.AddScoped<IReservaRepository, ReservaRepository>();
        services.AddScoped<IAgendamentoRepository, AgendamentoRepository>();
        services.AddScoped<IPecaRepository, PecaRepository>();
        services.AddScoped<IContatoRepository, ContatoRepository>();
        services.AddScoped<IVendaRepository, VendaRepository>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IInstitucionalService, InstitucionalService>();
        services.AddHostedService<ReservaExpiracaoWorker>();
        return services;
    }
}
