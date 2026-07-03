using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionCommerciale.Shared.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddBonCommandeClient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BonCommandeClientId",
                table: "BonsLivraison",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BonsCommandeClient",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Numero = table.Column<string>(type: "TEXT", nullable: false),
                    ClientId = table.Column<int>(type: "INTEGER", nullable: false),
                    DevisId = table.Column<int>(type: "INTEGER", nullable: true),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Note = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BonsCommandeClient", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BonCommandeClientLignes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BonCommandeClientId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProduitId = table.Column<int>(type: "INTEGER", nullable: false),
                    Designation = table.Column<string>(type: "TEXT", nullable: false),
                    QuantiteCommandee = table.Column<decimal>(type: "TEXT", nullable: false),
                    PrixUnitaireHT = table.Column<decimal>(type: "TEXT", nullable: false),
                    TauxTVA = table.Column<decimal>(type: "TEXT", nullable: false),
                    Conditionnement = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BonCommandeClientLignes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BonCommandeClientLignes_BonsCommandeClient_BonCommandeClientId",
                        column: x => x.BonCommandeClientId,
                        principalTable: "BonsCommandeClient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BonsLivraison_BonCommandeClientId",
                table: "BonsLivraison",
                column: "BonCommandeClientId");

            migrationBuilder.CreateIndex(
                name: "IX_BonCommandeClientLignes_BonCommandeClientId",
                table: "BonCommandeClientLignes",
                column: "BonCommandeClientId");

            migrationBuilder.AddForeignKey(
                name: "FK_BonsLivraison_BonsCommandeClient_BonCommandeClientId",
                table: "BonsLivraison",
                column: "BonCommandeClientId",
                principalTable: "BonsCommandeClient",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BonsLivraison_BonsCommandeClient_BonCommandeClientId",
                table: "BonsLivraison");

            migrationBuilder.DropTable(
                name: "BonCommandeClientLignes");

            migrationBuilder.DropTable(
                name: "BonsCommandeClient");

            migrationBuilder.DropIndex(
                name: "IX_BonsLivraison_BonCommandeClientId",
                table: "BonsLivraison");

            migrationBuilder.DropColumn(
                name: "BonCommandeClientId",
                table: "BonsLivraison");
        }
    }
}
