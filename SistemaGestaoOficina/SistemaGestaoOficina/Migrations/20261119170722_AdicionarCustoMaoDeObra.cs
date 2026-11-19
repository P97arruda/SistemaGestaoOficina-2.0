using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaGestaoOficina.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarCustoMaoDeObra : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CustoMaoDeObra",
                table: "Reparacoes",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustoMaoDeObra",
                table: "Reparacoes");
        }
    }
}
