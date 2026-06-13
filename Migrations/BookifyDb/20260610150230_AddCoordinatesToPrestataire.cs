using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bookify_API.Migrations.BookifyDb
{
    /// <inheritdoc />
    public partial class AddCoordinatesToPrestataire : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "latitude",
                table: "prestataire",
                type: "double",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "longitude",
                table: "prestataire",
                type: "double",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "latitude",
                table: "prestataire");

            migrationBuilder.DropColumn(
                name: "longitude",
                table: "prestataire");
        }
    }
}
