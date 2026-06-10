using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bookify_API.Migrations.BookifyDb
{
    /// <inheritdoc />
    public partial class AddFavoris : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "favoris",
                columns: table => new
                {
                    idFavori = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    idUtilisateur = table.Column<int>(type: "int", nullable: false),
                    idPrestataire = table.Column<int>(type: "int", nullable: false),
                    dateAjout = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.idFavori);
                    table.ForeignKey(
                        name: "fk_favoris_prestataire",
                        column: x => x.idPrestataire,
                        principalTable: "prestataire",
                        principalColumn: "idPres",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_favoris_utilisateur",
                        column: x => x.idUtilisateur,
                        principalTable: "utilisateur",
                        principalColumn: "idUtilisateur",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateIndex(
                name: "idPrestataire_favoris",
                table: "favoris",
                column: "idPrestataire");

            migrationBuilder.CreateIndex(
                name: "idUtilisateur_favoris",
                table: "favoris",
                column: "idUtilisateur");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "favoris");
        }
    }
}
