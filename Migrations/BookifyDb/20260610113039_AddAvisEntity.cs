using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bookify_API.Migrations.BookifyDb
{
    /// <inheritdoc />
    public partial class AddAvisEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "avis",
                columns: table => new
                {
                    idAvis = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    idUtilisateur = table.Column<int>(type: "int", nullable: false),
                    idPrestataire = table.Column<int>(type: "int", nullable: false),
                    idRendezVous = table.Column<int>(type: "int", nullable: true),
                    note = table.Column<int>(type: "int", nullable: false),
                    commentaire = table.Column<string>(type: "text", nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    dateCreation = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.idAvis);
                    table.ForeignKey(
                        name: "fk_avis_prestataire",
                        column: x => x.idPrestataire,
                        principalTable: "prestataire",
                        principalColumn: "idPres",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_avis_rendezvous",
                        column: x => x.idRendezVous,
                        principalTable: "rendez_vous",
                        principalColumn: "idRendez_vous",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_avis_utilisateur",
                        column: x => x.idUtilisateur,
                        principalTable: "utilisateur",
                        principalColumn: "idUtilisateur",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateIndex(
                name: "idPrestataire_avis",
                table: "avis",
                column: "idPrestataire");

            migrationBuilder.CreateIndex(
                name: "idRendezVous_avis",
                table: "avis",
                column: "idRendezVous");

            migrationBuilder.CreateIndex(
                name: "idUtilisateur_avis",
                table: "avis",
                column: "idUtilisateur");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "avis");
        }
    }
}
