using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionCommerciale.Shared.Database.Migrations
{
    /// <inheritdoc />
    public partial class FactureEstPayeeRemoveStatut : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EstPayee",
                table: "Factures",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            // Legacy StatutFacture.Payee == 3
            migrationBuilder.Sql("UPDATE Factures SET EstPayee = 1 WHERE Statut = 3;");

            migrationBuilder.DropColumn(
                name: "Statut",
                table: "Factures");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Statut",
                table: "Factures",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.Sql("""
                UPDATE Factures SET Statut = CASE WHEN EstPayee = 1 THEN 3 ELSE 1 END;
                """);

            migrationBuilder.DropColumn(
                name: "EstPayee",
                table: "Factures");
        }
    }
}
