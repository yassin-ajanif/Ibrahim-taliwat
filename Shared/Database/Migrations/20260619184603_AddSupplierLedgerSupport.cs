using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionCommerciale.Shared.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddSupplierLedgerSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TotalTtc",
                table: "BonsReception",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.Sql(@"
                UPDATE BonsReception
                SET TotalTtc = COALESCE((
                    SELECT SUM(
                        bl.QuantiteRecue * bl.PrixUnitaireHT
                        * (1.0 + bl.TauxTVA / 100.0)
                    )
                    FROM BonReceptionLignes bl
                    WHERE bl.BRId = BonsReception.Id
                ), 0)");

            migrationBuilder.CreateTable(
                name: "PaiementsFournisseurs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BonReceptionId = table.Column<int>(type: "INTEGER", nullable: false),
                    Montant = table.Column<decimal>(type: "TEXT", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Mode = table.Column<int>(type: "INTEGER", nullable: false),
                    Reference = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaiementsFournisseurs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaiementsFournisseurs_BonsReception_BonReceptionId",
                        column: x => x.BonReceptionId,
                        principalTable: "BonsReception",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaiementsFournisseurs_BonReceptionId",
                table: "PaiementsFournisseurs",
                column: "BonReceptionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaiementsFournisseurs");

            migrationBuilder.DropColumn(
                name: "TotalTtc",
                table: "BonsReception");
        }
    }
}
