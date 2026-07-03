using GestionCommerciale.Shared.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionCommerciale.Shared.Database.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260430120000_AddAppSettingsUiLanguage")]
public partial class AddAppSettingsUiLanguage : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "UiLanguage",
            table: "AppSettings",
            type: "TEXT",
            nullable: false,
            defaultValue: "fr");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "UiLanguage",
            table: "AppSettings");
    }
}
