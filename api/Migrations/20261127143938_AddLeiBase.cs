using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Salgados.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddLeiBase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LeiAceitaEm",
                table: "Usuarios",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VersaoLeiAceita",
                table: "Usuarios",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "LeisBase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titulo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Conteudo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Versao = table.Column<int>(type: "int", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AtualizadoPorId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeisBase", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeisBase_Usuarios_AtualizadoPorId",
                        column: x => x.AtualizadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "LeisBase",
                columns: new[] { "Id", "AtualizadoEm", "AtualizadoPorId", "Conteudo", "Titulo", "Versao" },
                values: new object[] { 1, null, null, "", "Lei Base dos Salgados", 0 });

            migrationBuilder.CreateIndex(
                name: "IX_LeisBase_AtualizadoPorId",
                table: "LeisBase",
                column: "AtualizadoPorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LeisBase");

            migrationBuilder.DropColumn(
                name: "LeiAceitaEm",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "VersaoLeiAceita",
                table: "Usuarios");
        }
    }
}
