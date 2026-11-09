using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StarConcessionaria.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFotoPrincipalOrdem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_fotoveiculo_id_vei_fk",
                table: "fotoveiculo");

            migrationBuilder.AddColumn<int>(
                name: "ordem_fot",
                table: "fotoveiculo",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "principal_fot",
                table: "fotoveiculo",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "UX_FotoVeiculo_Principal",
                table: "fotoveiculo",
                column: "id_vei_fk",
                unique: true,
                filter: "principal_fot");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_FotoVeiculo_Principal",
                table: "fotoveiculo");

            migrationBuilder.DropColumn(
                name: "ordem_fot",
                table: "fotoveiculo");

            migrationBuilder.DropColumn(
                name: "principal_fot",
                table: "fotoveiculo");

            migrationBuilder.CreateIndex(
                name: "IX_fotoveiculo_id_vei_fk",
                table: "fotoveiculo",
                column: "id_vei_fk");
        }
    }
}
