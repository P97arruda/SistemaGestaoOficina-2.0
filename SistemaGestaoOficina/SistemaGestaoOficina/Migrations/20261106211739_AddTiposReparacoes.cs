using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaGestaoOficina.Migrations
{
    /// <inheritdoc />
    public partial class AddTiposReparacoes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TipoReparacoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(70)", maxLength: 70, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipoReparacoes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReparacaoTipoReparacoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReparacaoId = table.Column<int>(type: "int", nullable: false),
                    TipoReparacaoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReparacaoTipoReparacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReparacaoTipoReparacoes_Reparacoes_ReparacaoId",
                        column: x => x.ReparacaoId,
                        principalTable: "Reparacoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReparacaoTipoReparacoes_TipoReparacoes_TipoReparacaoId",
                        column: x => x.TipoReparacaoId,
                        principalTable: "TipoReparacoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReparacaoTipoReparacoes_ReparacaoId_TipoReparacaoId",
                table: "ReparacaoTipoReparacoes",
                columns: new[] { "ReparacaoId", "TipoReparacaoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReparacaoTipoReparacoes_TipoReparacaoId",
                table: "ReparacaoTipoReparacoes",
                column: "TipoReparacaoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReparacaoTipoReparacoes");

            migrationBuilder.DropTable(
                name: "TipoReparacoes");
        }
    }
}
