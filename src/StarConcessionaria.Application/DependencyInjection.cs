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

namespace StarConcessionaria.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUnidadeService, UnidadeService>();
        services.AddScoped<IVeiculoService, VeiculoService>();
        services.AddScoped<IPropostaService, PropostaService>();
        services.AddScoped<IReservaService, ReservaService>();
        services.AddScoped<IAgendamentoService, AgendamentoService>();
        services.AddScoped<IPecaService, PecaService>();
        services.AddScoped<IContatoService, ContatoService>();
        services.AddScoped<IVendaService, VendaService>();
        return services;
    }
}
