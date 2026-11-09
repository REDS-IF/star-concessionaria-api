using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StarConcessionaria.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedFaqsIniciais : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"INSERT INTO duvidafrequente (pergunta_df, resposta_df) VALUES
                ('Como funciona a reserva online?', 'Apos solicitar a reserva na plataforma, seu veiculo e bloqueado por 48h para outros clientes enquanto nossa equipe valida os dados.'),
                ('Posso dar meu veiculo usado na troca?', 'Sim! Avaliamos seu seminovo com precos justos e utilizamos o valor como entrada.'),
                ('Quais sao os documentos necessarios para test-drive?', 'Apresentar documento oficial com foto e CNH valida categoria B ou superior no momento do comparecimento a loja.');");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
