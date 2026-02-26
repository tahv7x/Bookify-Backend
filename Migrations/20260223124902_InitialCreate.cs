using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bookify_API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "utilisateur",
                columns: table => new
                {
                    idUtilisateur = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    nomComplet = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    email = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    telephone = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    adresse = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    passwordHash = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    role = table.Column<string>(type: "enum('CLIENT','PRESTATAIRE','ADMIN')", nullable: false, defaultValueSql: "'CLIENT'", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    creerA = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    resetCode = table.Column<string>(type: "varchar(6)", maxLength: 6, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    resetCodeExpiry = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.idUtilisateur);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "prestataire",
                columns: table => new
                {
                    idPres = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    idUtili = table.Column<int>(type: "int", nullable: false),
                    speciallite = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    bio = table.Column<string>(type: "text", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.idPres);
                    table.ForeignKey(
                        name: "prestataire_ibfk_1",
                        column: x => x.idUtili,
                        principalTable: "utilisateur",
                        principalColumn: "idUtilisateur");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "service",
                columns: table => new
                {
                    idService = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    idPres = table.Column<int>(type: "int", nullable: false),
                    nom = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    duration = table.Column<int>(type: "int", nullable: false),
                    prix = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.idService);
                    table.ForeignKey(
                        name: "service_ibfk_1",
                        column: x => x.idPres,
                        principalTable: "prestataire",
                        principalColumn: "idPres");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "rendez_vous",
                columns: table => new
                {
                    idRendez_vous = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    idUtili = table.Column<int>(type: "int", nullable: false),
                    idPres = table.Column<int>(type: "int", nullable: false),
                    idSer = table.Column<int>(type: "int", nullable: false),
                    date_debut = table.Column<DateTime>(type: "datetime", nullable: false),
                    date_fin = table.Column<DateTime>(type: "datetime", nullable: false),
                    statut = table.Column<string>(type: "enum('EN_ATTENTE','ACCEPTE','REFUSE','ANNULE','TERMINE')", nullable: true, defaultValueSql: "'EN_ATTENTE'", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    date_creation = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.idRendez_vous);
                    table.ForeignKey(
                        name: "rendez_vous_ibfk_1",
                        column: x => x.idUtili,
                        principalTable: "utilisateur",
                        principalColumn: "idUtilisateur");
                    table.ForeignKey(
                        name: "rendez_vous_ibfk_2",
                        column: x => x.idPres,
                        principalTable: "prestataire",
                        principalColumn: "idPres");
                    table.ForeignKey(
                        name: "rendez_vous_ibfk_3",
                        column: x => x.idSer,
                        principalTable: "service",
                        principalColumn: "idService");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "fichier",
                columns: table => new
                {
                    idfichier = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    idRendez_vous = table.Column<int>(type: "int", nullable: false),
                    idUtilisateur = table.Column<int>(type: "int", nullable: false),
                    nom_fichier = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    url = table.Column<string>(type: "text", nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    type_mime = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    date_upload = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.idfichier);
                    table.ForeignKey(
                        name: "fichier_ibfk_1",
                        column: x => x.idRendez_vous,
                        principalTable: "rendez_vous",
                        principalColumn: "idRendez_vous");
                    table.ForeignKey(
                        name: "fichier_ibfk_2",
                        column: x => x.idUtilisateur,
                        principalTable: "utilisateur",
                        principalColumn: "idUtilisateur");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateIndex(
                name: "idRendez_vous",
                table: "fichier",
                column: "idRendez_vous");

            migrationBuilder.CreateIndex(
                name: "idUtilisateur",
                table: "fichier",
                column: "idUtilisateur");

            migrationBuilder.CreateIndex(
                name: "idUtili",
                table: "prestataire",
                column: "idUtili");

            migrationBuilder.CreateIndex(
                name: "idPres",
                table: "rendez_vous",
                column: "idPres");

            migrationBuilder.CreateIndex(
                name: "idSer",
                table: "rendez_vous",
                column: "idSer");

            migrationBuilder.CreateIndex(
                name: "idUtili1",
                table: "rendez_vous",
                column: "idUtili");

            migrationBuilder.CreateIndex(
                name: "idPres1",
                table: "service",
                column: "idPres");

            migrationBuilder.CreateIndex(
                name: "email",
                table: "utilisateur",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "fichier");

            migrationBuilder.DropTable(
                name: "rendez_vous");

            migrationBuilder.DropTable(
                name: "service");

            migrationBuilder.DropTable(
                name: "prestataire");

            migrationBuilder.DropTable(
                name: "utilisateur");
        }
    }
}
