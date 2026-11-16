using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaGestaoOficina.Migrations
{
    /// <inheritdoc />
    public partial class AddPecasEReparacaoPecas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Pecas",
                table: "Reparacoes");

            migrationBuilder.CreateTable(
                name: "Pecas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Preco = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Ativa = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pecas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReparacaoPecas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReparacaoId = table.Column<int>(type: "int", nullable: false),
                    PecaId = table.Column<int>(type: "int", nullable: false),
                    Quantidade = table.Column<int>(type: "int", nullable: false),
                    PrecoUnitario = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReparacaoPecas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReparacaoPecas_Pecas_PecaId",
                        column: x => x.PecaId,
                        principalTable: "Pecas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReparacaoPecas_Reparacoes_ReparacaoId",
                        column: x => x.ReparacaoId,
                        principalTable: "Reparacoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReparacaoPecas_PecaId",
                table: "ReparacaoPecas",
                column: "PecaId");

            migrationBuilder.CreateIndex(
                name: "IX_ReparacaoPecas_ReparacaoId_PecaId",
                table: "ReparacaoPecas",
                columns: new[] { "ReparacaoId", "PecaId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReparacaoPecas");

            migrationBuilder.DropTable(
                name: "Pecas");

            migrationBuilder.AddColumn<string>(
                name: "Pecas",
                table: "Reparacoes",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }
    }
}
