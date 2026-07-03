using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionCommerciale.Shared.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddBonCommande : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BonCommandeId",
                table: "BonsReception",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BonsCommande",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Numero = table.Column<string>(type: "TEXT", nullable: false),
                    FournisseurId = table.Column<int>(type: "INTEGER", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Statut = table.Column<int>(type: "INTEGER", nullable: false),
                    Note = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BonsCommande", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BonCommandeLignes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BonCommandeId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProduitId = table.Column<int>(type: "INTEGER", nullable: false),
                    Designation = table.Column<string>(type: "TEXT", nullable: false),
                    QuantiteCommandee = table.Column<decimal>(type: "TEXT", nullable: false),
                    PrixUnitaireHT = table.Column<decimal>(type: "TEXT", nullable: false),
                    TauxTVA = table.Column<decimal>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BonCommandeLignes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BonCommandeLignes_BonsCommande_BonCommandeId",
                        column: x => x.BonCommandeId,
                        principalTable: "BonsCommande",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BonsReception_BonCommandeId",
                table: "BonsReception",
                column: "BonCommandeId");

            migrationBuilder.CreateIndex(
                name: "IX_BonCommandeLignes_BonCommandeId",
                table: "BonCommandeLignes",
                column: "BonCommandeId");

            migrationBuilder.AddForeignKey(
                name: "FK_BonsReception_BonsCommande_BonCommandeId",
                table: "BonsReception",
                column: "BonCommandeId",
                principalTable: "BonsCommande",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BonsReception_BonsCommande_BonCommandeId",
                table: "BonsReception");

            migrationBuilder.DropTable(
                name: "BonCommandeLignes");

            migrationBuilder.DropTable(
                name: "BonsCommande");

            migrationBuilder.DropIndex(
                name: "IX_BonsReception_BonCommandeId",
                table: "BonsReception");

            migrationBuilder.DropColumn(
                name: "BonCommandeId",
                table: "BonsReception");
        }
    }
}
