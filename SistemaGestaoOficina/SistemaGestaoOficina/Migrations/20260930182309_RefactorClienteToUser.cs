using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaGestaoOficina.Migrations
{
    /// <inheritdoc />
    public partial class RefactorClienteToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Marcacoes_Clientes_ClienteId",
                table: "Marcacoes");

            migrationBuilder.DropForeignKey(
                name: "FK_Veiculos_Clientes_ClienteId",
                table: "Veiculos");

            migrationBuilder.DropTable(
                name: "Clientes");

            migrationBuilder.DropIndex(
                name: "IX_Veiculos_ClienteId",
                table: "Veiculos");

            migrationBuilder.DropIndex(
                name: "IX_Marcacoes_ClienteId",
                table: "Marcacoes");

            migrationBuilder.DropColumn(
                name: "ClienteId",
                table: "Veiculos");

            migrationBuilder.DropColumn(
                name: "ClienteId",
                table: "Marcacoes");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Veiculos",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Marcacoes",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NIF",
                table: "AspNetUsers",
                type: "nvarchar(9)",
                maxLength: 9,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Veiculos_UserId",
                table: "Veiculos",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Marcacoes_UserId",
                table: "Marcacoes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_NIF",
                table: "AspNetUsers",
                column: "NIF",
                unique: true,
                filter: "[NIF] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Marcacoes_AspNetUsers_UserId",
                table: "Marcacoes",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Veiculos_AspNetUsers_UserId",
                table: "Veiculos",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Marcacoes_AspNetUsers_UserId",
                table: "Marcacoes");

            migrationBuilder.DropForeignKey(
                name: "FK_Veiculos_AspNetUsers_UserId",
                table: "Veiculos");

            migrationBuilder.DropIndex(
                name: "IX_Veiculos_UserId",
                table: "Veiculos");

            migrationBuilder.DropIndex(
                name: "IX_Marcacoes_UserId",
                table: "Marcacoes");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_NIF",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Veiculos");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Marcacoes");

            migrationBuilder.DropColumn(
                name: "NIF",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<int>(
                name: "ClienteId",
                table: "Veiculos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ClienteId",
                table: "Marcacoes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    NIF = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Clientes_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Veiculos_ClienteId",
                table: "Veiculos",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Marcacoes_ClienteId",
                table: "Marcacoes",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_NIF",
                table: "Clientes",
                column: "NIF",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_UserId",
                table: "Clientes",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Marcacoes_Clientes_ClienteId",
                table: "Marcacoes",
                column: "ClienteId",
                principalTable: "Clientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Veiculos_Clientes_ClienteId",
                table: "Veiculos",
                column: "ClienteId",
                principalTable: "Clientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
