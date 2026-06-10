using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bookify_API.Migrations.BookifyDb
{
    /// <inheritdoc />
    public partial class AddServiceDuration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "service",
                type: "longtext",
                nullable: true,
                collation: "utf8mb4_0900_ai_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "UniteDuree",
                table: "service",
                type: "longtext",
                nullable: false,
                collation: "utf8mb4_0900_ai_ci")
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "service");

            migrationBuilder.DropColumn(
                name: "UniteDuree",
                table: "service");
        }
    }
}
