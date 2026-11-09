using Microsoft.EntityFrameworkCore;
using StarConcessionaria.Domain.Entities;

namespace StarConcessionaria.Infrastructure.Persistence;

public class StarConcessionariaDbContext : DbContext
{
    public StarConcessionariaDbContext(DbContextOptions<StarConcessionariaDbContext> options)
        : base(options)
    {
    }

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
        base.OnModelCreating(modelBuilder);

        // 1. Loja
        modelBuilder.Entity<Loja>(entity =>
        {
            entity.ToTable("Loja");
            entity.HasKey(e => e.IdLoj);
            entity.Property(e => e.IdLoj).HasColumnName("id_loj");
            entity.Property(e => e.NomeLoj).HasColumnName("nome_loj").HasMaxLength(300).IsRequired();
            entity.Property(e => e.TelefoneLoj).HasColumnName("telefone_loj").HasMaxLength(30);
            entity.Property(e => e.CepLoj).HasColumnName("cep_loj").HasMaxLength(15);
            entity.Property(e => e.EnderecoLoj).HasColumnName("endereco_loj").HasMaxLength(300);
            entity.Property(e => e.NumeroLoj).HasColumnName("numero_loj").HasMaxLength(30);
            entity.Property(e => e.ComplementoLoj).HasColumnName("complemento_loj").HasMaxLength(100);
            entity.Property(e => e.BairroLoj).HasColumnName("bairro_loj").HasMaxLength(100);
            entity.Property(e => e.CidadeLoj).HasColumnName("cidade_loj").HasMaxLength(100);
            entity.Property(e => e.EstadoLoj).HasColumnName("estado_loj").HasMaxLength(2);
            entity.Property(e => e.LatitudeLoj).HasColumnName("latitude_loj").HasPrecision(10, 7);
            entity.Property(e => e.LongitudeLoj).HasColumnName("longitude_loj").HasPrecision(10, 7);
        });

        // 2. UsuarioSistema
        modelBuilder.Entity<UsuarioSistema>(entity =>
        {
            entity.ToTable("UsuarioSistema");
            entity.HasKey(e => e.IdUsu);
            entity.Property(e => e.IdUsu).HasColumnName("id_usu");
            entity.Property(e => e.NomeUsu).HasColumnName("nome_usu").HasMaxLength(300).IsRequired();
            entity.Property(e => e.EmailUsu).HasColumnName("email_usu").HasMaxLength(300).IsRequired();
            entity.Property(e => e.SenhaUsu).HasColumnName("senha_usu").HasMaxLength(300).IsRequired();
            entity.Property(e => e.PerfilUsu).HasColumnName("perfil_usu").HasMaxLength(100).IsRequired();
            entity.HasIndex(e => e.EmailUsu).IsUnique();
        });

        // 3. Cliente
        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.ToTable("Cliente");
            entity.HasKey(e => e.IdCli);
            entity.Property(e => e.IdCli).HasColumnName("id_cli");
            entity.Property(e => e.NomeCli).HasColumnName("nome_cli").HasMaxLength(300).IsRequired();
            entity.Property(e => e.EmailCli).HasColumnName("email_cli").HasMaxLength(300).IsRequired();
            entity.Property(e => e.TelefoneCli).HasColumnName("telefone_cli").HasMaxLength(30).IsRequired();
        });

        // 4. FormaPagamento
        modelBuilder.Entity<FormaPagamento>(entity =>
        {
            entity.ToTable("FormaPagamento");
            entity.HasKey(e => e.IdFp);
            entity.Property(e => e.IdFp).HasColumnName("id_fp");
            entity.Property(e => e.NomeFp).HasColumnName("nome_fp").HasMaxLength(100).IsRequired();
        });

        // 5. Veiculo
        modelBuilder.Entity<Veiculo>(entity =>
        {
            entity.ToTable("Veiculo");
            entity.HasKey(e => e.IdVei);
            entity.Property(e => e.IdVei).HasColumnName("id_vei");
            entity.Property(e => e.IdLojFk).HasColumnName("id_loj_fk").IsRequired();
            entity.Property(e => e.TipoVei).HasColumnName("tipo_vei").HasMaxLength(30).IsRequired();
            entity.Property(e => e.MarcaVei).HasColumnName("marca_vei").HasMaxLength(100).IsRequired();
            entity.Property(e => e.ModeloVei).HasColumnName("modelo_vei").HasMaxLength(100).IsRequired();
            entity.Property(e => e.AnoVei).HasColumnName("ano_vei").IsRequired();
            entity.Property(e => e.PrecoVei).HasColumnName("preco_vei").HasPrecision(12, 2).IsRequired();
            entity.Property(e => e.QuilometragemVei).HasColumnName("quilometragem_vei").IsRequired();
            entity.Property(e => e.CorVei).HasColumnName("cor_vei").HasMaxLength(50).IsRequired();
            entity.Property(e => e.DescricaoVei).HasColumnName("descricao_vei");
            entity.Property(e => e.SituacaoVei).HasColumnName("situacao_vei").HasMaxLength(30).IsRequired();

            entity.HasOne(e => e.Loja)
                .WithMany(l => l.Veiculos)
                .HasForeignKey(e => e.IdLojFk)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.SituacaoVei, e.MarcaVei, e.ModeloVei, e.AnoVei, e.PrecoVei });
        });

        // 6. FotoVeiculo
        modelBuilder.Entity<FotoVeiculo>(entity =>
        {
            entity.ToTable("FotoVeiculo");
            entity.HasKey(e => e.IdFot);
            entity.Property(e => e.IdFot).HasColumnName("id_fot");
            entity.Property(e => e.IdVeiFk).HasColumnName("id_vei_fk").IsRequired();
            entity.Property(e => e.UrlFot).HasColumnName("url_fot").HasMaxLength(500).IsRequired();

            entity.HasOne(e => e.Veiculo)
                .WithMany(v => v.Fotos)
                .HasForeignKey(e => e.IdVeiFk)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // 7. AlteracaoVeiculo
        modelBuilder.Entity<AlteracaoVeiculo>(entity =>
        {
            entity.ToTable("AlteracaoVeiculo");
            entity.HasKey(e => e.IdAlt);
            entity.Property(e => e.IdAlt).HasColumnName("id_alt");
            entity.Property(e => e.IdVeiFk).HasColumnName("id_vei_fk").IsRequired();
            entity.Property(e => e.DescricaoAlt).HasColumnName("descricao_alt").IsRequired();

            entity.HasOne(e => e.Veiculo)
                .WithMany(v => v.Alteracoes)
                .HasForeignKey(e => e.IdVeiFk)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // 8. Contato
        modelBuilder.Entity<Contato>(entity =>
        {
            entity.ToTable("Contato");
            entity.HasKey(e => e.IdCon);
            entity.Property(e => e.IdCon).HasColumnName("id_con");
            entity.Property(e => e.IdCliFk).HasColumnName("id_cli_fk");
            entity.Property(e => e.IdVeiFk).HasColumnName("id_vei_fk");
            entity.Property(e => e.IdLojFk).HasColumnName("id_loj_fk");
            entity.Property(e => e.IdUsuFk).HasColumnName("id_usu_fk");
            entity.Property(e => e.TipoCon).HasColumnName("tipo_con").HasMaxLength(100).IsRequired();
            entity.Property(e => e.MensagemCon).HasColumnName("mensagem_con").IsRequired();
            entity.Property(e => e.DataCon).HasColumnName("data_con");

            entity.HasOne(e => e.Cliente)
                .WithMany(c => c.Contatos)
                .HasForeignKey(e => e.IdCliFk)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Veiculo)
                .WithMany(v => v.Contatos)
                .HasForeignKey(e => e.IdVeiFk)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Loja)
                .WithMany(l => l.Contatos)
                .HasForeignKey(e => e.IdLojFk)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Usuario)
                .WithMany(u => u.ContatosAtendidos)
                .HasForeignKey(e => e.IdUsuFk)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // 9. Reserva
        modelBuilder.Entity<Reserva>(entity =>
        {
            entity.ToTable("Reserva");
            entity.HasKey(e => e.IdRes);
            entity.Property(e => e.IdRes).HasColumnName("id_res");
            entity.Property(e => e.IdCliFk).HasColumnName("id_cli_fk").IsRequired();
            entity.Property(e => e.IdVeiFk).HasColumnName("id_vei_fk").IsRequired();
            entity.Property(e => e.DataSolicitacaoRes).HasColumnName("data_solicitacao_res");
            entity.Property(e => e.SituacaoRes).HasColumnName("situacao_res").HasMaxLength(100).IsRequired();

            entity.HasOne(e => e.Cliente)
                .WithMany(c => c.Reservas)
                .HasForeignKey(e => e.IdCliFk)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Veiculo)
                .WithMany(v => v.Reservas)
                .HasForeignKey(e => e.IdVeiFk)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // 10. Agendamento
        modelBuilder.Entity<Agendamento>(entity =>
        {
            entity.ToTable("Agendamento");
            entity.HasKey(e => e.IdAge);
            entity.Property(e => e.IdAge).HasColumnName("id_age");
            entity.Property(e => e.IdCliFk).HasColumnName("id_cli_fk").IsRequired();
            entity.Property(e => e.IdLojFk).HasColumnName("id_loj_fk").IsRequired();
            entity.Property(e => e.IdVeiFk).HasColumnName("id_vei_fk");
            entity.Property(e => e.TipoAge).HasColumnName("tipo_age").HasMaxLength(100).IsRequired();
            entity.Property(e => e.DataHoraInicioAge).HasColumnName("data_hora_inicio_age").IsRequired();
            entity.Property(e => e.DataHoraFimAge).HasColumnName("data_hora_fim_age").IsRequired();
            entity.Property(e => e.SituacaoAge).HasColumnName("situacao_age").HasMaxLength(100).IsRequired();

            entity.HasOne(e => e.Cliente)
                .WithMany(c => c.Agendamentos)
                .HasForeignKey(e => e.IdCliFk)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Loja)
                .WithMany(l => l.Agendamentos)
                .HasForeignKey(e => e.IdLojFk)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Veiculo)
                .WithMany(v => v.Agendamentos)
                .HasForeignKey(e => e.IdVeiFk)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // 11. Peca
        modelBuilder.Entity<Peca>(entity =>
        {
            entity.ToTable("Peca");
            entity.HasKey(e => e.IdPec);
            entity.Property(e => e.IdPec).HasColumnName("id_pec");
            entity.Property(e => e.NomePec).HasColumnName("nome_pec").HasMaxLength(300).IsRequired();
            entity.Property(e => e.DescricaoPec).HasColumnName("descricao_pec");
            entity.Property(e => e.PrecoPec).HasColumnName("preco_pec").HasPrecision(10, 2).IsRequired();
            entity.Property(e => e.QuantidadeEstoquePec).HasColumnName("quantidade_estoque_pec").IsRequired();
        });

        // 12. VendaVeiculo
        modelBuilder.Entity<VendaVeiculo>(entity =>
        {
            entity.ToTable("VendaVeiculo");
            entity.HasKey(e => e.IdVv);
            entity.Property(e => e.IdVv).HasColumnName("id_vv");
            entity.Property(e => e.IdVeiFk).HasColumnName("id_vei_fk").IsRequired();
            entity.Property(e => e.IdCliFk).HasColumnName("id_cli_fk").IsRequired();
            entity.Property(e => e.IdVendedorFk).HasColumnName("id_vendedor_fk").IsRequired();
            entity.Property(e => e.IdLojFk).HasColumnName("id_loj_fk").IsRequired();
            entity.Property(e => e.IdFpFk).HasColumnName("id_fp_fk").IsRequired();
            entity.Property(e => e.DataVendaVv).HasColumnName("data_venda_vv");
            entity.Property(e => e.ValorFinalVv).HasColumnName("valor_final_vv").HasPrecision(12, 2).IsRequired();

            entity.HasIndex(e => e.IdVeiFk).IsUnique();

            entity.HasOne(e => e.Veiculo)
                .WithOne(v => v.Venda)
                .HasForeignKey<VendaVeiculo>(e => e.IdVeiFk)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Cliente)
                .WithMany(c => c.ComprasVeiculo)
                .HasForeignKey(e => e.IdCliFk)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Vendedor)
                .WithMany(u => u.VendasRealizadas)
                .HasForeignKey(e => e.IdVendedorFk)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Loja)
                .WithMany(l => l.Vendas)
                .HasForeignKey(e => e.IdLojFk)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.FormaPagamento)
                .WithMany(fp => fp.Vendas)
                .HasForeignKey(e => e.IdFpFk)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // 13. VendaPeca
        modelBuilder.Entity<VendaPeca>(entity =>
        {
            entity.ToTable("VendaPeca");
            entity.HasKey(e => e.IdVp);
            entity.Property(e => e.IdVp).HasColumnName("id_vp");
            entity.Property(e => e.IdPecFk).HasColumnName("id_pec_fk").IsRequired();
            entity.Property(e => e.IdCliFk).HasColumnName("id_cli_fk");
            entity.Property(e => e.QuantidadeVp).HasColumnName("quantidade_vp").IsRequired();
            entity.Property(e => e.ValorUnitarioVp).HasColumnName("valor_unitario_vp").HasPrecision(10, 2).IsRequired();
            entity.Property(e => e.DataVendaVp).HasColumnName("data_venda_vp");

            entity.HasOne(e => e.Peca)
                .WithMany(p => p.Vendas)
                .HasForeignKey(e => e.IdPecFk)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Cliente)
                .WithMany(c => c.ComprasPeca)
                .HasForeignKey(e => e.IdCliFk)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // 14. DuvidaFrequente
        modelBuilder.Entity<DuvidaFrequente>(entity =>
        {
            entity.ToTable("DuvidaFrequente");
            entity.HasKey(e => e.IdDf);
            entity.Property(e => e.IdDf).HasColumnName("id_df");
            entity.Property(e => e.PerguntaDf).HasColumnName("pergunta_df").IsRequired();
            entity.Property(e => e.RespostaDf).HasColumnName("resposta_df").IsRequired();
        });
    }
}
