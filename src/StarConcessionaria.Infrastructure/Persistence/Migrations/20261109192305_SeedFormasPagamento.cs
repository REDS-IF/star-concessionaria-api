using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StarConcessionaria.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedFormasPagamento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Formas de pagamento aceitas (idempotente; nome_fp e UNIQUE). Sem ids explicitos para nao dessincronizar a sequence.
            migrationBuilder.Sql(@"INSERT INTO formapagamento (nome_fp) VALUES
                ('A vista'), ('Financiamento'), ('Cartao de credito'), ('Pix'), ('Consorcio')
                ON CONFLICT (nome_fp) DO NOTHING;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
