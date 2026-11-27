using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Salgados.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddLeiBasePdf : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "ArquivoBytes",
                table: "LeisBase",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ArquivoNome",
                table: "LeisBase",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tipo",
                table: "LeisBase",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "LeisBase",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ArquivoBytes", "ArquivoNome", "Tipo" },
                values: new object[] { null, null, "Texto" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArquivoBytes",
                table: "LeisBase");

            migrationBuilder.DropColumn(
                name: "ArquivoNome",
                table: "LeisBase");

            migrationBuilder.DropColumn(
                name: "Tipo",
                table: "LeisBase");
        }
    }
}
