using GestionCommerciale.Shared.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionCommerciale.Shared.Database.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260430220000_AddSocieteMentionsLegales")]
public partial class AddSocieteMentionsLegales : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "SocieteMentionsLegales",
            table: "AppSettings",
            type: "TEXT",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "SocieteMentionsLegales",
            table: "AppSettings");
    }
}
