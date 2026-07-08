using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionCommerciale.Shared.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceIdToAllDocumentLines : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ProduitId",
                table: "FactureFournisseurLignes",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<int>(
                name: "ServiceId",
                table: "FactureFournisseurLignes",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ProduitId",
                table: "DevisLignes",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<int>(
                name: "ServiceId",
                table: "DevisLignes",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ProduitId",
                table: "BonReceptionLignes",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<int>(
                name: "ServiceId",
                table: "BonReceptionLignes",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ProduitId",
                table: "BonCommandeLignes",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<int>(
                name: "ServiceId",
                table: "BonCommandeLignes",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ProduitId",
                table: "BonCommandeClientLignes",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<int>(
                name: "ServiceId",
                table: "BonCommandeClientLignes",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ProduitId",
                table: "AvoirLignes",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<int>(
                name: "ServiceId",
                table: "AvoirLignes",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ProduitId",
                table: "AvoirFournisseurLignes",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<int>(
                name: "ServiceId",
                table: "AvoirFournisseurLignes",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FactureFournisseurLignes_ServiceId",
                table: "FactureFournisseurLignes",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_DevisLignes_ServiceId",
                table: "DevisLignes",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_BonReceptionLignes_ServiceId",
                table: "BonReceptionLignes",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_BonCommandeLignes_ServiceId",
                table: "BonCommandeLignes",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_BonCommandeClientLignes_ServiceId",
                table: "BonCommandeClientLignes",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_AvoirLignes_ServiceId",
                table: "AvoirLignes",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_AvoirFournisseurLignes_ServiceId",
                table: "AvoirFournisseurLignes",
                column: "ServiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_AvoirFournisseurLignes_Services_ServiceId",
                table: "AvoirFournisseurLignes",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AvoirLignes_Services_ServiceId",
                table: "AvoirLignes",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BonCommandeClientLignes_Services_ServiceId",
                table: "BonCommandeClientLignes",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BonCommandeLignes_Services_ServiceId",
                table: "BonCommandeLignes",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BonReceptionLignes_Services_ServiceId",
                table: "BonReceptionLignes",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DevisLignes_Services_ServiceId",
                table: "DevisLignes",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FactureFournisseurLignes_Services_ServiceId",
                table: "FactureFournisseurLignes",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AvoirFournisseurLignes_Services_ServiceId",
                table: "AvoirFournisseurLignes");

            migrationBuilder.DropForeignKey(
                name: "FK_AvoirLignes_Services_ServiceId",
                table: "AvoirLignes");

            migrationBuilder.DropForeignKey(
                name: "FK_BonCommandeClientLignes_Services_ServiceId",
                table: "BonCommandeClientLignes");

            migrationBuilder.DropForeignKey(
                name: "FK_BonCommandeLignes_Services_ServiceId",
                table: "BonCommandeLignes");

            migrationBuilder.DropForeignKey(
                name: "FK_BonReceptionLignes_Services_ServiceId",
                table: "BonReceptionLignes");

            migrationBuilder.DropForeignKey(
                name: "FK_DevisLignes_Services_ServiceId",
                table: "DevisLignes");

            migrationBuilder.DropForeignKey(
                name: "FK_FactureFournisseurLignes_Services_ServiceId",
                table: "FactureFournisseurLignes");

            migrationBuilder.DropIndex(
                name: "IX_FactureFournisseurLignes_ServiceId",
                table: "FactureFournisseurLignes");

            migrationBuilder.DropIndex(
                name: "IX_DevisLignes_ServiceId",
                table: "DevisLignes");

            migrationBuilder.DropIndex(
                name: "IX_BonReceptionLignes_ServiceId",
                table: "BonReceptionLignes");

            migrationBuilder.DropIndex(
                name: "IX_BonCommandeLignes_ServiceId",
                table: "BonCommandeLignes");

            migrationBuilder.DropIndex(
                name: "IX_BonCommandeClientLignes_ServiceId",
                table: "BonCommandeClientLignes");

            migrationBuilder.DropIndex(
                name: "IX_AvoirLignes_ServiceId",
                table: "AvoirLignes");

            migrationBuilder.DropIndex(
                name: "IX_AvoirFournisseurLignes_ServiceId",
                table: "AvoirFournisseurLignes");

            migrationBuilder.DropColumn(
                name: "ServiceId",
                table: "FactureFournisseurLignes");

            migrationBuilder.DropColumn(
                name: "ServiceId",
                table: "DevisLignes");

            migrationBuilder.DropColumn(
                name: "ServiceId",
                table: "BonReceptionLignes");

            migrationBuilder.DropColumn(
                name: "ServiceId",
                table: "BonCommandeLignes");

            migrationBuilder.DropColumn(
                name: "ServiceId",
                table: "BonCommandeClientLignes");

            migrationBuilder.DropColumn(
                name: "ServiceId",
                table: "AvoirLignes");

            migrationBuilder.DropColumn(
                name: "ServiceId",
                table: "AvoirFournisseurLignes");

            migrationBuilder.AlterColumn<int>(
                name: "ProduitId",
                table: "FactureFournisseurLignes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ProduitId",
                table: "DevisLignes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ProduitId",
                table: "BonReceptionLignes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ProduitId",
                table: "BonCommandeLignes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ProduitId",
                table: "BonCommandeClientLignes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ProduitId",
                table: "AvoirLignes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ProduitId",
                table: "AvoirFournisseurLignes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);
        }
    }
}
