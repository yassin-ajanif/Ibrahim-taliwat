using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionCommerciale.Shared.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiBlFactureViaBlFactureId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FactureId",
                table: "BonsLivraison",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BonLivraisonId",
                table: "FactureLignes",
                type: "INTEGER",
                nullable: true);

            // Migrate existing single-BL links (Factures.BLId → BonsLivraison.FactureId)
            migrationBuilder.Sql(@"
                UPDATE BonsLivraison
                SET FactureId = (SELECT Id FROM Factures WHERE Factures.BLId = BonsLivraison.Id)
                WHERE Id IN (SELECT BLId FROM Factures WHERE BLId IS NOT NULL)");

            // Backfill BonLivraisonId on lines for legacy single-BL factures
            migrationBuilder.Sql(@"
                UPDATE FactureLignes
                SET BonLivraisonId = (SELECT BLId FROM Factures WHERE Factures.Id = FactureLignes.FactureId)
                WHERE FactureId IN (SELECT Id FROM Factures WHERE BLId IS NOT NULL)");

            migrationBuilder.DropColumn(
                name: "BLId",
                table: "Factures");

            migrationBuilder.CreateIndex(
                name: "IX_BonsLivraison_FactureId",
                table: "BonsLivraison",
                column: "FactureId");

            migrationBuilder.CreateIndex(
                name: "IX_FactureLignes_BonLivraisonId",
                table: "FactureLignes",
                column: "BonLivraisonId");

            migrationBuilder.AddForeignKey(
                name: "FK_BonsLivraison_Factures_FactureId",
                table: "BonsLivraison",
                column: "FactureId",
                principalTable: "Factures",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_FactureLignes_BonsLivraison_BonLivraisonId",
                table: "FactureLignes",
                column: "BonLivraisonId",
                principalTable: "BonsLivraison",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BonsLivraison_Factures_FactureId",
                table: "BonsLivraison");

            migrationBuilder.DropForeignKey(
                name: "FK_FactureLignes_BonsLivraison_BonLivraisonId",
                table: "FactureLignes");

            migrationBuilder.DropIndex(
                name: "IX_FactureLignes_BonLivraisonId",
                table: "FactureLignes");

            migrationBuilder.DropIndex(
                name: "IX_BonsLivraison_FactureId",
                table: "BonsLivraison");

            migrationBuilder.DropColumn(
                name: "BonLivraisonId",
                table: "FactureLignes");

            migrationBuilder.DropColumn(
                name: "FactureId",
                table: "BonsLivraison");

            migrationBuilder.AddColumn<int>(
                name: "BLId",
                table: "Factures",
                type: "INTEGER",
                nullable: true);
        }
    }
}
