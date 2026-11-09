using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StarConcessionaria.Domain.Entities;

namespace StarConcessionaria.Infrastructure.Persistence;

// Mapeamento Fluent API fiel ao schema oficial em docs/database/star_postgres.sql
// (nomes de tabelas/colunas em minusculas, como o PostgreSQL normaliza identificadores sem aspas).

public class LojaConfiguration : IEntityTypeConfiguration<Loja>
{
    public void Configure(EntityTypeBuilder<Loja> b)
    {
        b.ToTable("loja");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id_loj").ValueGeneratedOnAdd();
        b.Property(x => x.Nome).HasColumnName("nome_loj").HasMaxLength(300).IsRequired();
        b.Property(x => x.Telefone).HasColumnName("telefone_loj").HasMaxLength(30);
        b.Property(x => x.Cep).HasColumnName("cep_loj").HasMaxLength(15);
        b.Property(x => x.Endereco).HasColumnName("endereco_loj").HasMaxLength(300);
        b.Property(x => x.Numero).HasColumnName("numero_loj").HasMaxLength(30);
        b.Property(x => x.Complemento).HasColumnName("complemento_loj").HasMaxLength(100);
        b.Property(x => x.Bairro).HasColumnName("bairro_loj").HasMaxLength(100);
        b.Property(x => x.Cidade).HasColumnName("cidade_loj").HasMaxLength(100);
        b.Property(x => x.Estado).HasColumnName("estado_loj").HasColumnType("character(2)");
        b.Property(x => x.Latitude).HasColumnName("latitude_loj").HasPrecision(10, 7);
        b.Property(x => x.Longitude).HasColumnName("longitude_loj").HasPrecision(10, 7);
    }
}

public class UsuarioSistemaConfiguration : IEntityTypeConfiguration<UsuarioSistema>
{
    public void Configure(EntityTypeBuilder<UsuarioSistema> b)
    {
        b.ToTable("usuariosistema");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id_usu").ValueGeneratedOnAdd();
        b.Property(x => x.Nome).HasColumnName("nome_usu").HasMaxLength(300).IsRequired();
        b.Property(x => x.Email).HasColumnName("email_usu").HasMaxLength(300).IsRequired();
        b.Property(x => x.SenhaHash).HasColumnName("senha_usu").HasMaxLength(300).IsRequired();
        b.Property(x => x.Perfil).HasColumnName("perfil_usu").HasMaxLength(100).IsRequired();
        b.HasIndex(x => x.Email).IsUnique();
    }
}

public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> b)
    {
        b.ToTable("cliente");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id_cli").ValueGeneratedOnAdd();
        b.Property(x => x.Nome).HasColumnName("nome_cli").HasMaxLength(300).IsRequired();
        b.Property(x => x.Email).HasColumnName("email_cli").HasMaxLength(300).IsRequired();
        b.Property(x => x.Telefone).HasColumnName("telefone_cli").HasMaxLength(30).IsRequired();
    }
}

public class FormaPagamentoConfiguration : IEntityTypeConfiguration<FormaPagamento>
{
    public void Configure(EntityTypeBuilder<FormaPagamento> b)
    {
        b.ToTable("formapagamento");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id_fp").ValueGeneratedOnAdd();
        b.Property(x => x.Nome).HasColumnName("nome_fp").HasMaxLength(100).IsRequired();
        b.HasIndex(x => x.Nome).IsUnique();
    }
}

public class VeiculoConfiguration : IEntityTypeConfiguration<Veiculo>
{
    public void Configure(EntityTypeBuilder<Veiculo> b)
    {
        b.ToTable("veiculo");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id_vei").ValueGeneratedOnAdd();
        b.Property(x => x.LojaId).HasColumnName("id_loj_fk");
        b.Property(x => x.Tipo).HasColumnName("tipo_vei").HasMaxLength(30).IsRequired();
        b.Property(x => x.Marca).HasColumnName("marca_vei").HasMaxLength(100).IsRequired();
        b.Property(x => x.Modelo).HasColumnName("modelo_vei").HasMaxLength(100).IsRequired();
        b.Property(x => x.Ano).HasColumnName("ano_vei");
        b.Property(x => x.Preco).HasColumnName("preco_vei").HasPrecision(12, 2);
        b.Property(x => x.Quilometragem).HasColumnName("quilometragem_vei");
        b.Property(x => x.Cor).HasColumnName("cor_vei").HasMaxLength(50).IsRequired();
        b.Property(x => x.Descricao).HasColumnName("descricao_vei").HasColumnType("text");
        b.Property(x => x.Situacao).HasColumnName("situacao_vei").HasMaxLength(30).IsRequired().HasDefaultValue(SituacaoVeiculo.Disponivel);

        b.HasOne(x => x.Loja).WithMany(l => l.Veiculos).HasForeignKey(x => x.LojaId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.Situacao, x.Marca, x.Modelo, x.Ano, x.Preco }).HasDatabaseName("IX_Veiculo_Filtros");
        b.HasIndex(x => x.LojaId).HasDatabaseName("IX_Veiculo_Loja");
    }
}

public class FotoVeiculoConfiguration : IEntityTypeConfiguration<FotoVeiculo>
{
    public void Configure(EntityTypeBuilder<FotoVeiculo> b)
    {
        b.ToTable("fotoveiculo");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id_fot").ValueGeneratedOnAdd();
        b.Property(x => x.VeiculoId).HasColumnName("id_vei_fk");
        b.Property(x => x.Url).HasColumnName("url_fot").HasMaxLength(500).IsRequired();
        b.Property(x => x.Principal).HasColumnName("principal_fot").HasDefaultValue(false);
        b.Property(x => x.Ordem).HasColumnName("ordem_fot").HasDefaultValue(0);
        // Garante no banco: apenas uma foto principal por veiculo.
        b.HasIndex(x => x.VeiculoId).IsUnique().HasFilter("principal_fot").HasDatabaseName("UX_FotoVeiculo_Principal");
        b.HasOne(x => x.Veiculo).WithMany(v => v.Fotos).HasForeignKey(x => x.VeiculoId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class AlteracaoVeiculoConfiguration : IEntityTypeConfiguration<AlteracaoVeiculo>
{
    public void Configure(EntityTypeBuilder<AlteracaoVeiculo> b)
    {
        b.ToTable("alteracaoveiculo");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id_alt").ValueGeneratedOnAdd();
        b.Property(x => x.VeiculoId).HasColumnName("id_vei_fk");
        b.Property(x => x.Descricao).HasColumnName("descricao_alt").HasColumnType("text").IsRequired();
        b.HasOne(x => x.Veiculo).WithMany(v => v.Alteracoes).HasForeignKey(x => x.VeiculoId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class ContatoConfiguration : IEntityTypeConfiguration<Contato>
{
    public void Configure(EntityTypeBuilder<Contato> b)
    {
        b.ToTable("contato");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id_con").ValueGeneratedOnAdd();
        b.Property(x => x.ClienteId).HasColumnName("id_cli_fk");
        b.Property(x => x.VeiculoId).HasColumnName("id_vei_fk");
        b.Property(x => x.LojaId).HasColumnName("id_loj_fk");
        b.Property(x => x.UsuarioId).HasColumnName("id_usu_fk");
        b.Property(x => x.Tipo).HasColumnName("tipo_con").HasMaxLength(100).IsRequired();
        b.Property(x => x.Mensagem).HasColumnName("mensagem_con").HasColumnType("text").IsRequired();
        b.Property(x => x.Data).HasColumnName("data_con").HasDefaultValueSql("CURRENT_TIMESTAMP");

        b.HasOne(x => x.Cliente).WithMany().HasForeignKey(x => x.ClienteId).OnDelete(DeleteBehavior.SetNull);
        b.HasOne(x => x.Veiculo).WithMany().HasForeignKey(x => x.VeiculoId).OnDelete(DeleteBehavior.SetNull);
        b.HasOne(x => x.Loja).WithMany().HasForeignKey(x => x.LojaId).OnDelete(DeleteBehavior.SetNull);
        b.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.SetNull);
    }
}

public class ReservaConfiguration : IEntityTypeConfiguration<Reserva>
{
    public void Configure(EntityTypeBuilder<Reserva> b)
    {
        b.ToTable("reserva");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id_res").ValueGeneratedOnAdd();
        b.Property(x => x.ClienteId).HasColumnName("id_cli_fk");
        b.Property(x => x.VeiculoId).HasColumnName("id_vei_fk");
        b.Property(x => x.DataSolicitacao).HasColumnName("data_solicitacao_res").HasDefaultValueSql("CURRENT_TIMESTAMP");
        b.Property(x => x.Situacao).HasColumnName("situacao_res").HasMaxLength(100).IsRequired().HasDefaultValue("Pendente");

        b.HasOne(x => x.Cliente).WithMany().HasForeignKey(x => x.ClienteId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Veiculo).WithMany().HasForeignKey(x => x.VeiculoId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.VeiculoId, x.Situacao }).HasDatabaseName("IX_Reserva_Veiculo");
    }
}

public class AgendamentoConfiguration : IEntityTypeConfiguration<Agendamento>
{
    public void Configure(EntityTypeBuilder<Agendamento> b)
    {
        b.ToTable("agendamento");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id_age").ValueGeneratedOnAdd();
        b.Property(x => x.ClienteId).HasColumnName("id_cli_fk");
        b.Property(x => x.LojaId).HasColumnName("id_loj_fk");
        b.Property(x => x.VeiculoId).HasColumnName("id_vei_fk");
        b.Property(x => x.Tipo).HasColumnName("tipo_age").HasMaxLength(100).IsRequired();
        b.Property(x => x.DataHoraInicio).HasColumnName("data_hora_inicio_age");
        b.Property(x => x.DataHoraFim).HasColumnName("data_hora_fim_age");
        b.Property(x => x.Situacao).HasColumnName("situacao_age").HasMaxLength(100).IsRequired().HasDefaultValue("Solicitado");

        b.HasOne(x => x.Cliente).WithMany().HasForeignKey(x => x.ClienteId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Loja).WithMany().HasForeignKey(x => x.LojaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Veiculo).WithMany().HasForeignKey(x => x.VeiculoId).OnDelete(DeleteBehavior.SetNull);
        b.HasIndex(x => new { x.VeiculoId, x.DataHoraInicio, x.DataHoraFim }).HasDatabaseName("IX_Agendamento_Veiculo_Horario");
    }
}

public class PecaConfiguration : IEntityTypeConfiguration<Peca>
{
    public void Configure(EntityTypeBuilder<Peca> b)
    {
        b.ToTable("peca");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id_pec").ValueGeneratedOnAdd();
        b.Property(x => x.Nome).HasColumnName("nome_pec").HasMaxLength(300).IsRequired();
        b.Property(x => x.Descricao).HasColumnName("descricao_pec").HasColumnType("text");
        b.Property(x => x.Preco).HasColumnName("preco_pec").HasPrecision(10, 2);
        b.Property(x => x.QuantidadeEstoque).HasColumnName("quantidade_estoque_pec").HasDefaultValue(0);
    }
}

public class VendaVeiculoConfiguration : IEntityTypeConfiguration<VendaVeiculo>
{
    public void Configure(EntityTypeBuilder<VendaVeiculo> b)
    {
        b.ToTable("vendaveiculo");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id_vv").ValueGeneratedOnAdd();
        b.Property(x => x.VeiculoId).HasColumnName("id_vei_fk");
        b.Property(x => x.ClienteId).HasColumnName("id_cli_fk");
        b.Property(x => x.VendedorId).HasColumnName("id_vendedor_fk");
        b.Property(x => x.LojaId).HasColumnName("id_loj_fk");
        b.Property(x => x.FormaPagamentoId).HasColumnName("id_fp_fk");
        b.Property(x => x.DataVenda).HasColumnName("data_venda_vv").HasDefaultValueSql("CURRENT_TIMESTAMP");
        b.Property(x => x.ValorFinal).HasColumnName("valor_final_vv").HasPrecision(12, 2);

        b.HasOne(x => x.Veiculo).WithOne().HasForeignKey<VendaVeiculo>(x => x.VeiculoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Cliente).WithMany().HasForeignKey(x => x.ClienteId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Vendedor).WithMany().HasForeignKey(x => x.VendedorId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Loja).WithMany().HasForeignKey(x => x.LojaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.FormaPagamento).WithMany().HasForeignKey(x => x.FormaPagamentoId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class VendaPecaConfiguration : IEntityTypeConfiguration<VendaPeca>
{
    public void Configure(EntityTypeBuilder<VendaPeca> b)
    {
        b.ToTable("vendapeca");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id_vp").ValueGeneratedOnAdd();
        b.Property(x => x.PecaId).HasColumnName("id_pec_fk");
        b.Property(x => x.ClienteId).HasColumnName("id_cli_fk");
        b.Property(x => x.Quantidade).HasColumnName("quantidade_vp");
        b.Property(x => x.ValorUnitario).HasColumnName("valor_unitario_vp").HasPrecision(10, 2);
        b.Property(x => x.DataVenda).HasColumnName("data_venda_vp").HasDefaultValueSql("CURRENT_TIMESTAMP");

        b.HasOne(x => x.Peca).WithMany().HasForeignKey(x => x.PecaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Cliente).WithMany().HasForeignKey(x => x.ClienteId).OnDelete(DeleteBehavior.SetNull);
    }
}

public class DuvidaFrequenteConfiguration : IEntityTypeConfiguration<DuvidaFrequente>
{
    public void Configure(EntityTypeBuilder<DuvidaFrequente> b)
    {
        b.ToTable("duvidafrequente");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id_df").ValueGeneratedOnAdd();
        b.Property(x => x.Pergunta).HasColumnName("pergunta_df").HasColumnType("text").IsRequired();
        b.Property(x => x.Resposta).HasColumnName("resposta_df").HasColumnType("text").IsRequired();
    }
}
