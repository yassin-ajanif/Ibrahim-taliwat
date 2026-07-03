using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionCommerciale.Shared.Database.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRhAndDepensesModules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CongesRh");

            migrationBuilder.DropTable(
                name: "ContratsRh");

            migrationBuilder.DropTable(
                name: "DepenseLignes");

            migrationBuilder.DropTable(
                name: "DepensePiecesJointes");

            migrationBuilder.DropTable(
                name: "DepenseRecurrences");

            migrationBuilder.DropTable(
                name: "PaieBulletinsRh");

            migrationBuilder.DropTable(
                name: "PresencesRh");

            migrationBuilder.DropTable(
                name: "Depenses");

            migrationBuilder.DropTable(
                name: "EmployesRh");

            migrationBuilder.DropTable(
                name: "CategoriesDepenses");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CategoriesDepenses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Actif = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "INTEGER", nullable: true),
                    Nom = table.Column<string>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoriesDepenses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmployesRh",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Actif = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "INTEGER", nullable: true),
                    DateEmbauche = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    Matricule = table.Column<string>(type: "TEXT", nullable: false),
                    NomComplet = table.Column<string>(type: "TEXT", nullable: false),
                    Poste = table.Column<string>(type: "TEXT", nullable: false),
                    SalaireBase = table.Column<decimal>(type: "TEXT", nullable: false),
                    Telephone = table.Column<string>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployesRh", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DepenseRecurrences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CategorieDepenseId = table.Column<int>(type: "INTEGER", nullable: false),
                    Actif = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "INTEGER", nullable: true),
                    Fournisseur = table.Column<string>(type: "TEXT", nullable: false),
                    JourMois = table.Column<int>(type: "INTEGER", nullable: false),
                    Libelle = table.Column<string>(type: "TEXT", nullable: false),
                    MontantTtc = table.Column<decimal>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepenseRecurrences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DepenseRecurrences_CategoriesDepenses_CategorieDepenseId",
                        column: x => x.CategorieDepenseId,
                        principalTable: "CategoriesDepenses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Depenses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CategorieDepenseId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "INTEGER", nullable: true),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Fournisseur = table.Column<string>(type: "TEXT", nullable: false),
                    MontantPaye = table.Column<decimal>(type: "TEXT", nullable: false),
                    MontantTtc = table.Column<decimal>(type: "TEXT", nullable: false),
                    Note = table.Column<string>(type: "TEXT", nullable: false),
                    Numero = table.Column<string>(type: "TEXT", nullable: false),
                    Statut = table.Column<int>(type: "INTEGER", nullable: false),
                    StatutPaiement = table.Column<int>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Depenses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Depenses_CategoriesDepenses_CategorieDepenseId",
                        column: x => x.CategorieDepenseId,
                        principalTable: "CategoriesDepenses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CongesRh",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EmployeId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "INTEGER", nullable: true),
                    DateDebut = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DateFin = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Motif = table.Column<string>(type: "TEXT", nullable: false),
                    Statut = table.Column<int>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CongesRh", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CongesRh_EmployesRh_EmployeId",
                        column: x => x.EmployeId,
                        principalTable: "EmployesRh",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ContratsRh",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EmployeId = table.Column<int>(type: "INTEGER", nullable: false),
                    Actif = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "INTEGER", nullable: true),
                    DateDebut = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DateFin = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ReferenceDocument = table.Column<string>(type: "TEXT", nullable: false),
                    TypeContrat = table.Column<string>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContratsRh", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContratsRh_EmployesRh_EmployeId",
                        column: x => x.EmployeId,
                        principalTable: "EmployesRh",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PaieBulletinsRh",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EmployeId = table.Column<int>(type: "INTEGER", nullable: false),
                    Annee = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "INTEGER", nullable: true),
                    Mois = table.Column<int>(type: "INTEGER", nullable: false),
                    NetAPayer = table.Column<decimal>(type: "TEXT", nullable: false),
                    Primes = table.Column<decimal>(type: "TEXT", nullable: false),
                    Retenues = table.Column<decimal>(type: "TEXT", nullable: false),
                    SalaireBase = table.Column<decimal>(type: "TEXT", nullable: false),
                    Statut = table.Column<int>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaieBulletinsRh", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaieBulletinsRh_EmployesRh_EmployeId",
                        column: x => x.EmployeId,
                        principalTable: "EmployesRh",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PresencesRh",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EmployeId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "INTEGER", nullable: true),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EstAbsenceJustifiee = table.Column<bool>(type: "INTEGER", nullable: false),
                    HeuresTravaillees = table.Column<decimal>(type: "TEXT", nullable: false),
                    Note = table.Column<string>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PresencesRh", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PresencesRh_EmployesRh_EmployeId",
                        column: x => x.EmployeId,
                        principalTable: "EmployesRh",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DepenseLignes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DepenseId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "INTEGER", nullable: true),
                    Libelle = table.Column<string>(type: "TEXT", nullable: false),
                    Montant = table.Column<decimal>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepenseLignes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DepenseLignes_Depenses_DepenseId",
                        column: x => x.DepenseId,
                        principalTable: "Depenses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DepensePiecesJointes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DepenseId = table.Column<int>(type: "INTEGER", nullable: false),
                    CheminFichier = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "INTEGER", nullable: true),
                    NomFichier = table.Column<string>(type: "TEXT", nullable: false),
                    TypeMime = table.Column<string>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepensePiecesJointes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DepensePiecesJointes_Depenses_DepenseId",
                        column: x => x.DepenseId,
                        principalTable: "Depenses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CongesRh_EmployeId",
                table: "CongesRh",
                column: "EmployeId");

            migrationBuilder.CreateIndex(
                name: "IX_ContratsRh_EmployeId",
                table: "ContratsRh",
                column: "EmployeId");

            migrationBuilder.CreateIndex(
                name: "IX_DepenseLignes_DepenseId",
                table: "DepenseLignes",
                column: "DepenseId");

            migrationBuilder.CreateIndex(
                name: "IX_DepensePiecesJointes_DepenseId",
                table: "DepensePiecesJointes",
                column: "DepenseId");

            migrationBuilder.CreateIndex(
                name: "IX_DepenseRecurrences_CategorieDepenseId",
                table: "DepenseRecurrences",
                column: "CategorieDepenseId");

            migrationBuilder.CreateIndex(
                name: "IX_Depenses_CategorieDepenseId",
                table: "Depenses",
                column: "CategorieDepenseId");

            migrationBuilder.CreateIndex(
                name: "IX_PaieBulletinsRh_EmployeId",
                table: "PaieBulletinsRh",
                column: "EmployeId");

            migrationBuilder.CreateIndex(
                name: "IX_PresencesRh_EmployeId",
                table: "PresencesRh",
                column: "EmployeId");
        }
    }
}
