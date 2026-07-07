using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionCommerciale.Shared.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddServicesModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ProduitId",
                table: "FactureLignes",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<int>(
                name: "ServiceId",
                table: "FactureLignes",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ProduitId",
                table: "BonLivraisonLignes",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<int>(
                name: "ServiceId",
                table: "BonLivraisonLignes",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Services",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Reference = table.Column<string>(type: "TEXT", nullable: false),
                    Designation = table.Column<string>(type: "TEXT", nullable: false),
                    Unite = table.Column<string>(type: "TEXT", nullable: false),
                    PrixVenteHT = table.Column<decimal>(type: "TEXT", nullable: false),
                    CoutHT = table.Column<decimal>(type: "TEXT", nullable: false),
                    TauxTVA = table.Column<decimal>(type: "TEXT", nullable: false),
                    Actif = table.Column<bool>(type: "INTEGER", nullable: false),
                    Note = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Services", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FactureLignes_ServiceId",
                table: "FactureLignes",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_BonLivraisonLignes_ServiceId",
                table: "BonLivraisonLignes",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Services_Actif",
                table: "Services",
                column: "Actif");

            migrationBuilder.CreateIndex(
                name: "IX_Services_Reference",
                table: "Services",
                column: "Reference",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BonLivraisonLignes_Services_ServiceId",
                table: "BonLivraisonLignes",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FactureLignes_Services_ServiceId",
                table: "FactureLignes",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BonLivraisonLignes_Services_ServiceId",
                table: "BonLivraisonLignes");

            migrationBuilder.DropForeignKey(
                name: "FK_FactureLignes_Services_ServiceId",
                table: "FactureLignes");

            migrationBuilder.DropTable(
                name: "Services");

            migrationBuilder.DropIndex(
                name: "IX_FactureLignes_ServiceId",
                table: "FactureLignes");

            migrationBuilder.DropIndex(
                name: "IX_BonLivraisonLignes_ServiceId",
                table: "BonLivraisonLignes");

            migrationBuilder.DropColumn(
                name: "ServiceId",
                table: "FactureLignes");

            migrationBuilder.DropColumn(
                name: "ServiceId",
                table: "BonLivraisonLignes");

            migrationBuilder.AlterColumn<int>(
                name: "ProduitId",
                table: "FactureLignes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ProduitId",
                table: "BonLivraisonLignes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);
        }
    }
}
