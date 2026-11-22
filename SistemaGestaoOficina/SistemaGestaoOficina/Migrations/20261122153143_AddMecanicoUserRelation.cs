using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaGestaoOficina.Migrations
{
    /// <inheritdoc />
    public partial class AddMecanicoUserRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Mecanicos",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Mecanicos_UserId",
                table: "Mecanicos",
                column: "UserId",
                unique: true,
                filter: "[UserId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Mecanicos_AspNetUsers_UserId",
                table: "Mecanicos",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Mecanicos_AspNetUsers_UserId",
                table: "Mecanicos");

            migrationBuilder.DropIndex(
                name: "IX_Mecanicos_UserId",
                table: "Mecanicos");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Mecanicos");
        }
    }
}
