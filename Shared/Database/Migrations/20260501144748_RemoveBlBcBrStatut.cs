using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionCommerciale.Shared.Database.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBlBcBrStatut : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Statut",
                table: "BonsReception");

            migrationBuilder.DropColumn(
                name: "Statut",
                table: "BonsLivraison");

            migrationBuilder.DropColumn(
                name: "Statut",
                table: "BonsCommande");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Statut",
                table: "BonsReception",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Statut",
                table: "BonsLivraison",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Statut",
                table: "BonsCommande",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }
    }
}
