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
        // 1. Verifica se foi passada uma ConnectionString completa pronta (via env ou config)
        var defaultConn = configuration.GetConnectionString("DefaultConnection") 
                          ?? configuration["ConnectionStrings__DefaultConnection"] 
                          ?? configuration["DATABASE_URL"];

        // 2. Tenta capturar variaveis individuais (Bd_Host, etc.) com busca case-insensitive e limpeza de espacos/aspas
        string? ObterValor(params string[] chaves)
        {
            foreach (var chave in chaves)
            {
                var val = configuration[chave];
                if (!string.IsNullOrWhiteSpace(val))
                    return val.Trim().Trim('"', '\'');
            }
            return null;
        }

        var host = ObterValor("Bd_Host", "BD_HOST", "bd_host");
        var password = ObterValor("Bd_Password", "Bd_password", "BD_PASSWORD", "bd_password");

        // Se tiver host E password fornecidos, monta a string
        if (!string.IsNullOrWhiteSpace(host) && !string.IsNullOrWhiteSpace(password))
        {
            var database = ObterValor("Bd_Database", "Bd_database", "BD_DATABASE", "bd_database") ?? "neondb";
            var username = ObterValor("Bd_Username", "Bd_username", "BD_USERNAME", "bd_username") ?? "neondb_owner";
            var sslMode = ObterValor("Bd_SSL_Mode", "Bd_Ssl_Mode", "Bd_ssl_mode", "BD_SSL_MODE") ?? "VerifyFull";
            var channelBinding = ObterValor("Bd_Channel_Binding", "Bd_channel_binding", "BD_CHANNEL_BINDING") ?? "Require";

            return $"Host={host};Database={database};Username={username};Password={password};SSL Mode={sslMode};Channel Binding={channelBinding};";
        }

        // 3. Fallback para DefaultConnection caso Bd_* nao esteja completo
        if (!string.IsNullOrWhiteSpace(defaultConn))
        {
            return defaultConn.Trim().Trim('"', '\'');
        }

        throw new InvalidOperationException("Nenhuma ConnectionString valida configurada. Certifique-se de configurar Bd_Password e Bd_Host, ou ConnectionStrings__DefaultConnection.");
    }

}
