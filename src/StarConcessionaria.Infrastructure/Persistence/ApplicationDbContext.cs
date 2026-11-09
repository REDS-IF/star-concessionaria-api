using Microsoft.EntityFrameworkCore;
using StarConcessionaria.Domain.Entities;

namespace StarConcessionaria.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Loja> Lojas => Set<Loja>();
    public DbSet<UsuarioSistema> Usuarios => Set<UsuarioSistema>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<FormaPagamento> FormasPagamento => Set<FormaPagamento>();
    public DbSet<Veiculo> Veiculos => Set<Veiculo>();
    public DbSet<FotoVeiculo> FotosVeiculo => Set<FotoVeiculo>();
    public DbSet<AlteracaoVeiculo> AlteracoesVeiculo => Set<AlteracaoVeiculo>();
    public DbSet<Contato> Contatos => Set<Contato>();
    public DbSet<Reserva> Reservas => Set<Reserva>();
    public DbSet<Agendamento> Agendamentos => Set<Agendamento>();
    public DbSet<Peca> Pecas => Set<Peca>();
    public DbSet<VendaVeiculo> VendasVeiculo => Set<VendaVeiculo>();
    public DbSet<VendaPeca> VendasPeca => Set<VendaPeca>();
    public DbSet<DuvidaFrequente> DuvidasFrequentes => Set<DuvidaFrequente>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
