using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bookify_API.Migrations.BookifyDb
{
    /// <inheritdoc />
    public partial class AddLieuToRendezVous : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Lieu",
                table: "rendez_vous",
                type: "longtext",
                nullable: true,
                collation: "utf8mb4_0900_ai_ci")
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Lieu",
                table: "rendez_vous");
        }
    }
}
