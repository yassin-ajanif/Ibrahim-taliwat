using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionCommerciale.Shared.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddFactureTotalTtc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TotalTtc",
                table: "Factures",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.Sql(@"
                UPDATE Factures
                SET TotalTtc = COALESCE((
                    SELECT SUM(
                        fl.Quantite * fl.PrixUnitaireHT
                        * (1.0 - fl.Remise / 100.0)
                        * (1.0 + fl.TauxTVA / 100.0)
                    )
                    FROM FactureLignes fl
                    WHERE fl.FactureId = Factures.Id
                ), 0) * (1.0 - Factures.RemiseGlobale / 100.0)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalTtc",
                table: "Factures");
        }
    }
}
