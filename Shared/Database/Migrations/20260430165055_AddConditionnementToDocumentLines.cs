using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionCommerciale.Shared.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddConditionnementToDocumentLines : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Conditionnement",
                table: "FactureLignes",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Conditionnement",
                table: "DevisLignes",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Conditionnement",
                table: "BonCommandeLignes",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Conditionnement",
                table: "FactureLignes");

            migrationBuilder.DropColumn(
                name: "Conditionnement",
                table: "DevisLignes");

            migrationBuilder.DropColumn(
                name: "Conditionnement",
                table: "BonCommandeLignes");
        }
    }
}
