using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace StarConcessionaria.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "cliente",
                columns: table => new
                {
                    id_cli = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nome_cli = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    email_cli = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    telefone_cli = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cliente", x => x.id_cli);
                });

            migrationBuilder.CreateTable(
                name: "duvidafrequente",
                columns: table => new
                {
                    id_df = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    pergunta_df = table.Column<string>(type: "text", nullable: false),
                    resposta_df = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_duvidafrequente", x => x.id_df);
                });

            migrationBuilder.CreateTable(
                name: "formapagamento",
                columns: table => new
                {
                    id_fp = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nome_fp = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_formapagamento", x => x.id_fp);
                });

            migrationBuilder.CreateTable(
                name: "loja",
                columns: table => new
                {
                    id_loj = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nome_loj = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    telefone_loj = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    cep_loj = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    endereco_loj = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    numero_loj = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    complemento_loj = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    bairro_loj = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    cidade_loj = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    estado_loj = table.Column<string>(type: "character(2)", nullable: true),
                    latitude_loj = table.Column<decimal>(type: "numeric(10,7)", precision: 10, scale: 7, nullable: true),
                    longitude_loj = table.Column<decimal>(type: "numeric(10,7)", precision: 10, scale: 7, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_loja", x => x.id_loj);
                });

            migrationBuilder.CreateTable(
                name: "peca",
                columns: table => new
                {
                    id_pec = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nome_pec = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    descricao_pec = table.Column<string>(type: "text", nullable: true),
                    preco_pec = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    quantidade_estoque_pec = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_peca", x => x.id_pec);
                });

            migrationBuilder.CreateTable(
                name: "usuariosistema",
                columns: table => new
                {
                    id_usu = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nome_usu = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    email_usu = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    senha_usu = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    perfil_usu = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuariosistema", x => x.id_usu);
                });

            migrationBuilder.CreateTable(
                name: "veiculo",
                columns: table => new
                {
                    id_vei = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_loj_fk = table.Column<int>(type: "integer", nullable: false),
                    tipo_vei = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    marca_vei = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    modelo_vei = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ano_vei = table.Column<int>(type: "integer", nullable: false),
                    preco_vei = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    quilometragem_vei = table.Column<int>(type: "integer", nullable: false),
                    cor_vei = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    descricao_vei = table.Column<string>(type: "text", nullable: true),
                    situacao_vei = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "Disponivel")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_veiculo", x => x.id_vei);
                    table.ForeignKey(
                        name: "FK_veiculo_loja_id_loj_fk",
                        column: x => x.id_loj_fk,
                        principalTable: "loja",
                        principalColumn: "id_loj",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "vendapeca",
                columns: table => new
                {
                    id_vp = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_pec_fk = table.Column<int>(type: "integer", nullable: false),
                    id_cli_fk = table.Column<int>(type: "integer", nullable: true),
                    quantidade_vp = table.Column<int>(type: "integer", nullable: false),
                    valor_unitario_vp = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    data_venda_vp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vendapeca", x => x.id_vp);
                    table.ForeignKey(
                        name: "FK_vendapeca_cliente_id_cli_fk",
                        column: x => x.id_cli_fk,
                        principalTable: "cliente",
                        principalColumn: "id_cli",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_vendapeca_peca_id_pec_fk",
                        column: x => x.id_pec_fk,
                        principalTable: "peca",
                        principalColumn: "id_pec",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "agendamento",
                columns: table => new
                {
                    id_age = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_cli_fk = table.Column<int>(type: "integer", nullable: false),
                    id_loj_fk = table.Column<int>(type: "integer", nullable: false),
                    id_vei_fk = table.Column<int>(type: "integer", nullable: true),
                    tipo_age = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    data_hora_inicio_age = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    data_hora_fim_age = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    situacao_age = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValue: "Solicitado")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agendamento", x => x.id_age);
                    table.ForeignKey(
                        name: "FK_agendamento_cliente_id_cli_fk",
                        column: x => x.id_cli_fk,
                        principalTable: "cliente",
                        principalColumn: "id_cli",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_agendamento_loja_id_loj_fk",
                        column: x => x.id_loj_fk,
                        principalTable: "loja",
                        principalColumn: "id_loj",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_agendamento_veiculo_id_vei_fk",
                        column: x => x.id_vei_fk,
                        principalTable: "veiculo",
                        principalColumn: "id_vei",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "alteracaoveiculo",
                columns: table => new
                {
                    id_alt = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_vei_fk = table.Column<int>(type: "integer", nullable: false),
                    descricao_alt = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_alteracaoveiculo", x => x.id_alt);
                    table.ForeignKey(
                        name: "FK_alteracaoveiculo_veiculo_id_vei_fk",
                        column: x => x.id_vei_fk,
                        principalTable: "veiculo",
                        principalColumn: "id_vei",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "contato",
                columns: table => new
                {
                    id_con = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_cli_fk = table.Column<int>(type: "integer", nullable: true),
                    id_vei_fk = table.Column<int>(type: "integer", nullable: true),
                    id_loj_fk = table.Column<int>(type: "integer", nullable: true),
                    id_usu_fk = table.Column<int>(type: "integer", nullable: true),
                    tipo_con = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    mensagem_con = table.Column<string>(type: "text", nullable: false),
                    data_con = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contato", x => x.id_con);
                    table.ForeignKey(
                        name: "FK_contato_cliente_id_cli_fk",
                        column: x => x.id_cli_fk,
                        principalTable: "cliente",
                        principalColumn: "id_cli",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_contato_loja_id_loj_fk",
                        column: x => x.id_loj_fk,
                        principalTable: "loja",
                        principalColumn: "id_loj",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_contato_usuariosistema_id_usu_fk",
                        column: x => x.id_usu_fk,
                        principalTable: "usuariosistema",
                        principalColumn: "id_usu",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_contato_veiculo_id_vei_fk",
                        column: x => x.id_vei_fk,
                        principalTable: "veiculo",
                        principalColumn: "id_vei",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "fotoveiculo",
                columns: table => new
                {
                    id_fot = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_vei_fk = table.Column<int>(type: "integer", nullable: false),
                    url_fot = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fotoveiculo", x => x.id_fot);
                    table.ForeignKey(
                        name: "FK_fotoveiculo_veiculo_id_vei_fk",
                        column: x => x.id_vei_fk,
                        principalTable: "veiculo",
                        principalColumn: "id_vei",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reserva",
                columns: table => new
                {
                    id_res = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_cli_fk = table.Column<int>(type: "integer", nullable: false),
                    id_vei_fk = table.Column<int>(type: "integer", nullable: false),
                    data_solicitacao_res = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    situacao_res = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValue: "Pendente")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reserva", x => x.id_res);
                    table.ForeignKey(
                        name: "FK_reserva_cliente_id_cli_fk",
                        column: x => x.id_cli_fk,
                        principalTable: "cliente",
                        principalColumn: "id_cli",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_reserva_veiculo_id_vei_fk",
                        column: x => x.id_vei_fk,
                        principalTable: "veiculo",
                        principalColumn: "id_vei",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "vendaveiculo",
                columns: table => new
                {
                    id_vv = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_vei_fk = table.Column<int>(type: "integer", nullable: false),
                    id_cli_fk = table.Column<int>(type: "integer", nullable: false),
                    id_vendedor_fk = table.Column<int>(type: "integer", nullable: false),
                    id_loj_fk = table.Column<int>(type: "integer", nullable: false),
                    id_fp_fk = table.Column<int>(type: "integer", nullable: false),
                    data_venda_vv = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    valor_final_vv = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vendaveiculo", x => x.id_vv);
                    table.ForeignKey(
                        name: "FK_vendaveiculo_cliente_id_cli_fk",
                        column: x => x.id_cli_fk,
                        principalTable: "cliente",
                        principalColumn: "id_cli",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_vendaveiculo_formapagamento_id_fp_fk",
                        column: x => x.id_fp_fk,
                        principalTable: "formapagamento",
                        principalColumn: "id_fp",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_vendaveiculo_loja_id_loj_fk",
                        column: x => x.id_loj_fk,
                        principalTable: "loja",
                        principalColumn: "id_loj",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_vendaveiculo_usuariosistema_id_vendedor_fk",
                        column: x => x.id_vendedor_fk,
                        principalTable: "usuariosistema",
                        principalColumn: "id_usu",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_vendaveiculo_veiculo_id_vei_fk",
                        column: x => x.id_vei_fk,
                        principalTable: "veiculo",
                        principalColumn: "id_vei",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_agendamento_id_cli_fk",
                table: "agendamento",
                column: "id_cli_fk");

            migrationBuilder.CreateIndex(
                name: "IX_agendamento_id_loj_fk",
                table: "agendamento",
                column: "id_loj_fk");

            migrationBuilder.CreateIndex(
                name: "IX_Agendamento_Veiculo_Horario",
                table: "agendamento",
                columns: new[] { "id_vei_fk", "data_hora_inicio_age", "data_hora_fim_age" });

            migrationBuilder.CreateIndex(
                name: "IX_alteracaoveiculo_id_vei_fk",
                table: "alteracaoveiculo",
                column: "id_vei_fk");

            migrationBuilder.CreateIndex(
                name: "IX_contato_id_cli_fk",
                table: "contato",
                column: "id_cli_fk");

            migrationBuilder.CreateIndex(
                name: "IX_contato_id_loj_fk",
                table: "contato",
                column: "id_loj_fk");

            migrationBuilder.CreateIndex(
                name: "IX_contato_id_usu_fk",
                table: "contato",
                column: "id_usu_fk");

            migrationBuilder.CreateIndex(
                name: "IX_contato_id_vei_fk",
                table: "contato",
                column: "id_vei_fk");

            migrationBuilder.CreateIndex(
                name: "IX_formapagamento_nome_fp",
                table: "formapagamento",
                column: "nome_fp",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_fotoveiculo_id_vei_fk",
                table: "fotoveiculo",
                column: "id_vei_fk");

            migrationBuilder.CreateIndex(
                name: "IX_reserva_id_cli_fk",
                table: "reserva",
                column: "id_cli_fk");

            migrationBuilder.CreateIndex(
                name: "IX_Reserva_Veiculo",
                table: "reserva",
                columns: new[] { "id_vei_fk", "situacao_res" });

            migrationBuilder.CreateIndex(
                name: "IX_usuariosistema_email_usu",
                table: "usuariosistema",
                column: "email_usu",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Veiculo_Filtros",
                table: "veiculo",
                columns: new[] { "situacao_vei", "marca_vei", "modelo_vei", "ano_vei", "preco_vei" });

            migrationBuilder.CreateIndex(
                name: "IX_Veiculo_Loja",
                table: "veiculo",
                column: "id_loj_fk");

            migrationBuilder.CreateIndex(
                name: "IX_vendapeca_id_cli_fk",
                table: "vendapeca",
                column: "id_cli_fk");

            migrationBuilder.CreateIndex(
                name: "IX_vendapeca_id_pec_fk",
                table: "vendapeca",
                column: "id_pec_fk");

            migrationBuilder.CreateIndex(
                name: "IX_vendaveiculo_id_cli_fk",
                table: "vendaveiculo",
                column: "id_cli_fk");

            migrationBuilder.CreateIndex(
                name: "IX_vendaveiculo_id_fp_fk",
                table: "vendaveiculo",
                column: "id_fp_fk");

            migrationBuilder.CreateIndex(
                name: "IX_vendaveiculo_id_loj_fk",
                table: "vendaveiculo",
                column: "id_loj_fk");

            migrationBuilder.CreateIndex(
                name: "IX_vendaveiculo_id_vei_fk",
                table: "vendaveiculo",
                column: "id_vei_fk",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vendaveiculo_id_vendedor_fk",
                table: "vendaveiculo",
                column: "id_vendedor_fk");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "agendamento");

            migrationBuilder.DropTable(
                name: "alteracaoveiculo");

            migrationBuilder.DropTable(
                name: "contato");

            migrationBuilder.DropTable(
                name: "duvidafrequente");

            migrationBuilder.DropTable(
                name: "fotoveiculo");

            migrationBuilder.DropTable(
                name: "reserva");

            migrationBuilder.DropTable(
                name: "vendapeca");

            migrationBuilder.DropTable(
                name: "vendaveiculo");

            migrationBuilder.DropTable(
                name: "peca");

            migrationBuilder.DropTable(
                name: "cliente");

            migrationBuilder.DropTable(
                name: "formapagamento");

            migrationBuilder.DropTable(
                name: "usuariosistema");

            migrationBuilder.DropTable(
                name: "veiculo");

            migrationBuilder.DropTable(
                name: "loja");
        }
    }
}
