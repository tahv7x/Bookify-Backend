using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bookify_API.Migrations.BookifyDb
{
    /// <inheritdoc />
    public partial class SyncSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.DropTable(
            //     name: "fichier");

            // migrationBuilder.DropColumn(
            //     name: "categorie",
            //     table: "prestataire");

            //migrationBuilder.AddColumn<bool>(
              //  name: "IsBlocked",
                //table: "utilisateur",
                //type: "tinyint(1)",
                //nullable: false,
                //defaultValueSql: "0");

            migrationBuilder.AlterColumn<string>(
                name: "statut",
                table: "rendez_vous",
                type: "enum('EN_ATTENTE','ACCEPTE','REFUSE','ANNULE','TERMINE','A_REPLANIFIER')",
                nullable: true,
                defaultValueSql: "'EN_ATTENTE'",
                collation: "utf8mb4_0900_ai_ci",
                oldClrType: typeof(string),
                oldType: "enum('EN_ATTENTE','ACCEPTE','REFUSE','ANNULE','TERMINE')",
                oldNullable: true,
                oldDefaultValueSql: "'EN_ATTENTE'")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            // migrationBuilder.AddColumn<int>(
            //     name: "idCategorie",
            //     table: "prestataire",
            //     type: "int",
            //     nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "jourSemaine",
                table: "disponibilite",
                type: "enum('Lun','Mar','Mer','Jeu','Ven','Sam','Dim')",
                nullable: true,
                collation: "utf8mb4_0900_ai_ci",
                oldClrType: typeof(string),
                oldType: "enum('Lun','Mar','Mer','Jeu','Ven','Sam','Dim')")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "heureFin",
                table: "disponibilite",
                type: "time(6)",
                nullable: true,
                oldClrType: typeof(TimeSpan),
                oldType: "time(6)");

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "heureDebut",
                table: "disponibilite",
                type: "time(6)",
                nullable: true,
                oldClrType: typeof(TimeSpan),
                oldType: "time(6)");

            // migrationBuilder.CreateTable(
            //     name: "categorie",
            //     columns: table => new
            //     {
            //         idCategorie = table.Column<int>(type: "int", nullable: false)
            //             .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
            //         nom = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, collation: "utf8mb4_0900_ai_ci")
            //             .Annotation("MySql:CharSet", "utf8mb4"),
            //         description = table.Column<string>(type: "text", nullable: true, collation: "utf8mb4_0900_ai_ci")
            //             .Annotation("MySql:CharSet", "utf8mb4"),
            //         isActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValueSql: "1"),
            //         createdAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("PRIMARY", x => x.idCategorie);
            //     })
            //     .Annotation("MySql:CharSet", "utf8mb4")
            //     .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            // migrationBuilder.CreateIndex(
            //     name: "IX_prestataire_idCategorie",
            //     table: "prestataire",
            //     column: "idCategorie");

            // migrationBuilder.CreateIndex(
            //     name: "IX_categorie_nom",
            //     table: "categorie",
            //     column: "nom",
            //     unique: true);

            // migrationBuilder.AddForeignKey(
            //     name: "FK_prestataire_categorie_idCategorie",
            //     table: "prestataire",
            //     column: "idCategorie",
            //     principalTable: "categorie",
            //     principalColumn: "idCategorie",
            //     onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_prestataire_categorie_idCategorie",
                table: "prestataire");

            migrationBuilder.DropTable(
                name: "categorie");

            migrationBuilder.DropIndex(
                name: "IX_prestataire_idCategorie",
                table: "prestataire");

            migrationBuilder.DropColumn(
                name: "isBlocked",
                table: "utilisateur");

            migrationBuilder.DropColumn(
                name: "idCategorie",
                table: "prestataire");

            migrationBuilder.AlterColumn<string>(
                name: "statut",
                table: "rendez_vous",
                type: "enum('EN_ATTENTE','ACCEPTE','REFUSE','ANNULE','TERMINE')",
                nullable: true,
                defaultValueSql: "'EN_ATTENTE'",
                collation: "utf8mb4_0900_ai_ci",
                oldClrType: typeof(string),
                oldType: "enum('EN_ATTENTE','ACCEPTE','REFUSE','ANNULE','TERMINE','A_REPLANIFIER')",
                oldNullable: true,
                oldDefaultValueSql: "'EN_ATTENTE'")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.AddColumn<string>(
                name: "categorie",
                table: "prestataire",
                type: "enum('Sante & medical','Beaute & Bien etre','Services profesionnels','Service techniques')",
                nullable: true,
                collation: "utf8mb4_0900_ai_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "disponibilite",
                keyColumn: "jourSemaine",
                keyValue: null,
                column: "jourSemaine",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "jourSemaine",
                table: "disponibilite",
                type: "enum('Lun','Mar','Mer','Jeu','Ven','Sam','Dim')",
                nullable: false,
                collation: "utf8mb4_0900_ai_ci",
                oldClrType: typeof(string),
                oldType: "enum('Lun','Mar','Mer','Jeu','Ven','Sam','Dim')",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "heureFin",
                table: "disponibilite",
                type: "time(6)",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0),
                oldClrType: typeof(TimeSpan),
                oldType: "time(6)",
                oldNullable: true);

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "heureDebut",
                table: "disponibilite",
                type: "time(6)",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0),
                oldClrType: typeof(TimeSpan),
                oldType: "time(6)",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "fichier",
                columns: table => new
                {
                    idfichier = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    idRendez_vous = table.Column<int>(type: "int", nullable: true),
                    idUtilisateur = table.Column<int>(type: "int", nullable: true),
                    date_upload = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    nom_fichier = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    type_mime = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    url = table.Column<string>(type: "text", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
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
        }
    }
}
