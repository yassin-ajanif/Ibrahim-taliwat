using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionCommerciale.Shared.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddFactureFournisseurModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaiementsFournisseurs_BonsReception_BonReceptionId",
                table: "PaiementsFournisseurs");

            migrationBuilder.RenameColumn(
                name: "BonReceptionId",
                table: "PaiementsFournisseurs",
                newName: "FactureFournisseurId");

            migrationBuilder.RenameIndex(
                name: "IX_PaiementsFournisseurs_BonReceptionId",
                table: "PaiementsFournisseurs",
                newName: "IX_PaiementsFournisseurs_FactureFournisseurId");

            migrationBuilder.AddColumn<int>(
                name: "FactureFournisseurId",
                table: "BonsReception",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FacturesFournisseurs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Numero = table.Column<string>(type: "TEXT", nullable: false),
                    FournisseurId = table.Column<int>(type: "INTEGER", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DateEcheance = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EstPayee = table.Column<bool>(type: "INTEGER", nullable: false),
                    RemiseGlobale = table.Column<decimal>(type: "TEXT", nullable: false),
                    TotalTtc = table.Column<decimal>(type: "TEXT", nullable: false),
                    Note = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FacturesFournisseurs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FactureFournisseurLignes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FactureFournisseurId = table.Column<int>(type: "INTEGER", nullable: false),
                    BonReceptionId = table.Column<int>(type: "INTEGER", nullable: true),
                    ProduitId = table.Column<int>(type: "INTEGER", nullable: false),
                    Designation = table.Column<string>(type: "TEXT", nullable: false),
                    Quantite = table.Column<decimal>(type: "TEXT", nullable: false),
                    PrixUnitaireHT = table.Column<decimal>(type: "TEXT", nullable: false),
                    Remise = table.Column<decimal>(type: "TEXT", nullable: false),
                    TauxTVA = table.Column<decimal>(type: "TEXT", nullable: false),
                    Conditionnement = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FactureFournisseurLignes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FactureFournisseurLignes_BonsReception_BonReceptionId",
                        column: x => x.BonReceptionId,
                        principalTable: "BonsReception",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_FactureFournisseurLignes_FacturesFournisseurs_FactureFournisseurId",
                        column: x => x.FactureFournisseurId,
                        principalTable: "FacturesFournisseurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BonsReception_FactureFournisseurId",
                table: "BonsReception",
                column: "FactureFournisseurId");

            migrationBuilder.CreateIndex(
                name: "IX_FactureFournisseurLignes_BonReceptionId",
                table: "FactureFournisseurLignes",
                column: "BonReceptionId");

            migrationBuilder.CreateIndex(
                name: "IX_FactureFournisseurLignes_FactureFournisseurId",
                table: "FactureFournisseurLignes",
                column: "FactureFournisseurId");

            migrationBuilder.AddForeignKey(
                name: "FK_BonsReception_FacturesFournisseurs_FactureFournisseurId",
                table: "BonsReception",
                column: "FactureFournisseurId",
                principalTable: "FacturesFournisseurs",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PaiementsFournisseurs_FacturesFournisseurs_FactureFournisseurId",
                table: "PaiementsFournisseurs",
                column: "FactureFournisseurId",
                principalTable: "FacturesFournisseurs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BonsReception_FacturesFournisseurs_FactureFournisseurId",
                table: "BonsReception");

            migrationBuilder.DropForeignKey(
                name: "FK_PaiementsFournisseurs_FacturesFournisseurs_FactureFournisseurId",
                table: "PaiementsFournisseurs");

            migrationBuilder.DropTable(
                name: "FactureFournisseurLignes");

            migrationBuilder.DropTable(
                name: "FacturesFournisseurs");

            migrationBuilder.DropIndex(
                name: "IX_BonsReception_FactureFournisseurId",
                table: "BonsReception");

            migrationBuilder.DropColumn(
                name: "FactureFournisseurId",
                table: "BonsReception");

            migrationBuilder.RenameColumn(
                name: "FactureFournisseurId",
                table: "PaiementsFournisseurs",
                newName: "BonReceptionId");

            migrationBuilder.RenameIndex(
                name: "IX_PaiementsFournisseurs_FactureFournisseurId",
                table: "PaiementsFournisseurs",
                newName: "IX_PaiementsFournisseurs_BonReceptionId");

            migrationBuilder.AddForeignKey(
                name: "FK_PaiementsFournisseurs_BonsReception_BonReceptionId",
                table: "PaiementsFournisseurs",
                column: "BonReceptionId",
                principalTable: "BonsReception",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
