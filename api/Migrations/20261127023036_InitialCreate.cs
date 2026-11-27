using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Salgados.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Configuracoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExigirAceiteDivida = table.Column<bool>(type: "bit", nullable: false),
                    TipoDividaAniversario = table.Column<int>(type: "int", nullable: false),
                    ValorMinimoQuitacaoConjunta = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Configuracoes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Motivos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Descricao = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Motivos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Perfis",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Ativo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Perfis", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Permissoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Modulo = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissoes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Produtos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Valor = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Produtos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Login = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SenhaHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataNascimento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Avatar = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DeveAlterarSenha = table.Column<bool>(type: "bit", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PerfilId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Usuarios_Perfis_PerfilId",
                        column: x => x.PerfilId,
                        principalTable: "Perfis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PerfilPermissoes",
                columns: table => new
                {
                    PerfilId = table.Column<int>(type: "int", nullable: false),
                    PermissaoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerfilPermissoes", x => new { x.PerfilId, x.PermissaoId });
                    table.ForeignKey(
                        name: "FK_PerfilPermissoes_Perfis_PerfilId",
                        column: x => x.PerfilId,
                        principalTable: "Perfis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PerfilPermissoes_Permissoes_PermissaoId",
                        column: x => x.PermissaoId,
                        principalTable: "Permissoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Dividas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    MotivoId = table.Column<int>(type: "int", nullable: false),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    Observacao = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DataAquisicao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CriadoPorId = table.Column<int>(type: "int", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataQuitacao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ValorPago = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    QuitadoPorId = table.Column<int>(type: "int", nullable: true),
                    QuitadoEm = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dividas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Dividas_Motivos_MotivoId",
                        column: x => x.MotivoId,
                        principalTable: "Motivos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Dividas_Usuarios_CriadoPorId",
                        column: x => x.CriadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Dividas_Usuarios_QuitadoPorId",
                        column: x => x.QuitadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Dividas_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GruposPagamento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProdutoId = table.Column<int>(type: "int", nullable: false),
                    ValorMeta = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    ValorMinimoPorDivida = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    DataCompra = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Observacao = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CriadoPorId = table.Column<int>(type: "int", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechadoEm = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GruposPagamento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GruposPagamento_Produtos_ProdutoId",
                        column: x => x.ProdutoId,
                        principalTable: "Produtos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GruposPagamento_Usuarios_CriadoPorId",
                        column: x => x.CriadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DividaInteracoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DividaId = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    Mensagem = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DividaInteracoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DividaInteracoes_Dividas_DividaId",
                        column: x => x.DividaId,
                        principalTable: "Dividas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DividaInteracoes_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Pagamentos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DividaId = table.Column<int>(type: "int", nullable: false),
                    DataPagamento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ValorPago = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Observacao = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CriadoPorId = table.Column<int>(type: "int", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AvaliadoPorId = table.Column<int>(type: "int", nullable: true),
                    AvaliadoEm = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MotivoRecusa = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pagamentos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pagamentos_Dividas_DividaId",
                        column: x => x.DividaId,
                        principalTable: "Dividas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Pagamentos_Usuarios_AvaliadoPorId",
                        column: x => x.AvaliadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Pagamentos_Usuarios_CriadoPorId",
                        column: x => x.CriadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GrupoPagamentoParticipacoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GrupoPagamentoId = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    ValorContribuido = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrupoPagamentoParticipacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GrupoPagamentoParticipacoes_GruposPagamento_GrupoPagamentoId",
                        column: x => x.GrupoPagamentoId,
                        principalTable: "GruposPagamento",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GrupoPagamentoParticipacoes_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GrupoPagamentoDividas",
                columns: table => new
                {
                    GrupoPagamentoParticipacaoId = table.Column<int>(type: "int", nullable: false),
                    DividaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrupoPagamentoDividas", x => new { x.GrupoPagamentoParticipacaoId, x.DividaId });
                    table.ForeignKey(
                        name: "FK_GrupoPagamentoDividas_Dividas_DividaId",
                        column: x => x.DividaId,
                        principalTable: "Dividas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GrupoPagamentoDividas_GrupoPagamentoParticipacoes_GrupoPagamentoParticipacaoId",
                        column: x => x.GrupoPagamentoParticipacaoId,
                        principalTable: "GrupoPagamentoParticipacoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Configuracoes",
                columns: new[] { "Id", "ExigirAceiteDivida", "TipoDividaAniversario", "ValorMinimoQuitacaoConjunta" },
                values: new object[] { 1, true, 1, 30m });

            migrationBuilder.InsertData(
                table: "Motivos",
                columns: new[] { "Id", "Ativo", "Descricao" },
                values: new object[,]
                {
                    { 1, true, "Aniversário" },
                    { 2, true, "Atraso em reunião" },
                    { 3, true, "Quebrou o build" },
                    { 4, true, "Bug em produção" },
                    { 5, true, "Esqueceu de bloquear o computador" }
                });

            migrationBuilder.InsertData(
                table: "Perfis",
                columns: new[] { "Id", "Ativo", "Descricao", "Nome" },
                values: new object[,]
                {
                    { 1, true, "Acesso total ao sistema, usuários, perfis e aprovação de pagamentos", "Administrador" },
                    { 2, true, "Gerencia dívidas, pagamentos e motivos", "Gestor" },
                    { 3, true, "Registra dívidas, aceita/contesta e informa pagamentos", "Colaborador" }
                });

            migrationBuilder.InsertData(
                table: "Permissoes",
                columns: new[] { "Id", "Codigo", "Descricao", "Modulo" },
                values: new object[,]
                {
                    { 1, "dividas.ver", "Visualizar dívidas e painel", "Dívidas" },
                    { 2, "dividas.criar", "Registrar novas dívidas", "Dívidas" },
                    { 3, "dividas.quitar", "Aprovar/recusar ou cadastrar pagamento aprovado", "Dívidas" },
                    { 4, "usuarios.ver", "Visualizar equipe e histórico de dívidas", "Usuários" },
                    { 5, "usuarios.gerenciar", "Cadastrar, editar e ativar/desativar usuários", "Usuários" },
                    { 6, "motivos.ver", "Visualizar motivos", "Motivos" },
                    { 7, "motivos.gerenciar", "Cadastrar e ativar/desativar motivos", "Motivos" },
                    { 8, "perfis.gerenciar", "Gerenciar perfis e permissões", "Segurança" }
                });

            migrationBuilder.InsertData(
                table: "PerfilPermissoes",
                columns: new[] { "PerfilId", "PermissaoId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 1, 2 },
                    { 1, 3 },
                    { 1, 4 },
                    { 1, 5 },
                    { 1, 6 },
                    { 1, 7 },
                    { 1, 8 },
                    { 2, 1 },
                    { 2, 2 },
                    { 2, 3 },
                    { 2, 4 },
                    { 2, 6 },
                    { 2, 7 },
                    { 3, 1 },
                    { 3, 2 },
                    { 3, 4 },
                    { 3, 6 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_DividaInteracoes_DividaId",
                table: "DividaInteracoes",
                column: "DividaId");

            migrationBuilder.CreateIndex(
                name: "IX_DividaInteracoes_UsuarioId",
                table: "DividaInteracoes",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Dividas_CriadoPorId",
                table: "Dividas",
                column: "CriadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_Dividas_MotivoId",
                table: "Dividas",
                column: "MotivoId");

            migrationBuilder.CreateIndex(
                name: "IX_Dividas_QuitadoPorId",
                table: "Dividas",
                column: "QuitadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_Dividas_UsuarioId",
                table: "Dividas",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_GrupoPagamentoDividas_DividaId",
                table: "GrupoPagamentoDividas",
                column: "DividaId");

            migrationBuilder.CreateIndex(
                name: "IX_GrupoPagamentoParticipacoes_GrupoPagamentoId",
                table: "GrupoPagamentoParticipacoes",
                column: "GrupoPagamentoId");

            migrationBuilder.CreateIndex(
                name: "IX_GrupoPagamentoParticipacoes_UsuarioId",
                table: "GrupoPagamentoParticipacoes",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_GruposPagamento_CriadoPorId",
                table: "GruposPagamento",
                column: "CriadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_GruposPagamento_ProdutoId",
                table: "GruposPagamento",
                column: "ProdutoId");

            migrationBuilder.CreateIndex(
                name: "IX_Pagamentos_AvaliadoPorId",
                table: "Pagamentos",
                column: "AvaliadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_Pagamentos_CriadoPorId",
                table: "Pagamentos",
                column: "CriadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_Pagamentos_DividaId",
                table: "Pagamentos",
                column: "DividaId");

            migrationBuilder.CreateIndex(
                name: "IX_PerfilPermissoes_PermissaoId",
                table: "PerfilPermissoes",
                column: "PermissaoId");

            migrationBuilder.CreateIndex(
                name: "IX_Perfis_Nome",
                table: "Perfis",
                column: "Nome",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Permissoes_Codigo",
                table: "Permissoes",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Login",
                table: "Usuarios",
                column: "Login",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_PerfilId",
                table: "Usuarios",
                column: "PerfilId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Configuracoes");

            migrationBuilder.DropTable(
                name: "DividaInteracoes");

            migrationBuilder.DropTable(
                name: "GrupoPagamentoDividas");

            migrationBuilder.DropTable(
                name: "Pagamentos");

            migrationBuilder.DropTable(
                name: "PerfilPermissoes");

            migrationBuilder.DropTable(
                name: "GrupoPagamentoParticipacoes");

            migrationBuilder.DropTable(
                name: "Dividas");

            migrationBuilder.DropTable(
                name: "Permissoes");

            migrationBuilder.DropTable(
                name: "GruposPagamento");

            migrationBuilder.DropTable(
                name: "Motivos");

            migrationBuilder.DropTable(
                name: "Produtos");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Perfis");
        }
    }
}
