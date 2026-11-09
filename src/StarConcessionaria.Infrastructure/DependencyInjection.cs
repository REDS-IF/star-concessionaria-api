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
        var connectionString = ObterConnectionString(configuration);

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

    private static string ObterConnectionString(IConfiguration configuration)
    {
        // 1. Prioriza montar via variaveis individuais configuradas no Render (Bd_Host, Bd_Username, etc.)
        var host = configuration["Bd_Host"];
        if (!string.IsNullOrWhiteSpace(host))
        {
            var database = configuration["Bd_Database"] ?? configuration["Bd_database"] ?? "neondb";
            var username = configuration["Bd_Username"] ?? configuration["Bd_username"];
            var password = configuration["Bd_Password"] ?? configuration["Bd_password"];
            var sslMode = configuration["Bd_SSL_Mode"] ?? configuration["Bd_Ssl_Mode"] ?? configuration["Bd_ssl_mode"] ?? "VerifyFull";
            var channelBinding = configuration["Bd_Channel_Binding"] ?? configuration["Bd_channel_binding"] ?? "Require";

            return $"Host={host};Database={database};Username={username};Password={password};SSL Mode={sslMode};Channel Binding={channelBinding};";
        }

        // 2. Se nao encontrar as variaveis separadas, utiliza o padrao ConnectionStrings:DefaultConnection
        return configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Nenhuma ConnectionString configurada (verifique as variaveis Bd_Host/Bd_Username/Bd_Password ou ConnectionStrings:DefaultConnection).");
    }

}
